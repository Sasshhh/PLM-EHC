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

                Console.WriteLine("App Status: " + app.Status.Name);
                
                var lease = db.LeaseDetails
                    .Include(l => l.Status)
                    .FirstOrDefault(l => l.PropertyLeaseApplicationId == app.Id);

                if (lease != null) 
                {
                    Console.WriteLine("Lease Status: " + lease.Status.Name);
                }

                var rr = db.RoundRobins
                    .Include(r => r.ResponsibilityType)
                    .Include(r => r.SystemUser)
                    .Where(r => r.RecordId == app.Id && r.IsActive)
                    .OrderByDescending(r => r.Id)
                    .ToList();

                Console.WriteLine("Active RoundRobins:");
                foreach(var r in rr)
                {
                    Console.WriteLine($"Queue: {r.ResponsibilityType?.Name}, User: {r.SystemUser?.FullName}, Status: {r.IsActive}");
                }
            }
        }
    }
}
