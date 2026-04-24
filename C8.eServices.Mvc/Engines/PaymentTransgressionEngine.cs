using System;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Engines
{
    public class PaymentTransgressionEngine
    {
        private readonly eServicesDbContext _db;
        private readonly NotificationEngine _notificationEngine;

        public PaymentTransgressionEngine(eServicesDbContext db)
        {
            _db = db;
            _notificationEngine = new NotificationEngine(db);
        }

        /// <summary>
        /// Generates a unique case reference number for payment transgressions
        /// Format: PAYTG-YYYY-MM-XXXXXX
        /// </summary>
        public string GenerateCaseReferenceNumber()
        {
            var prefix = "PAYTG";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");

            var lastCaseNumber = _db.PaymentTransgressions
                .Where(pt => pt.CaseReferenceNumber.StartsWith($"{prefix}-{year}-{month}"))
                .OrderByDescending(pt => pt.CaseReferenceNumber)
                .Select(pt => pt.CaseReferenceNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCaseNumber))
            {
                var lastNumberPart = lastCaseNumber.Split('-').Last();
                if (int.TryParse(lastNumberPart, out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"{prefix}-{year}-{month}-{nextNumber.ToString("D6")}";
        }

        /// <summary>
        /// Retrieves tenant details by Official Number from PropertyLeaseApplication
        /// </summary>
        public dynamic GetTenantDetailsByOfficialNumber(string officialNumber)
        {
            var application = _db.PropertyLeaseApplications
                .Where(p => p.Customer.IdentificationNumber == officialNumber && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Select(p => new
                {
                    p.Customer.IdentificationNumber,
                    p.ApplicationReferenceNumber,
                    p.Customer.FirstName,
                    p.Customer.LastName,
                    Email = p.Customer.EmailAddress ?? "",
                    PhoneNumber = p.Customer.CellPhoneNumber ?? "",
                    ComplexId = 0, // TODO: Get from ApplicationAllocatedProperty
                    ComplexName = "",
                    BlockNumber = "",
                    UnitNumber = "",
                    CurrentAccountNumber = "", // TODO: Get account number
                    LastPaymentAmount = 0m, // TODO: Update with actual payment tracking
                    LastPaymentDate = (DateTime?)null,
                    TotalAmountDue = 0m // TODO: Calculate from billing system
                })
                .FirstOrDefault();

            return application;
        }

        /// <summary>
        /// Updates payment transgression status
        /// </summary>
        public void UpdateStatus(int paymentTransgressionId, string statusKey)
        {
            var transgression = _db.PaymentTransgressions.Find(paymentTransgressionId);
            if (transgression != null)
            {
                var status = _db.Status.FirstOrDefault(s => s.Key == statusKey);
                if (status != null)
                {
                    transgression.StatusId = status.Id;
                    transgression.ModifiedDateTime = DateTime.Now;
                    _db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Logs audit trail for payment transgression actions (BR27)
        /// </summary>
        public void LogAuditTrail(int paymentTransgressionId, string action, string details, int performedByCustomerId)
        {
            var auditLog = new PaymentTransgressionAuditLog
            {
                PaymentTransgressionId = paymentTransgressionId,
                Action = action,
                Details = details,
                PerformedByCustomerId = performedByCustomerId,
                PerformedAt = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now,
                DepartmentId = 1
            };

            _db.PaymentTransgressionAuditLogs.Add(auditLog);
            _db.SaveChanges();
        }

        /// <summary>
        /// Sends payment transgression notification to tenant
        /// </summary>
        public void SendTransgressionNotification(int paymentTransgressionId, string letterType)
        {
            var transgression = _db.PaymentTransgressions.Find(paymentTransgressionId);
            if (transgression == null) return;

            _notificationEngine.SendPaymentTransgressionNotification(transgression, letterType);
        }

        /// <summary>
        /// Gets count of payment transgressions for a tenant
        /// </summary>
        public int GetTransgressionCountForTenant(string officialNumber)
        {
            return _db.PaymentTransgressions
                .Count(pt => pt.OfficialNumber == officialNumber && pt.IsActive && !pt.IsDeleted);
        }

        /// <summary>
        /// Checks if tenant has reached critical transgression level (3+ transgressions)
        /// </summary>
        public bool HasReachedCriticalLevel(string officialNumber)
        {
            var count = GetTransgressionCountForTenant(officialNumber);
            return count >= 3;
        }

        /// <summary>
        /// Triggers lease termination for critical payment transgressions
        /// </summary>
        public void TriggerLeaseTermination(int paymentTransgressionId, string reason)
        {
            var transgression = _db.PaymentTransgressions.Find(paymentTransgressionId);
            if (transgression == null) return;

            // Find the lease via ApplicationAllocatedProperty
            var allocatedProperty = _db.ApplicationAllocatedProperty
                .FirstOrDefault(aap => aap.OfferedComplexId == transgression.ComplexId
                                    && aap.BuildingName == transgression.BlockNumber
                                    && aap.SpaceUnitNumber == transgression.UnitNumber
                                    && aap.IsActive && !aap.IsDeleted);

            if (allocatedProperty?.PropertyLeaseApplicationId != null)
            {
                var leaseDetails = _db.LeaseDetails
                    .FirstOrDefault(ld => ld.PropertyLeaseApplication.Id == allocatedProperty.PropertyLeaseApplicationId
                                       && ld.IsActive && !ld.IsDeleted);

                if (leaseDetails != null)
                {
                    var terminationStatus = _db.Status.FirstOrDefault(s => s.Key == "lease_termination_pending");
                    if (terminationStatus != null)
                    {
                        leaseDetails.StatusId = terminationStatus.Id;
                        leaseDetails.ModifiedDateTime = DateTime.Now;
                        _db.SaveChanges();

                        LogAuditTrail(paymentTransgressionId, "Lease Termination Triggered", reason, transgression.AssignedToCustomerId ?? 0);
                    }
                }
            }
        }
    }
}
