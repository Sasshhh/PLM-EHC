using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// DPRE UCXX - Lease register record (Capture Lease Details and Classify Lease Categories).
    /// Fields follow the SDS v0.1 UCXX Step 5 list and the Lease Dashboard "MASTER DATA REPORT" columns.
    /// Dropdown values live in RE_LeaseLookups so they can change once the BA confirms them (DPRE-Q refs there).
    /// </summary>
    [Table("RE_Leases")]
    public class RE_Lease : BaseModel
    {
        // ---- Lease / property identification ----
        [MaxLength(50)]  public string ContractReferenceNumber { get; set; }
        [Required, MaxLength(20)] public string LeaseReferenceNumber { get; set; }   // unique, e.g. LR000018
        [MaxLength(30)]  public string SolarAccountNumber { get; set; }
        [MaxLength(20)]  public string PropertyCode { get; set; }
        [MaxLength(20)]  public string HigherLevelPropertyCode { get; set; }
        [MaxLength(250)] public string PhysicalBuildingNumberAndName { get; set; }
        [MaxLength(250)] public string InvoiceClarification { get; set; }
        [MaxLength(250)] public string PhysicalStreetAddress { get; set; }
        [MaxLength(50)]  public string Cca { get; set; }                     // CCA name (from CCCId) used by the dashboard
        public int? CCCId { get; set; }
        [ForeignKey("CCCId")] public virtual CCC CCC { get; set; }

        // ---- Classification / purpose ----
        [MaxLength(60)]  public string LeaseClassification { get; set; }
        [MaxLength(60)]  public string SourceClassification { get; set; }    // value as received in the spreadsheet register
        public string LeaseDetailsAndPurpose { get; set; }
        public decimal? LeasedAreaSqm { get; set; }
        public bool AgreementInPlace { get; set; }
        [MaxLength(10)]  public string RecordStatus { get; set; }             // Active / Inactive
        public bool ArchibusInvoice { get; set; }
        public bool IsMemorandumOfAgreement { get; set; }

        // ---- Term ----
        [MaxLength(60)]  public string LeaseTerm { get; set; }
        public DateTime? LeaseStartDate { get; set; }
        public DateTime? EscalationDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public DateTime? LeaseTerminationDate { get; set; }
        public bool LeaseIndefinite { get; set; }
        public int? TerminationNoticePeriodMonths { get; set; }
        public bool Legal { get; set; }

        // ---- People ----
        [MaxLength(100)] public string PortfolioManager { get; set; }
        [MaxLength(100)] public string PropertyManager { get; set; }
        [MaxLength(100)] public string CollectionManager { get; set; }
        [MaxLength(100)] public string EscalationAlertsRecipient { get; set; }
        [MaxLength(100)] public string FinancialOfficer { get; set; }

        // ---- Financial ----
        [MaxLength(250)] public string EscalationTerms { get; set; }
        public decimal? EscalationPercentage { get; set; }
        public decimal? MonthlyRental { get; set; }
        [MaxLength(50)]  public string VenusRentalAccountNumber { get; set; }
        public decimal? DepositHeld { get; set; }
        [MaxLength(30)]  public string PaymentFrequency { get; set; }

        // ---- Tenant ----
        [MaxLength(60)]  public string ApplicantType { get; set; }
        [MaxLength(100)] public string ApplicantTypeOther { get; set; }
        [MaxLength(50)]  public string RegistrationNumber { get; set; }
        [MaxLength(20)]  public string VatRegistrationNumber { get; set; }
        [Required, MaxLength(150)] public string TenantName { get; set; }
        [MaxLength(20)]  public string TenantCode { get; set; }
        [MaxLength(150)] public string DomiciliumBuildingNumberAndName { get; set; }
        [MaxLength(150)] public string DomiciliumStreetAddress { get; set; }
        [MaxLength(60)]  public string DomiciliumSuburb { get; set; }
        [MaxLength(60)]  public string DomiciliumTown { get; set; }
        [MaxLength(10)]  public string DomiciliumPostalCode { get; set; }
        public string AdditionalComments { get; set; }

        // ---- Billing contact ----
        [MaxLength(30)]  public string ContactType { get; set; }              // Tenant / Authorised Representative
        [MaxLength(100)] public string BillingContactName { get; set; }
        [MaxLength(100)] public string BillingContactSurname { get; set; }
        [MaxLength(150)] public string AuthorisedRepresentative { get; set; }
        [MaxLength(150)] public string BillingBuildingNumberAndName { get; set; }
        [MaxLength(150)] public string BillingStreetAddress { get; set; }
        [MaxLength(60)]  public string BillingSuburb { get; set; }
        [MaxLength(60)]  public string BillingTown { get; set; }
        [MaxLength(10)]  public string BillingPostalCode { get; set; }
        [MaxLength(30)]  public string BillingMobile1 { get; set; }
        [MaxLength(30)]  public string BillingMobile2 { get; set; }
        [MaxLength(30)]  public string BillingTelephone1 { get; set; }
        [MaxLength(30)]  public string BillingTelephone2 { get; set; }
        [MaxLength(30)]  public string BillingFax1 { get; set; }
        [MaxLength(30)]  public string BillingFax2 { get; set; }
        [MaxLength(150)] public string BillingEmail1 { get; set; }
        [MaxLength(150)] public string BillingEmail2 { get; set; }

        // ---- Lease contact ----
        [MaxLength(100)] public string LeaseContactName { get; set; }
        [MaxLength(100)] public string LeaseContactSurname { get; set; }
        [MaxLength(30)]  public string LeaseContactMobile1 { get; set; }
        [MaxLength(30)]  public string LeaseContactMobile2 { get; set; }
        [MaxLength(30)]  public string LeaseContactTelephone1 { get; set; }
        [MaxLength(30)]  public string LeaseContactTelephone2 { get; set; }
        [MaxLength(30)]  public string LeaseContactFax1 { get; set; }
        [MaxLength(150)] public string LeaseContactEmail1 { get; set; }
        [MaxLength(150)] public string LeaseContactEmail2 { get; set; }

        // ---- Links / source ----
        public int? RE_ApplicationId { get; set; }                            // set when created from an approved application
        [MaxLength(30)]  public string Source { get; set; }                   // Capture / Import / Application

        /// <summary>Dashboard bucket: MOA, No Lease, Indefinite, Expired, Active or Inactive.</summary>
        [NotMapped]
        public string DashboardStatus { get { return RE_LeaseLookups.DeriveDashboardStatus(this, DateTime.Today); } }
    }
}
