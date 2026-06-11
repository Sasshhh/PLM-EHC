using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace C8.eServices.Mvc.Engines
{
    public class ComplaintSLAEngine
    {
        private readonly eServicesDbContext _db;
        private readonly NotificationEngine _notificationEngine;
        private const int SLA_DAYS = 7;

        public ComplaintSLAEngine(eServicesDbContext db)
        {
            _db = db;
            _notificationEngine = new NotificationEngine(db);
        }

        public List<TenantComplaint> GetOverdueComplaints()
        {
            var cutoffDate = DateTime.Now.AddDays(-SLA_DAYS);

            return _db.TenantComplaints
                .Where(tc => tc.DateSubmitted <= cutoffDate &&
                            tc.IsActive && !tc.IsDeleted &&
                            (tc.Status.Key == ComplaintStatusKeys.Submitted ||
                             tc.Status.Key == ComplaintStatusKeys.AwaitingAppointment ||
                             tc.Status.Key == ComplaintStatusKeys.AwaitingInvestigation))
                .ToList();
        }

        public int GetDaysOpen(TenantComplaint complaint)
        {
            if (complaint.DateSubmitted == null) return 0;
            return (DateTime.Now - complaint.DateSubmitted.Value).Days;
        }

        public bool IsOverdue(TenantComplaint complaint)
        {
            return GetDaysOpen(complaint) > SLA_DAYS;
        }

        public int GetDaysUntilSLABreach(TenantComplaint complaint)
        {
            if (complaint.DateSubmitted == null) return 0;
            var daysOpen = GetDaysOpen(complaint);
            return SLA_DAYS - daysOpen;
        }

        public string GetSLAStatus(TenantComplaint complaint)
        {
            var daysOpen = GetDaysOpen(complaint);

            if (daysOpen > SLA_DAYS)
                return "Overdue";
            else if (daysOpen >= SLA_DAYS - 2)
                return "At Risk";
            else
                return "On Track";
        }

        public Dictionary<string, int> GetSLAStatistics()
        {
            var activeComplaints = _db.TenantComplaints
                .Where(tc => tc.IsActive && !tc.IsDeleted &&
                            (tc.Status.Key == ComplaintStatusKeys.Submitted ||
                             tc.Status.Key == ComplaintStatusKeys.AwaitingAppointment ||
                             tc.Status.Key == ComplaintStatusKeys.AwaitingInvestigation))
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

        /// <summary>
        /// After 7 calendar days: email escalation to Revenue Manager.
        /// Called from Complaints Index or a scheduled task.
        /// </summary>
        public void CheckAndEscalateOverdueComplaints()
        {
            var overdueComplaints = GetOverdueComplaints();
            var workflowEngine = new ComplaintWorkflowEngine(_db);

            foreach (var complaint in overdueComplaints)
            {
                if (complaint.EscalationTriggered) continue;

                try
                {
                    complaint.EscalationTriggered = true;
                    complaint.EscalationDate = DateTime.Now;
                    complaint.ModifiedDateTime = DateTime.Now;

                    // Email Revenue Manager
                    SendEscalationToRevenueManager(complaint);

                    workflowEngine.LogAuditTrail(
                        complaint.Id,
                        "SLA Breach – Escalated to Revenue Manager",
                        $"Complaint exceeded {SLA_DAYS} calendar days. Escalation email sent to Revenue Manager.",
                        null);

                    _db.SaveChanges();
                }
                catch
                {
                    // Don't crash the page
                }
            }
        }

        /// <summary>
        /// Sends escalation email to Revenue Manager via AppSettings lookup.
        /// </summary>
        private bool SendEscalationToRevenueManager(TenantComplaint complaint)
        {
            try
            {
                var revenueManagerSetting = _db.AppSettings
                    .FirstOrDefault(x => x.Key == AppSettingKeys.RevenueManager);
                if (revenueManagerSetting == null || string.IsNullOrWhiteSpace(revenueManagerSetting.Value))
                    return false;

                var revenueManagerId = Convert.ToInt32(revenueManagerSetting.Value);
                if (revenueManagerId == 0) return false;

                var revenueManager = _db.Customers.Find(revenueManagerId);
                if (revenueManager == null) return false;

                var systemUser = _db.SystemUsers.FirstOrDefault(su => su.Id == revenueManager.SystemUserId);
                if (systemUser == null || string.IsNullOrWhiteSpace(systemUser.EmailAddress)) return false;

                var daysOpen = GetDaysOpen(complaint);
                var assignedOfficer = complaint.AssignedToId.HasValue
                    ? _db.Customers.Find(complaint.AssignedToId.Value)
                    : null;
                var assignedName = assignedOfficer != null
                    ? $"{assignedOfficer.FirstName} {assignedOfficer.LastName}"
                    : "Unassigned";

                var subject = $"SLA BREACH – Complaint {complaint.CaseReferenceNumber} ({daysOpen} days)";
                var body = $"The following complaint has exceeded the 7 calendar day SLA and requires your immediate attention.<br/><br/>" +
                           $"<strong>Case Reference:</strong> {complaint.CaseReferenceNumber}<br/>" +
                           $"<strong>Days Open:</strong> {daysOpen} calendar days<br/>" +
                           $"<strong>Assigned Officer:</strong> {assignedName}<br/>" +
                           $"<strong>Category:</strong> {complaint.ComplaintCategory?.Name ?? "N/A"}<br/>" +
                           $"<strong>Complainant:</strong> {complaint.ComplainantFirstName} {complaint.ComplainantSurname}<br/>" +
                           $"<strong>Date Submitted:</strong> {complaint.DateSubmitted?.ToString("dd MMMM yyyy")}<br/><br/>" +
                           $"<strong>Description:</strong><br/>{complaint.DetailedDescription}<br/><br/>" +
                           "Please take the necessary steps to resolve this matter urgently.<br/><br/>" +
                           "Regards,<br/>Property Lease Management System";

                _notificationEngine.QueueEscalationEmail(
                    systemUser.EmailAddress,
                    $"{revenueManager.FirstName} {revenueManager.LastName}",
                    subject,
                    body,
                    complaint.CaseReferenceNumber);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
