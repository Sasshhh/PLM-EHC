using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// DPRE UC05 Scenarios 2-4: the type-specific part of an Outdoor Advertising (DPRE-OA), Telecommunications (DPRE-TEL)
    /// or Concession/Leasehold (DPRE-LH: Social, Agricultural, Servitude) application. One row per RE_Application.
    /// Applicant, address, status and workflow stay on RE_Application so all lease types share the same back-office flow.
    /// </summary>
    [Table("RE_ApplicationSiteDetails")]
    public class RE_ApplicationSiteDetail : BaseModel
    {
        public int RE_ApplicationId { get; set; }
        [ForeignKey("RE_ApplicationId")]
        public virtual RE_Application Application { get; set; }

        [MaxLength(10)] public string FormCode { get; set; }                 // OA / TEL / LH

        // ---- Applicant (Section B) not already on RE_Application ----
        [MaxLength(100)] public string FirstName { get; set; }
        [MaxLength(100)] public string Surname { get; set; }
        [MaxLength(100)] public string ApplicantTypeOther { get; set; }
        [MaxLength(60)]  public string IdentificationType { get; set; }
        [MaxLength(500)] public string PostalAddress { get; set; }

        // ---- Outdoor Advertising (Scenario 2, Section A) ----
        [MaxLength(60)]  public string SignLocationType { get; set; }        // Individual ERF / Farm Portion / Road Reserve
        [MaxLength(60)]  public string SignType { get; set; }
        [MaxLength(100)] public string SignTypeOther { get; set; }
        public bool? SignIlluminated { get; set; }
        [MaxLength(40)]  public string SignElevations { get; set; }
        [MaxLength(60)]  public string SignElevationsOther { get; set; }
        [MaxLength(60)]  public string AdvertisingSignReferenceNumber { get; set; }
        [MaxLength(60)]  public string RealEstateReferenceNumber { get; set; }
        [MaxLength(60)]  public string CityPlanningReferenceNumber { get; set; }
        [MaxLength(250)] public string FarmPortionDescription { get; set; }
        [MaxLength(60)]  public string Town { get; set; }
        [MaxLength(150)] public string RoadReserveStreetName { get; set; }
        [MaxLength(150)] public string RoadReserveNearestIntersection { get; set; }
        [MaxLength(250)] public string RoadReserveAdjacentProperty { get; set; }

        // ---- Telecommunications (Scenario 3, Sections A and C) ----
        [MaxLength(150)] public string SiteName { get; set; }
        [MaxLength(60)]  public string SiteNumber { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [MaxLength(120)] public string InstallationType { get; set; }
        [MaxLength(150)] public string InstallationTypeOther { get; set; }
        public decimal? AreaOnStructureSqm { get; set; }
        public decimal? AreaGroundSqm { get; set; }
        public bool? OptionToRenew { get; set; }
        public bool? CurrentlySublet { get; set; }
        public int? CurrentCoLocators { get; set; }
        public bool? WillBeSublet { get; set; }
        public int? AnticipatedCoLocators { get; set; }

        // ---- Telecommunications and Concession/Leasehold ----
        [MaxLength(100)] public string Zoning { get; set; }
        public int? ProposedLeaseYears { get; set; }
        public int? ProposedLeaseMonths { get; set; }

        // ---- Concession/Leasehold (Scenario 4, Section A) ----
        public string ProposedUse { get; set; }
        public decimal? TotalExtentSqm { get; set; }

        // ---- Terms and Conditions + Submission (all three) ----
        public bool TermsAccepted { get; set; }
        public DateTime? TermsAcceptedDateTime { get; set; }
        [MaxLength(150)] public string SignatoryName { get; set; }
        [MaxLength(100)] public string SignatoryCapacity { get; set; }
        public string SignatureImage { get; set; }                           // PNG data URL from the signature pad
        public DateTime? SubmittedDateTime { get; set; }
    }
}
