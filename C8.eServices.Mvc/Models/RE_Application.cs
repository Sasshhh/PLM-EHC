using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("RE_Applications")]
    public class RE_Application : BaseModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Application Reference Number")]
        public string ApplicationReferenceNumber { get; set; }

        [Required]
        public int SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public virtual SystemUser SystemUser { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Applicant Type")]
        public string ApplicantType { get; set; }

        // Entity Details
        [MaxLength(250)]
        [Display(Name = "Entity/Business Name")]
        public string EntityName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Company Registration Number")]
        public string CompanyRegistrationNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "VAT Registration Number")]
        public string VatRegistrationNumber { get; set; }

        [MaxLength(100)]
        [Display(Name = "Tax Reference Number")]
        public string TaxReferenceNumber { get; set; }

        [MaxLength(500)]
        [Display(Name = "Entity Registered Address")]
        public string EntityRegisteredAddress { get; set; }

        [MaxLength(20)]
        [Display(Name = "Entity Registered Postal Code")]
        public string EntityRegisteredPostalCode { get; set; }

        [MaxLength(250)]
        [Display(Name = "Authorized Representative Name")]
        public string AuthorizedRepresentativeName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Authorized Representative Capacity")]
        public string AuthorizedRepresentativeCapacity { get; set; }

        [MaxLength(50)]
        [Display(Name = "Entity Telephone Number")]
        public string EntityTelephone { get; set; }

        [MaxLength(50)]
        [Display(Name = "Entity Mobile Number")]
        public string EntityMobile { get; set; }

        [MaxLength(50)]
        [Display(Name = "Entity Fax Number")]
        public string EntityFax { get; set; }

        [MaxLength(150)]
        [Display(Name = "Entity Email Address")]
        public string EntityEmail { get; set; }

        // Banking Details
        [MaxLength(100)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Bank Account Type")]
        public string BankAccountType { get; set; }

        [MaxLength(150)]
        [Display(Name = "Bank Account Holder Name")]
        public string BankAccountName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Bank Account Number")]
        public string BankAccountNumber { get; set; }

        [MaxLength(50)]
        [Display(Name = "Branch Code")]
        public string BankBranchCode { get; set; }

        // Lease Details
        [Required]
        [MaxLength(100)]
        [Display(Name = "Purpose of Lease")]
        public string PurposeOfLease { get; set; }

        [Required]
        public int CCCId { get; set; }
        [ForeignKey("CCCId")]
        public virtual CCC CCC { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Erf/Farm Number")]
        public string ErfFarmNumber { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Physical Address of space to let")]
        public string PropertyAddress { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Township/Suburb/Farm Name")]
        public string TownshipSuburbFarmName { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Postal Code")]
        public string PropertyPostalCode { get; set; }

        // Facility Types
        [Display(Name = "Outdoor Advertising")]
        public bool FacilityOutdoorAdvertising { get; set; }

        [Display(Name = "Telecommunications (e.g. cell masts)")]
        public bool FacilityTelecommunications { get; set; }

        [Display(Name = "Informal Trading Facility and Kiosk")]
        public bool FacilityInformalTrading { get; set; }

        [Display(Name = "Taxi Rank Trading Facility and Kiosk")]
        public bool FacilityTaxiRankTrading { get; set; }

        [Display(Name = "Township Vocational Skills Development Centre")]
        public bool FacilityVocationalSkills { get; set; }

        [Display(Name = "Computer Training Centre")]
        public bool FacilityComputerTraining { get; set; }

        [Display(Name = "Township Industrial Park")]
        public bool FacilityIndustrialPark { get; set; }

        [Display(Name = "Township Business Hub")]
        public bool FacilityBusinessHub { get; set; }

        [Display(Name = "Township Automotive Hub")]
        public bool FacilityAutomotiveHub { get; set; }

        [Display(Name = "Township Agri-Park Facility")]
        public bool FacilityAgriPark { get; set; }

        [Display(Name = "Municipal Incubation Farm")]
        public bool FacilityIncubationFarm { get; set; }

        [Required]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }

        public int? SelectedFacilityId { get; set; }
        [ForeignKey("SelectedFacilityId")]
        public virtual RE_Facility SelectedFacility { get; set; }

        public int? SelectedFacilityUnitId { get; set; }
        [ForeignKey("SelectedFacilityUnitId")]
        public virtual RE_FacilityUnit SelectedFacilityUnit { get; set; }

        [Display(Name = "Number of Units")]
        public int? SelectedUnitCount { get; set; }

        [Display(Name = "Calculated Monthly Rental")]
        public decimal? CalculatedMonthlyRental { get; set; }

        [Display(Name = "Selected Units JSON")]
        public string SelectedUnitsJson { get; set; }

        // Risk Assessment & Payment Validation properties
        public string CreditBureauResult { get; set; }
        public string HomeAffairsResult { get; set; }
        public string DeedsResult { get; set; }
        public string SassaResult { get; set; }
        public string CipcResult { get; set; }
        public string RiskAssessmentRecommendation { get; set; }
        public string RiskAssessmentReason { get; set; }
        public int? RiskAssessmentEvidenceFileId { get; set; }
        [ForeignKey("RiskAssessmentEvidenceFileId")]
        public virtual File RiskAssessmentEvidenceFile { get; set; }
        public string PaymentValidationComment { get; set; }

        // Committee Phase
        public string CommitteeDecision { get; set; }
        public string CommitteeComments { get; set; }
        public int? CommitteeResolutionFileId { get; set; }
        [ForeignKey("CommitteeResolutionFileId")]
        public virtual File CommitteeResolutionFile { get; set; }

        // Final Authorization Phase
        public string FinalOutcome { get; set; }
        public string FinalComments { get; set; }
        public string FinalSignature { get; set; }

        // Inspection Phase
        public string InspectionType { get; set; }
        public DateTime? InspectionDate { get; set; }
        public string InspectionTime { get; set; }
        public string InspectionStatus { get; set; }
        public string InspectionComments { get; set; }
        public string InspectionPlumbing { get; set; }
        public string InspectionElectrical { get; set; }
        public string InspectionFixtures { get; set; }
        public string InspectionSanitation { get; set; }
        public string InspectionHazards { get; set; }
        public string InspectionWearTear { get; set; }
        public int? InspectionFormFileId { get; set; }
        [ForeignKey("InspectionFormFileId")]
        public virtual File InspectionFormFile { get; set; }

        // Works Orders / Maintenance
        public string WorkOrderNumber { get; set; }
        public string WorkOrderStatus { get; set; }
        public string WorkOrderTasks { get; set; }
        public string WorkOrderIssueDescription { get; set; }
        public string WorkOrderPriority { get; set; }
        public DateTime? WorkOrderDueDate { get; set; }
        public string WorkOrderMaterials { get; set; }
        public string WorkOrderSafetyInstructions { get; set; }
        public string WorkOrderAssignmentType { get; set; }
        public string WorkOrderTechnicianName { get; set; }
        public string WorkOrderRejectionReason { get; set; }
        public int? WorkOrderJobSheetFileId { get; set; }
        [ForeignKey("WorkOrderJobSheetFileId")]
        public virtual File WorkOrderJobSheetFile { get; set; }
        public string WorkOrderManagerComments { get; set; }
        public string WorkOrderManagerSignature { get; set; }

        // Permission to Occupy (PTO)
        public string PtoReferenceNumber { get; set; }
        public string PtoPurposeOfOccupation { get; set; }
        public DateTime? PtoStartDate { get; set; }
        public DateTime? PtoEndDate { get; set; }
        public bool? PtoAcceptedIndemnity { get; set; }
        public string PtoStatus { get; set; }
        public string PtoReviewRecommendation { get; set; }
        public string PtoReviewReason { get; set; }
        public string PtoDecision { get; set; }
        public string PtoDecisionReason { get; set; }
        public string PtoSignature { get; set; }

        // Use Cases 21 to 25 properties
        public string PtoRevocationReason { get; set; }
        public DateTime? PtoRevocationDate { get; set; }

        public int? LeaseAgreementFileId { get; set; }
        [ForeignKey("LeaseAgreementFileId")]
        public virtual File LeaseAgreementFile { get; set; }

        public int? LeaseAgreementSignedFileId { get; set; }
        [ForeignKey("LeaseAgreementSignedFileId")]
        public virtual File LeaseAgreementSignedFile { get; set; }

        public DateTime? LeaseAgreementTenantSignatureDate { get; set; }
        public string LeaseAgreementHodSignature { get; set; }
        public DateTime? LeaseAgreementHodSignatureDate { get; set; }

        public string LeaseCategory { get; set; }
        public string UniqueTenancyLeaseNumber { get; set; }
        public DateTime? LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public DateTime? LeaseDateOfOccupation { get; set; }
        public string LeaseEscalationTerms { get; set; }
        public string LeasePaymentFrequency { get; set; }
        public decimal? LeaseDepositAmount { get; set; }
        public string LeaseStatus { get; set; }
    }
}
