using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace C8.eServices.Mvc.Engines
{
    public class ComplaintSLAEngine
    {
        private readonly eServicesDbContext _db;
        private const int SLA_DAYS_NON_COMPLIANCE = 7;
        private const int SLA_DAYS_NON_MAINTENANCE = 7;

        public ComplaintSLAEngine(eServicesDbContext db)
        {
            _db = db;
        }

        public List<TenantComplaint> GetOverdueComplaints()
        {
            var slaDays = SLA_DAYS_NON_COMPLIANCE;
            var cutoffDate = DateTime.Now.AddDays(-slaDays);

            return _db.TenantComplaints
                .Where(tc => tc.DateSubmitted <= cutoffDate &&
                            tc.IsActive && !tc.IsDeleted &&
                            (tc.Status.Key == "complaint_status_submitted" ||
                             tc.Status.Key == "complaint_status_awaiting_investigation"))
                .ToList();
        }

        public int GetDaysOpen(TenantComplaint complaint)
        {
            if (complaint.DateSubmitted == null) return 0;
            return (DateTime.Now - complaint.DateSubmitted.Value).Days;
        }

        public bool IsOverdue(TenantComplaint complaint)
        {
            return GetDaysOpen(complaint) > SLA_DAYS_NON_COMPLIANCE;
        }

        public int GetDaysUntilSLABreach(TenantComplaint complaint)
        {
            if (complaint.DateSubmitted == null) return 0;
            var daysOpen = GetDaysOpen(complaint);
            return SLA_DAYS_NON_COMPLIANCE - daysOpen;
        }

        public string GetSLAStatus(TenantComplaint complaint)
        {
            var daysOpen = GetDaysOpen(complaint);

            if (daysOpen > SLA_DAYS_NON_COMPLIANCE)
                return "Overdue";
            else if (daysOpen >= SLA_DAYS_NON_COMPLIANCE - 2)
                return "At Risk";
            else
                return "On Track";
        }

        public Dictionary<string, int> GetSLAStatistics()
        {
            var activeComplaints = _db.TenantComplaints
                .Where(tc => tc.IsActive && !tc.IsDeleted &&
                            (tc.Status.Key == "complaint_status_submitted" ||
                             tc.Status.Key == "complaint_status_awaiting_investigation"))
                .ToList();

            var stats = new Dictionary<string, int>
            {
                { "Total", activeComplaints.Count },
                { "OnTrack", 0 },
                { "AtRisk", 0 },
                { "Overdue", 0 }
            };

            foreach (var complaint in activeComplaints)
            {
                var status = GetSLAStatus(complaint);
                if (status == "On Track") stats["OnTrack"]++;
                else if (status == "At Risk") stats["AtRisk"]++;
                else if (status == "Overdue") stats["Overdue"]++;
            }

            return stats;
        }
    }
}
