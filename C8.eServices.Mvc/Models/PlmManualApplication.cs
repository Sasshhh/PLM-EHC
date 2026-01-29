using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PlmManualApplication : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Applicant Type")]
        public int? PurchaserTypeId { get; set; }
        [ForeignKey("PurchaserTypeId")]
        public PurchaserType PurchaserType { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "Marital Status")]
        [Column(Order = 12)]
        [StringLength(20)]
        public string MaritalStatus { get; set; }

        [Display(Name = "Initials")]
        [Column(Order = 13)]
        [StringLength(10)]
        public string Initial { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 14)]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 15)]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 16)]
        [StringLength(25)]
        public string IDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 17)]
        public DateTime? DOB { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 18)]
        [StringLength(15)]
        public string CellNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Home number is not valid")]
        [Display(Name = "Home Number")]
        [Column(Order = 19)]
        [StringLength(15)]
        public string HomeNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Work number is not valid")]
        [Display(Name = "Work Number")]
        [Column(Order = 20)]
        [StringLength(15)]
        public string WorkNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 21)]
        [StringLength(50)]
        public string PurEmail { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 22)]
        [StringLength(50)]
        public string ResAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 23)]
        [StringLength(50)]
        public string ResSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 24)]
        [StringLength(10)]
        public string ResPostal { get; set; }

        [Column(Order = 25)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SassaNumber { get; set; }
        [Display(Name = "Combined Gross Income")]
        [Column(Order = 26)]
        public decimal? GrossIncome { get; set; }

        [Display(Name = "Combined Net Income")]
        [Column(Order = 27)]
        public decimal? NetIncome { get; set; }

        [Display(Name = "Total Combined Income")]
        [Column(Order = 28)]
        public decimal? TotalCombinedIncome { get; set; }

        [Display(Name = "House Required ")]
        [Column(Order = 29)]
        [StringLength(50)]
        public string HouseRequired { get; set; }
        [Display(Name = "Preferred Complex/Area")]
        [Column(Order = 30)]
        //[StringLength(20)]
        public string PrefArea { get; set; }

        [Display(Name = "Company Name")]
        [Column(Order = 31)]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Display(Name = "Name Trading As")]
        [Column(Order = 32)]
        [StringLength(100)]
        public string NameTradingAs { get; set; }

        [Display(Name = "Trust Reg No")]
        [Column(Order = 33)]
        [StringLength(100)]
        public string TrustRegNo { get; set; }

        [Display(Name = "Trust Name")]
        [Column(Order = 34)]
        [StringLength(100)]
        public string TrustName { get; set; }



        [Display(Name = "Company Type")]
        [Column(Order = 35)]
        [StringLength(100)]
        public string CompanyType { get; set; }



        [Display(Name = "CIPC Registration No")]
        [Column(Order = 36)]
        [StringLength(100)]
        public string CIPCRegistrationNo { get; set; }




        [Display(Name = "Street")]
        [Column(Order = 37)]
        [StringLength(50)]
        public string CoAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 38)]
        [StringLength(50)]
        public string CoSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 39)]
        [StringLength(10)]
        public string CoPostal { get; set; }


        [Display(Name = "Name")]
        [Column(Order = 40)]
        [StringLength(300)]
        public string DirectorName { get; set; }



        [Display(Name = "Surname")]
        [Column(Order = 41)]
        [StringLength(300)]
        public string DirectorSName { get; set; }


        [Display(Name = "ID No")]
        [Column(Order = 42)]
        [StringLength(300)]
        public string DirectorIdNo { get; set; }


        [Display(Name = "Surname")]
        [Column(Order = 43)]
        [StringLength(200)]
        public string ContactPSurname { get; set; }

        [Display(Name = "Name")]
        [Column(Order = 44)]
        [StringLength(200)]
        public string ContactPName { get; set; }

        [Display(Name = "Capacity")]
        [Column(Order = 45)]
        [StringLength(300)]
        public string Capacity { get; set; }

        [Display(Name = "Identity Number")]
        [Column(Order = 46)]
        [StringLength(300)]
        public string CPIdNumber { get; set; }


        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 47)]
        [StringLength(15)]
        public string CPCellNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Home number is not valid")]
        [Display(Name = "Contact Number (Home)")]
        [Column(Order = 48)]
        [StringLength(15)]
        public string CPHomeNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Work number is not valid")]
        [Display(Name = "Contact Number (Work)")]
        [Column(Order = 49)]
        [StringLength(15)]
        public string CPWorkNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 50)]
        [StringLength(50)]
        public string CPEmail1 { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 51)]
        [StringLength(50)]
        public string CPEmail2 { get; set; }

        [Display(Name = "Type")]
        [Column(Order = 108)]
        public int? BType { get; set; }

        [Display(Name = "Name of the Building ")]
        [Column(Order = 109)]
        [StringLength(50)]
        public string BName { get; set; }


        [Display(Name = "Address")]
        [Column(Order = 110)]
        [StringLength(50)]
        public string BAddress { get; set; }

        [Display(Name = "Street")]
        [Column(Order = 111)]
        [StringLength(50)]
        public string BStreet { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 112)]
        [StringLength(50)]
        public string BSuburb { get; set; }

        [Display(Name = "Envisaged Usage  ")]
        [Column(Order = 113)]
        public int? BUsage { get; set; }
        [Column(Order = 114)]
        public string HouseRequiredOption2 { get; set; }
        [Column(Order = 115)]
        public string PrefAreaOption2 { get; set; }

        [Column(Order = 116)]
        [Display(Name = "Application Reference Number")]
        [MaxLength(50)]
        public string ApplicationReferenceNumber { get; set; }

        [Column(Order = 117)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 118)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 119)]
        [Display(Name = "SystemUser")]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }
        [Column(Order = 120)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 121)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexArea2Id { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea2 { get; set; }
        [Column(Order = 122)]
        [Display(Name = "SystemUser")]
        public int? HumanEHCOptionsId { get; set; }
        [ForeignKey("HumanEHCOptionsId")]
        public HumanEHCOptions HumanEHCOptions { get; set; }

        [Column(Order = 123)]
        [Display(Name = "Income Source")]
        public int? IncomeSourceId { get; set; }
        [ForeignKey("IncomeSourceId")]
        public IncomeSource IncomeSource { get; set; }
        [Column(Order = 124)]
        [Display(Name = "HousingType")]
        public string HousingType { get; set; }
        [Display(Name = "Nationality")]
        [Column(Order = 125)]
        public int? IdentificationTypeId { get; set; }
        [ForeignKey("IdentificationTypeId")]
        public IdentificationType IdentificationType { get; set; }
        [Column(Order = 126)]
        [Display(Name = "Applicant Name")]
        public string ApplicantFullName { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 127)]
        public int? TitleTypeId { get; set; }
        public TitleType TitleType { get; set; }

        //Second Applicant Details
        [Column(Order = 128)]
        [Display(Name = "Second Applicant")]
        public bool? SecondApplicant { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 129)]
        [StringLength(50)]
        public string SecAppFirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 130)]
        [StringLength(50)]
        public string SecAppLastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 131)]
        [StringLength(25)]
        public string SecAppIDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 132)]
        public DateTime? SecAppDOB { get; set; }


        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 133)]
        [StringLength(15)]
        public string SecAppCellNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 134)]
        [StringLength(50)]
        public string SecAppEmail { get; set; }

        [Display(Name = "Combined Gross Income")]
        [Column(Order = 135)]
        public decimal? SecAppGrossIncome { get; set; }

        [Display(Name = "Combined Net Income")]
        [Column(Order = 136)]
        public decimal? SecAppNetIncome { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 137)]
        public string SecAppAddress { get; set; }
        [Display(Name = "Suburb")]
        [Column(Order = 138)]
        public string SecAppSuburb { get; set; }

        [Display(Name = "Postal")]
        [Column(Order = 139)]

        public string SecAppPostal { get; set; }

        [Column(Order = 140)]
        [Display(Name = "Income Source")]
        public int? SecAppIncomeSourceId { get; set; }
        [ForeignKey("SecAppIncomeSourceId")]
        public IncomeSource SecAppIncomeSource { get; set; }

        [Column(Order = 141)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SecAppSassaNumber { get; set; }

        [Column(Order = 142)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string SecAppGender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 143)]
        public int? SecAppTitleTypeId { get; set; }
        [ForeignKey("SecAppTitleTypeId")]
        public TitleType SecAppTitleType { get; set; }


        [Display(Name = "Committee Date")]
        [Column(Order = 144)]
        public DateTime? CommitteeDate { get; set; }

        [Display(Name = "Serve Notice")]
        [Column(Order = 145)]
        public DateTime? ServeNoticeDate { get; set; }

        //New Columns
        [Display(Name = "Property Code")]
        [Column(Order = 146)]
        public string PropertyCode { get; set; }

        [Display(Name = "Property Code Id")]
        [Column(Order = 147)]
        public int? PropertyCodeId { get; set; }

        ////
        [Column(Order = 148)]
        [Display(Name = "Solar Reference")]
        public String SolarReference { get; set; }
        [Column(Order = 149)]
        [Display(Name = "Space Unit Number")]
        public String UnitNumber { get; set; }
        [Display(Name = "Tenant Code")]
        [Column(Order = 150)]
        public string TenantCode { get; set; }


        [Display(Name = "Lease Start Date")]
        [Column(TypeName = "date", Order = 151)]
        public DateTime? LeaseStartDate { get; set; }
        [Display(Name = "Lease End Date")]
        [Column(TypeName = "date", Order = 152)]
        public DateTime? LeaseEndDate { get; set; }


        [Column(Order = 153)]
        public string InActiveLease { get; set; }

        [Column(Order = 154)]
        public string Rejected { get; set; }
        [Column(Order = 155)]
        public string Processed { get; set; }

        [Column(Order = 156)]
        public int? NumberOfOccupants { get; set; }
        [Column(Order = 157)]
        public string NumberOfBedroom { get; set; }

        [Column(Order = 158)]
        public int? HumanEHCOptionsIdFinalDecision { get; set; }

        [Column(Order = 159)]
        public Double? RentalCode { get; set; }  //MonthlyRentalAmount
        [Column(Order = 160)]
        public Double? DepositAmount { get; set; }  //RequiedDepositAmount

    }
}