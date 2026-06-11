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

            var searchPrefix = $"{prefix}-{year}-{month}";

            var lastCaseNumber = _db.PaymentTransgressions
                .Where(pt => pt.CaseReferenceNumber.StartsWith(searchPrefix))
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
            var leaseDetails = _db.LeaseDetails
                .Where(ld => ld.PropertyLeaseApplication != null && ld.PropertyLeaseApplication.Customer != null &&
                             ld.PropertyLeaseApplication.Customer.IdentificationNumber == officialNumber &&
                             ld.IsActive && !ld.IsDeleted)
                .OrderByDescending(ld => ld.Id)
                .Select(ld => new
                {
                    IdentificationNumber = ld.PropertyLeaseApplication.Customer.IdentificationNumber,
                    ApplicationReferenceNumber = ld.PropertyLeaseApplication.ApplicationReferenceNumber,
                    FirstName = ld.PropertyLeaseApplication.Customer.FirstName,
                    LastName = ld.PropertyLeaseApplication.Customer.LastName,
                    Email = ld.PropertyLeaseApplication.Customer.EmailAddress ?? "",
                    PhoneNumber = ld.PropertyLeaseApplication.Customer.CellPhoneNumber ?? "",
                    ComplexId = 0,
                    ComplexName = ld.OfficeParkName ?? ld.buildingName,
                    BlockNumber = ld.buildingName ?? "",
                    UnitNumber = ld.SpaceUnitNo ?? "",
                    CurrentAccountNumber = "",
                    LastPaymentAmount = 0m,
                    LastPaymentDate = (DateTime?)null,
                    TotalAmountDue = 0m
                })
                .FirstOrDefault();

            return leaseDetails;
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

        /// <summary>
        /// Searches for tenants with an active lease based on a query
        /// </summary>
        public object SearchTenants(string query)
        {
            var baseQuery = _db.LeaseDetails
                .Where(ld => ld.IsActive && !ld.IsDeleted && ld.PropertyLeaseApplication != null && ld.PropertyLeaseApplication.Customer != null);

            if (!string.IsNullOrWhiteSpace(query))
            {
                baseQuery = baseQuery.Where(ld => 
                    (ld.PropertyLeaseApplication.Customer.FirstName != null && ld.PropertyLeaseApplication.Customer.FirstName.Contains(query)) ||
                    (ld.PropertyLeaseApplication.Customer.LastName != null && ld.PropertyLeaseApplication.Customer.LastName.Contains(query)) ||
                    (ld.PropertyLeaseApplication.Customer.IdentificationNumber != null && ld.PropertyLeaseApplication.Customer.IdentificationNumber.Contains(query)) ||
                    (ld.PropertyLeaseApplication.ApplicationReferenceNumber != null && ld.PropertyLeaseApplication.ApplicationReferenceNumber.Contains(query))
                );
            }

            var activeLeases = baseQuery
                .OrderByDescending(ld => ld.Id)
                .Select(ld => new
                {
                    OfficialNumber = ld.PropertyLeaseApplication.Customer.IdentificationNumber,
                    Name = ld.PropertyLeaseApplication.Customer.FirstName,
                    Surname = ld.PropertyLeaseApplication.Customer.LastName,
                    TenancyRef = ld.PropertyLeaseApplication.ApplicationReferenceNumber,
                    Complex = ld.OfficeParkName ?? ld.buildingName
                })
                .Take(20)
                .ToList();

            var uniqueTenants = activeLeases
                .GroupBy(t => t.OfficialNumber)
                .Select(g => g.First())
                .Take(10)
                .ToList();

            return uniqueTenants;
        }
    }
}
