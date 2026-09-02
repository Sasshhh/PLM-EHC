using System;
using System.Linq;
using System.Data.Entity;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace CheckState
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new eServicesDbContext())
            {
                var app = db.PropertyLeaseApplications
                    .Include(a => a.Status)
                    .FirstOrDefault(a => a.ApplicationReferenceNumber == "EHC2023102300002" || a.LeaseReferenceNo == "EHC2023102300002");
                
                if (app == null) 
                {
                    Console.WriteLine("App not found.");
                    return;
                }

                var lease = db.LeaseDetails
                    .FirstOrDefault(l => l.PropertyLeaseApplicationId == app.Id);

                if (lease != null) 
                {
                    // Move to AwaitingLeaseRenewalAgreementConclusion
                    var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.AwaitingLeaseRenewalAgreementConclusion);
                    if (targetStatus != null)
                    {
                        lease.StatusId = targetStatus.Id;
                        app.StatusId = targetStatus.Id;
                        
                        // Add to RoundRobinQueues for LeaseRenewals
                        var resp = db.ResponsibilityTypes.FirstOrDefault(r => r.Key == ResponsibilityTypeKeys.LeaseRenewals);
                        var submittedStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.Submitted);
                        
                        // Need to know Ashkay's clerk id. Assuming LettingOfficer logic from MatchingHelper:
                        var complex = db.PreferredComplexAreas.FirstOrDefault(x => x.Id == app.PreferredComplexAreaId);
                        var storedCsoId = Convert.ToInt16(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.LettingOfficer)?.Value ?? "0");
                        int clerkId = (complex != null && complex.LettingOfficerId.HasValue) ? complex.LettingOfficerId.Value : storedCsoId;

                        Console.WriteLine("Assigning to Clerk ID: " + clerkId);

                        var queue = new C8.eServices.Mvc.Models.RoundRobinQueue
                        {
                            PropertyLeaseApplicationId = app.Id,
                            LeaseDetailsId = lease.Id,
                            ResponsibilityTypeId = resp.Id,
                            CurrentTaskDateTime = DateTime.Now,
                            ClerkId = clerkId,
                            StatusId = submittedStatus.Id
                        };
                        db.RoundRobinQueues.Add(queue);
                        
                        db.SaveChanges();
                        Console.WriteLine("Successfully moved application to Renewals inbox.");
                    }
                }
            }
        }
    }
}
