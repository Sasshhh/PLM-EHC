using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Linq;

namespace C8.eServices.Mvc.Engines
{
    public class ComplaintWorkflowEngine
    {
        private readonly eServicesDbContext _db;
        private readonly NotificationEngine _notificationEngine;

        public ComplaintWorkflowEngine(eServicesDbContext db)
        {
            _db = db;
            _notificationEngine = new NotificationEngine(db);
        }

        public Customer RoundRobinComplaints(int complaintId, string responsibilityTypeKey)
        {
            var complaint = _db.TenantComplaints.Find(complaintId);
            if (complaint == null) return null;

            var responsibilityType = _db.ResponsibilityTypes
                .FirstOrDefault(r => r.Key == responsibilityTypeKey);
            if (responsibilityType == null) return null;

            var complex = _db.PreferredComplexAreas.FirstOrDefault(x => x.Id == complaint.ComplainantComplexId);
            var storedCsoId = Convert.ToInt16(
                _db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.LettingOfficer)?.Value ?? "0");
            var clerkId = (complex != null && complex.LettingOfficerId.HasValue)
                ? complex.LettingOfficerId.Value
                : storedCsoId;

            if (clerkId == 0) return null;

            var cso = _db.Customers.Find(clerkId);
            if (cso == null) return null;

            var statusId = _db.Status.FirstOrDefault(s => s.Key == StatusKeys.Submitted)?.Id ?? 1;

            var roundRobinQueue = new RoundRobinQueue
            {
                PropertyLeaseApplicationId = null,
                TenantComplaintId = complaintId,
                ResponsibilityTypeId = responsibilityType.Id,
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = clerkId,
                StatusId = statusId
            };
            _db.RoundRobinQueues.Add(roundRobinQueue);

            complaint.AssignedToId = clerkId;
            complaint.DateAssigned = DateTime.Now;
            _db.SaveChanges();

            _notificationEngine.SendAssignmentNotification(complaint, cso);
            return cso;
        }

        public void RoundRobinMarkFinished(int clerkId, string responsibilityTypeKey, int? complaintId = null)
        {
            var responsibilityType = _db.ResponsibilityTypes
                .FirstOrDefault(r => r.Key == responsibilityTypeKey);

            if (responsibilityType == null) return;

            var archivedStatusId = _db.Status.FirstOrDefault(x => x.Key == StatusKeys.Archived).Id;

            var query = _db.RoundRobinQueues
                .Where(r => r.ResponsibilityTypeId == responsibilityType.Id
                         && r.PropertyLeaseApplicationId == null
                         && r.StatusId != archivedStatusId);

            if (complaintId.HasValue)
            {
                query = query.Where(r => r.TenantComplaintId == complaintId.Value);
            }
            else
            {
                query = query.Where(r => r.ClerkId == clerkId);
            }

            var entry = query.OrderBy(r => r.Id).FirstOrDefault();

            if (entry == null) return;

            entry.StatusId = archivedStatusId;
            entry.EndTaskDateTime = DateTime.Now;
            entry.ModifiedDateTime = DateTime.Now;
            _db.SaveChanges();
        }

        public bool UpdateComplaintStatus(int complaintId, string statusKey)
        {
            try
            {
                var complaint = _db.TenantComplaints.Find(complaintId);
                if (complaint == null) return false;

                var status = _db.Status.FirstOrDefault(s => s.Key == statusKey);
                if (status == null) return false;

                complaint.StatusId = status.Id;
                complaint.ModifiedDateTime = DateTime.Now;
                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int CountWarningLetters(int respondentComplexId, string respondentBlockNumber, string respondentUnitNumber)
        {
            return _db.TenantComplaints
                .Count(tc => tc.RespondentComplexId == respondentComplexId &&
                            tc.RespondentBlockNumber == respondentBlockNumber &&
                            tc.RespondentUnitNumber == respondentUnitNumber &&
                            tc.IsActive && !tc.IsDeleted);
        }

        public bool TriggerLeaseTermination(TenantComplaint complaint)
        {
            try
            {
                var allocatedProperty = _db.ApplicationAllocatedProperty
                    .Where(aap => aap.OfferedComplexId == complaint.RespondentComplexId &&
                                 aap.SpaceUnitNumber == complaint.RespondentUnitNumber &&
                                 (string.IsNullOrEmpty(aap.BuildingName) || aap.BuildingName == complaint.RespondentBlockNumber) &&
                                 aap.IsActive && !aap.IsDeleted)
                    .OrderByDescending(aap => aap.Id)
                    .FirstOrDefault();

                if (allocatedProperty == null) return false;

                var matchedUnit = _db.MatchedUnits
                    .Where(mu => mu.ApplicationAllocatedPropertyId == allocatedProperty.Id
                              && mu.PropertyLeaseApplicationId != null)
                    .OrderByDescending(mu => mu.Id)
                    .FirstOrDefault();

                if (matchedUnit?.PropertyLeaseApplicationId == null) return false;

                var lease = _db.LeaseDetails
                    .Where(ld => ld.PropertyLeaseApplicationId == matchedUnit.PropertyLeaseApplicationId &&
                                 ld.IsActive && !ld.IsDeleted)
                    .OrderByDescending(ld => ld.Id)
                    .FirstOrDefault();

                if (lease == null) return false;

                var terminationStatus = _db.Status.FirstOrDefault(s => s.Key == "lease_termination_pending");
                if (terminationStatus == null) return false;

                lease.StatusId = terminationStatus.Id;
                lease.ModifiedDateTime = DateTime.Now;
                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsSubLettingComplaint(TenantComplaint complaint)
        {
            if (complaint.ComplaintType == null) return false;
            return complaint.ComplaintType.Key == ComplaintTypeKeys.SubLetting;
        }

        public bool CheckAndEnforceSubLettingPolicy(TenantComplaint complaint)
        {
            if (!IsSubLettingComplaint(complaint)) return false;

            var investigation = complaint.Investigations.FirstOrDefault();
            if (investigation?.Outcome == ComplaintOutcomeKeys.Resolved)
            {
                return TriggerLeaseTermination(complaint);
            }

            return false;
        }

        public void LogAuditTrail(int complaintId, string action, string details, int? performedByCustomerId)
        {
            try
            {
                var auditLog = new ComplaintAuditLog
                {
                    TenantComplaintId = complaintId,
                    Action = action,
                    Details = details,
                    PerformedByCustomerId = performedByCustomerId,
                    PerformedAt = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.Now,
                    DepartmentId = 1
                };

                _db.ComplaintAuditLogs.Add(auditLog);
                _db.SaveChanges();
            }
            catch
            {
            }
        }
    }
}
