using System;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// Handles all lease renewal processing logic.
    /// IMPORTANT: No method in this service may modify LeaseDetails.EndDate, 
    /// LeaseDetails.RenewalNotice or LeaseDetails.TerminationNotice.
    /// Those fields are updated ONLY in UpdatePropertyLeaseDates(), called exclusively
    /// from LeaseOfferValidation POST when the tenant accepts.
    /// </summary>
    public static class LeaseRenewalService
    {
        /// <summary>
        /// Called at CSO recommendation step.
        /// Creates or updates the PropertyLeaseRenewalOffer staging record with the
        /// proposed months and pre-calculated proposed dates.
        /// Does NOT touch LeaseDetails date fields.
        /// </summary>
        public static PropertyLeaseRenewalOffer CreateOrUpdateOffer(
            eServicesDbContext db,
            int propertyLeaseApplicationId,
            int leaseId,
            int months,
            string csoOutcome,
            string csoComment,
            int csoSystemUserId)
        {
            var lease = db.LeaseDetails.FirstOrDefault(x => x.Id == leaseId);
            if (lease == null) throw new Exception("Lease not found: " + leaseId);

            // Calculate proposed dates from current lease dates
            DateTime baseEnd         = lease.EndDate.HasValue         ? lease.EndDate.Value         : DateTime.Now;
            DateTime baseRenewal     = lease.RenewalNotice.HasValue   ? lease.RenewalNotice.Value   : DateTime.Now;
            DateTime baseTermination = lease.TerminationNotice.HasValue ? lease.TerminationNotice.Value
                                       : (lease.TerminationDate.HasValue ? lease.TerminationDate.Value : DateTime.Now);

            var offer = db.PropertyLeaseRenewalOffers
                          .FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplicationId);

            if (offer == null)
            {
                offer = new PropertyLeaseRenewalOffer
                {
                    PropertyLeaseApplicationId = propertyLeaseApplicationId,
                    IsActive  = true,
                    IsDeleted = false,
                    IsLocked  = false,
                    CreatedDateTime = DateTime.Now
                };
                db.PropertyLeaseRenewalOffers.Add(offer);
            }

            offer.MonthsOffer               = months;
            offer.ProposedEndDate           = baseEnd.AddMonths(months);
            offer.ProposedRenewalNotice     = baseRenewal.AddMonths(months);
            offer.ProposedTerminationNotice = baseTermination.AddMonths(months);
            offer.CSO_Outcome               = csoOutcome;
            offer.CSO_Comment               = csoComment;
            offer.CSO_Date                  = DateTime.Now;
            offer.CSO_SystemUserId          = csoSystemUserId;
            offer.ModifiedDateTime          = DateTime.Now;

            db.SaveChanges();
            return offer;
        }

        /// <summary>
        /// Records the Revenue Manager's decision on the existing offer record.
        /// Does NOT touch LeaseDetails.
        /// </summary>
        public static void RecordRMDecision(
            eServicesDbContext db,
            int propertyLeaseApplicationId,
            string outcome,
            string comment,
            int rmSystemUserId)
        {
            var offer = db.PropertyLeaseRenewalOffers
                          .FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplicationId);
            if (offer == null) return;

            offer.RM_Outcome       = outcome;
            offer.RM_Comment       = comment;
            offer.RM_Date          = DateTime.Now;
            offer.RM_SystemUserId  = rmSystemUserId;
            offer.ModifiedDateTime = DateTime.Now;
            db.SaveChanges();
        }

        /// <summary>
        /// Records the Director/CEO's decision on the existing offer record.
        /// Does NOT touch LeaseDetails.
        /// </summary>
        public static void RecordCEODecision(
            eServicesDbContext db,
            int propertyLeaseApplicationId,
            string outcome,
            string comment,
            int ceoSystemUserId)
        {
            var offer = db.PropertyLeaseRenewalOffers
                          .FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplicationId);
            if (offer == null) return;

            offer.CEO_Outcome      = outcome;
            offer.CEO_Comment      = comment;
            offer.CEO_Date         = DateTime.Now;
            offer.CEO_SystemUserId = ceoSystemUserId;
            offer.ModifiedDateTime = DateTime.Now;
            db.SaveChanges();
        }

        /// <summary>
        /// Records the customer's decline reason on the offer record.
        /// Called when the customer rejects the renewal offer.
        /// Does NOT touch LeaseDetails.
        /// </summary>
        public static void RecordCustomerDecline(
            eServicesDbContext db,
            int propertyLeaseApplicationId,
            string declineReason)
        {
            var offer = db.PropertyLeaseRenewalOffers
                          .FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplicationId);
            if (offer == null) return;

            offer.CustomerDeclineReason = declineReason;
            offer.CustomerResponseDate  = DateTime.Now;
            offer.IsAccepted            = false;
            offer.ModifiedDateTime      = DateTime.Now;
            db.SaveChanges();
        }

        /// <summary>
        /// Returns the active (not yet accepted) offer for a given application.
        /// </summary>
        public static PropertyLeaseRenewalOffer GetActiveOffer(eServicesDbContext db, int propertyLeaseApplicationId)
        {
            return db.PropertyLeaseRenewalOffers
                     .FirstOrDefault(x => x.PropertyLeaseApplicationId == propertyLeaseApplicationId
                                       && x.IsAccepted != true);
        }
    }
}
