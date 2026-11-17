using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// EHC waiting list -> unit offers. One engine for Waiting List > Run Auto-Allocate,
    /// Waiting List > Manual Allocate and the automatic match when anyone opens an Inbox.
    ///
    /// Rules:
    ///  * Queue = PropertyLeaseWaitingLists rows with QueueStatus "Waiting", FIFO by DateAdded.
    ///  * A unit is free when ApplicationAllocatedProperties.IsTaken = 0 (active, not deleted).
    ///  * Match = same unit type (HumanEHCOptionId) in the applicant's Option 1 complex,
    ///    else in their Option 2 complex. A unit the applicant rejected before is never re-offered.
    ///  * Each queue (complex + unit type) is independent: a person waiting for Chris Hani
    ///    3 Bedroom never holds up Airport Park 1 Bedroom.
    ///
    /// Robustness:
    ///  * The plan is built from 3 read-only queries in memory (no per-unit queries).
    ///  * Each offer is its own short transaction in its own context, and the unit is claimed with
    ///    an atomic UPDATE ... WHERE IsTaken = 0, so a failure or a parallel run skips just that one.
    ///  * E-mails go out after the offers are committed, on a worker with a time limit,
    ///    so a slow or unreachable mail database cannot hold up the screen.
    /// </summary>
    public static class UnitMatchingEngine
    {
        public const string Waiting = "Waiting";
        public const string Offered = "Offered";

        private static readonly object Gate = new object();
        private static DateTime _lastAutomaticRun = DateTime.MinValue;
        private static readonly TimeSpan AutomaticInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan NotificationBudget = TimeSpan.FromSeconds(15);

        public class RunResult
        {
            public int Offered { get; set; }
            public int Failed { get; set; }
            public bool AlreadyRunning { get; set; }
            public List<string> Log { get; } = new List<string>();
            /// <summary>Application id -> unit number offered in this run.</summary>
            public Dictionary<int, string> OfferedUnits { get; } = new Dictionary<int, string>();
            public override string ToString() { return string.Join(Environment.NewLine, Log); }
        }

        private class Plan
        {
            public int WaitingListId;
            public int UnitId;
        }

        /// <summary>Called on every Inbox load: at most once a minute, never waits for another run.</summary>
        public static void RunIfDue(int systemUserId)
        {
            if (DateTime.Now - _lastAutomaticRun < AutomaticInterval) return;
            if (!Monitor.TryEnter(Gate)) return;
            try
            {
                _lastAutomaticRun = DateTime.Now;
                RunLocked(systemUserId, "Unit Auto-Allocated (waiting list)");
            }
            catch (Exception ex)
            {
                SafeLog("UnitMatchingEngine.RunIfDue: " + ex.Message);
            }
            finally
            {
                Monitor.Exit(Gate);
            }
        }

        /// <summary>Waiting List > Run Auto-Allocate.</summary>
        public static RunResult Run(int systemUserId)
        {
            if (!Monitor.TryEnter(Gate, TimeSpan.FromSeconds(20)))
                return new RunResult { AlreadyRunning = true };
            try
            {
                _lastAutomaticRun = DateTime.Now;
                return RunLocked(systemUserId, "Unit Auto-Allocated");
            }
            finally
            {
                Monitor.Exit(Gate);
            }
        }

        private static RunResult RunLocked(int systemUserId, string how)
        {
            var result = new RunResult();
            var plan = BuildPlan(result);
            var offeredApps = new List<int>();

            foreach (var p in plan)
            {
                try
                {
                    string unitNo;
                    int appId = OfferUnitCore(p.WaitingListId, p.UnitId, systemUserId, how, out unitNo);
                    offeredApps.Add(appId);
                    result.OfferedUnits[appId] = unitNo;
                    result.Offered++;
                    result.Log.Add(string.Format("Unit {0} offered to application id {1}.", unitNo, appId));
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Log.Add(string.Format("Waiting-list entry {0}, unit {1}: skipped ({2}).", p.WaitingListId, p.UnitId, Inner(ex).Message));
                }
            }

            NotifyApplicants(offeredApps);
            if (result.Log.Count == 0) result.Log.Add("Nothing to match: no waiting applicant has a free unit of their type in their preferred complexes.");
            return result;
        }

        private static List<Plan> BuildPlan(RunResult result)
        {
            using (var db = new eServicesDbContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var waiting = db.PropertyLeaseWaitingLists.AsNoTracking()
                    .Where(w => w.QueueStatus == Waiting && w.IsActive && !w.IsDeleted)
                    .OrderBy(w => w.DateAdded).ThenBy(w => w.Id)
                    .Select(w => new
                    {
                        w.Id,
                        w.PropertyLeaseApplicationId,
                        w.PreferredComplexId,
                        w.PreferredTypologyId,
                        Complex2 = w.PropertyLeaseApplication.PreferredComplexArea2Id
                    })
                    .ToList();
                if (waiting.Count == 0) return new List<Plan>();

                var free = db.ApplicationAllocatedProperty.AsNoTracking()
                    .Where(u => !u.IsTaken && u.IsActive && !u.IsDeleted && u.HumanEHCOptionId != null)
                    .OrderBy(u => u.Id)
                    .Select(u => new { u.Id, u.OfferedComplexId, Option = u.HumanEHCOptionId.Value })
                    .ToList();

                var appIds = waiting.Select(w => (int?)w.PropertyLeaseApplicationId).Distinct().ToList();
                var rejected = new HashSet<string>(db.MatchedUnits.AsNoTracking()
                    .Where(m => m.RejectedProperty && appIds.Contains(m.PropertyLeaseApplicationId) && m.ApplicationAllocatedPropertyId != null)
                    .Select(m => new { App = m.PropertyLeaseApplicationId.Value, Unit = m.ApplicationAllocatedPropertyId.Value })
                    .ToList()
                    .Select(x => x.App + ":" + x.Unit));

                var pools = free.GroupBy(u => u.OfferedComplexId + ":" + u.Option)
                                .ToDictionary(g => g.Key, g => new LinkedList<int>(g.Select(u => u.Id)));

                var plan = new List<Plan>();
                foreach (var w in waiting)
                {
                    int? unitId = Take(pools, w.PreferredComplexId + ":" + w.PreferredTypologyId, w.PropertyLeaseApplicationId, rejected);
                    if (unitId == null && w.Complex2.HasValue && w.Complex2.Value != w.PreferredComplexId)
                        unitId = Take(pools, w.Complex2.Value + ":" + w.PreferredTypologyId, w.PropertyLeaseApplicationId, rejected);
                    if (unitId != null) plan.Add(new Plan { WaitingListId = w.Id, UnitId = unitId.Value });
                }
                result.Log.Add(string.Format("{0} waiting, {1} free units, {2} matches planned.", waiting.Count, free.Count, plan.Count));
                return plan;
            }
        }

        private static int? Take(Dictionary<string, LinkedList<int>> pools, string key, int appId, HashSet<string> rejected)
        {
            LinkedList<int> pool;
            if (!pools.TryGetValue(key, out pool)) return null;
            for (var node = pool.First; node != null; node = node.Next)
            {
                if (rejected.Contains(appId + ":" + node.Value)) continue;
                pool.Remove(node);
                return node.Value;
            }
            return null;
        }

        /// <summary>Waiting List > Manual Allocate > Offer this unit. Returns a message for the screen.</summary>
        public static string OfferUnit(int waitingListId, int unitId, int systemUserId, string how)
        {
            string unitNo;
            int appId;
            lock (Gate)
            {
                appId = OfferUnitCore(waitingListId, unitId, systemUserId, how, out unitNo);
            }
            NotifyApplicants(new List<int> { appId });
            return unitNo;
        }

        private static int OfferUnitCore(int waitingListId, int unitId, int systemUserId, string how, out string unitNo)
        {
            using (var db = new eServicesDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                var entry = db.PropertyLeaseWaitingLists.FirstOrDefault(w => w.Id == waitingListId);
                if (entry == null || entry.QueueStatus != Waiting)
                    throw new InvalidOperationException("the applicant is no longer waiting");

                // Claim the unit atomically; 0 rows means someone else took it first.
                int claimed = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.ApplicationAllocatedProperties SET IsTaken = 1, AllocatedByUserId = @p0, ModifiedDateTime = GETDATE() WHERE Id = @p1 AND IsTaken = 0",
                    systemUserId, unitId);
                if (claimed == 0) throw new InvalidOperationException("the unit was just taken");

                var unit = db.ApplicationAllocatedProperty.AsNoTracking().Where(u => u.Id == unitId)
                             .Select(u => new { u.SpaceUnitNumber }).FirstOrDefault();
                unitNo = unit != null && !string.IsNullOrWhiteSpace(unit.SpaceUnitNumber) ? unit.SpaceUnitNumber.Trim() : unitId.ToString();

                var app = db.PropertyLeaseApplications.Include(a => a.Status).First(a => a.Id == entry.PropertyLeaseApplicationId);
                var awaited = db.Status.Where(s => s.Key == StatusKeys.awaited).OrderBy(s => s.Id).FirstOrDefault();
                if (awaited == null) throw new InvalidOperationException("status " + StatusKeys.awaited + " is missing");

                db.MatchedUnits.Add(new MatchedUnits
                {
                    PropertyLeaseApplicationId = app.Id,
                    LeaseReferenceNo = app.ApplicationReferenceNumber,
                    ApplicationAllocatedPropertyId = unitId,
                    IsAccepted = false,
                    RejectedProperty = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBySystemUserId = systemUserId,
                    CreatedDateTime = DateTime.Now
                });

                entry.QueueStatus = Offered;
                entry.OfferedUnitId = unitId;
                entry.ModifiedDateTime = DateTime.Now;

                foreach (var q in db.waitingListQues.Where(q => q.PropertyLeaseApplicationId == app.Id && !q.IsMatched).ToList())
                    q.IsMatched = true;

                string from = app.Status != null ? app.Status.Name : "";
                app.StatusId = awaited.Id;

                int historyUser = db.Customers.Where(c => c.SystemUserId == systemUserId).Select(c => c.Id).FirstOrDefault();
                db.PLMApplicationHistortyLogs.Add(new PLMApplicationHistortyLog
                {
                    PropertyLeaseApplicationId = app.Id,
                    AuditAction = string.Format("{0}: unit {1} offered. Status changed from {2} to {3}.", how, unitNo, from, awaited.Name),
                    UserId = historyUser > 0 ? historyUser : app.CustomerId,
                    CreatedBySystemUserId = systemUserId,
                    CreatedDateTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                });

                db.SaveChanges();
                tx.Commit();
                return app.Id;
            }
        }

        /// <summary>
        /// "You have been matched to a unit" e-mail/SMS. Runs on a worker and the caller waits at most
        /// NotificationBudget; a failure is logged and never undoes the offer.
        /// </summary>
        private static void NotifyApplicants(List<int> appIds)
        {
            if (appIds == null || appIds.Count == 0) return;
            var http = HttpContext.Current;   // the audit code in SaveChanges reads the request
            var work = Task.Run(() =>
            {
                HttpContext.Current = http;
                foreach (var appId in appIds)
                {
                    try
                    {
                        using (var db = new eServicesDbContext())
                        {
                            int emailId = db.EmailContentTypes.Where(x => x.Key == EmailContentKeys.AtUnitMatchApplication)
                                                              .Select(x => x.Id).FirstOrDefault();
                            if (emailId > 0) EmailHelper.CustomerEmailNotification(db, appId, emailId);
                        }
                    }
                    catch (Exception ex)
                    {
                        SafeLog("Unit offer notification for application " + appId + " failed: " + Inner(ex).Message);
                    }
                }
            });
            try { work.Wait(NotificationBudget); } catch { /* logged inside */ }
        }

        private static Exception Inner(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex;
        }

        private static void SafeLog(string message)
        {
            try { EventLogHelper.LogSystemError(message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog); }
            catch { }
        }
    }
}
