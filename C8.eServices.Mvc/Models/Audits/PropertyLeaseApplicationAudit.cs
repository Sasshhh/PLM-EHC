using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class PropertyLeaseApplicationAudit : BaseModelAudit
    {

        [Column(Order = 10)]
        [Display(Name = "Application Reference Number")]
        [MaxLength(50)]
        public string ApplicationReferenceNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }



        [Column(Order = 12)]
        [Display(Name = "Applicant Type")]
        public int? PurchaserTypeId { get; set; }
        [ForeignKey("PurchaserTypeId")]
        public PurchaserType PurchaserType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 14)]
        public int? TitleTypeId { get; set; }
        public TitleType TitleType { get; set; }

        [Display(Name = "Marital Status")]
        [Column(Order = 15)]
        [StringLength(20)]
        public string MaritalStatus { get; set; }

        [Display(Name = "Initials")]
        [Column(Order = 16)]
        [StringLength(10)]
        public string Initial { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 17)]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 18)]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 19)]
        [StringLength(25)]
        public string IDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 20)]
        public DateTime? DOB { get; set; }



        [Display(Name = "Nationality")]
        [Column(Order = 24)]
        public int? IdentificationTypeId { get; set; }
        [ForeignKey("IdentificationTypeId")]
        public IdentificationType IdentificationType { get; set; }


        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 27)]
        [StringLength(15)]
        public string CellNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Home number is not valid")]
        [Display(Name = "Home Number")]
        [Column(Order = 28)]
        [StringLength(15)]
        public string HomeNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Work number is not valid")]
        [Display(Name = "Work Number")]
        [Column(Order = 29)]
        [StringLength(15)]
        public string WorkNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 30)]
        [StringLength(50)]
        public string PurEmail { get; set; }



        [Display(Name = "Address")]
        [Column(Order = 35)]
        [StringLength(50)]
        public string ResAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 36)]
        [StringLength(50)]
        public string ResSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 37)]
        [StringLength(10)]
        public string ResPostal { get; set; }


        [Column(Order = 45)]
        [Display(Name = "Income Source")]
        public int IncomeSourceId { get; set; }
        //[ForeignKey("IncomeSourceId")]
        //public IncomeSource IncomeSource { get; set; }

        [Column(Order = 46)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SassaNumber { get; set; }

        //[Display(Name = "Property Information")]
        //[Column(Order = 47)]
        //public int? PropertyInfo { get; set; }

        [Display(Name = "Gross Income")]
        [Column(Order = 48)]
        public decimal GrossIncome { get; set; }

        [Display(Name = "Net Income")]
        [Column(Order = 49)]
        public decimal NetIncome { get; set; }

        [Display(Name = "Total Combined Income")]
        [Column(Order = 50)]
        public decimal TotalCombinedIncome { get; set; }

        [Display(Name = "House Required ")]
        [Column(Order = 51)]
        [StringLength(50)]
        public string HouseRequired { get; set; }
        public string HouseRequiredOption2 { get; set; }





        [Display(Name = "Preferred Complex/Area")]
        [Column(Order = 52)]
        [StringLength(20)]
        public string PrefArea { get; set; }
        public string PrefAreaOption2 { get; set; }

        [Column(Order = 53)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 54)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexArea2Id { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea2 { get; set; }




        [Display(Name = "Company Name")]
        [Column(Order = 57)]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Display(Name = "Name Trading As")]
        [Column(Order = 58)]
        [StringLength(100)]
        public string NameTradingAs { get; set; }

        [Display(Name = "Trust Reg No")]
        [Column(Order = 59)]
        [StringLength(100)]
        public string TrustRegNo { get; set; }

        [Display(Name = "Trust Name")]
        [Column(Order = 60)]
        [StringLength(100)]
        public string TrustName { get; set; }



        [Display(Name = "Company Type")]
        [Column(Order = 62)]
        [StringLength(100)]
        public string CompanyType { get; set; }



        [Display(Name = "CIPC Registration No")]
        [Column(Order = 64)]
        [StringLength(100)]
        public string CIPCRegistrationNo { get; set; }




        [Display(Name = "Street")]
        [Column(Order = 65)]
        [StringLength(50)]
        public string CoAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 66)]
        [StringLength(50)]
        public string CoSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 67)]
        [StringLength(10)]
        public string CoPostal { get; set; }


        [Display(Name = "Name")]
        [Column(Order = 68)]
        [StringLength(300)]
        public string DirectorName { get; set; }



        [Display(Name = "Surname")]
        [Column(Order = 69)]
        [StringLength(300)]
        public string DirectorSName { get; set; }


        [Display(Name = "ID No")]
        [Column(Order = 70)]
        [StringLength(300)]
        public string DirectorIdNo { get; set; }

        //CONTACT PERSON


        [Display(Name = "Surname")]
        [Column(Order = 71)]
        [StringLength(200)]
        public string ContactPSurname { get; set; }

        [Display(Name = "Name")]
        [Column(Order = 72)]
        [StringLength(200)]
        public string ContactPName { get; set; }

        [Display(Name = "Capacity")]
        [Column(Order = 73)]
        [StringLength(300)]
        public string Capacity { get; set; }

        [Display(Name = "Identity Number")]
        [Column(Order = 74)]
        [StringLength(300)]
        public string CPIdNumber { get; set; }


        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 75)]
        [StringLength(15)]
        public string CPCellNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Home number is not valid")]
        [Display(Name = "Contact Number (Home)")]
        [Column(Order = 76)]
        [StringLength(15)]
        public string CPHomeNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Work number is not valid")]
        [Display(Name = "Contact Number (Work)")]
        [Column(Order = 77)]
        [StringLength(15)]
        public string CPWorkNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 78)]
        [StringLength(50)]
        public string CPEmail1 { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 79)]
        [StringLength(50)]
        public string CPEmail2 { get; set; }

        //APPLICATION SPACE

        [Display(Name = "Type")]
        [Column(Order = 81)]
        public int? BType { get; set; }

        [Display(Name = "Name of the Building ")]
        [Column(Order = 82)]
        [StringLength(50)]
        public string BName { get; set; }


        [Display(Name = "Address")]
        [Column(Order = 83)]
        [StringLength(50)]
        public string BAddress { get; set; }

        [Display(Name = "Street")]
        [Column(Order = 84)]
        [StringLength(50)]
        public string BStreet { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 85)]
        [StringLength(50)]
        public string BSuburb { get; set; }

        [Display(Name = "Envisaged Usage  ")]
        [Column(Order = 86)]
        public int? BUsage { get; set; }


        [Column(Order = 87)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 88)]
        [Display(Name = "SystemUser")]
        public int SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }

        [Column(Order = 89)]
        [Display(Name = "SystemUser")]
        public int? HumanEHCOptionsId { get; set; }
        [ForeignKey("HumanEHCOptionsId")]
        public HumanEHCOptions HumanEHCOptions { get; set; }

        [Column(Order = 90)]
        [Display(Name = "HousingType")]
        public string HousingType { get; set; }

        [Column(Order = 91)]
        [Display(Name = "Applicant Name")]
        public string ApplicantFullName { get; set; }



        //Second Applicant Details
        [Column(Order = 92)]
        [Display(Name = "Second Applicant")]
        public bool SecondApplicant { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 93)]
        [StringLength(50)]
        public string SecAppFirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 94)]
        [StringLength(50)]
        public string SecAppLastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 95)]
        [StringLength(25)]
        public string SecAppIDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 96)]
        public DateTime? SecAppDOB { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 97)]
        [StringLength(15)]
        public string SecAppCellNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 98)]
        [StringLength(50)]
        public string SecAppEmail { get; set; }

        [Display(Name = "Gross Income")]
        [Column(Order = 99)]
        public decimal SecAppGrossIncome { get; set; }

        [Display(Name = "Net Income")]
        [Column(Order = 108)]
        public decimal SecAppNetIncome { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 109)]
        public string SecAppAddress { get; set; }

        [Display(Name = "Postal")]
        [Column(Order = 110)]

        public string SecAppPostal { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 111)]
        public string SecAppSuburb { get; set; }

        [Column(Order = 112)]
        [Display(Name = "Income Source")]
        public int? SecAppIncomeSourceId { get; set; }


        [Column(Order = 113)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SecAppSassaNumber { get; set; }

        [Column(Order = 114)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string SecAppGender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 115)]
        public int? SecAppTitleTypeId { get; set; }

        [Display(Name = "Committee Date")]
        [Column(Order = 116)]
        public DateTime? CommitteeDate { get; set; }


        [Display(Name = "Serve Notice")]
        [Column(Order = 117)]
        public DateTime? ServeNoticeDate { get; set; }


        [Display(Name = "Serve Notice")]
        [Column(Order = 118)]
        public int? RoundRobinQueueId { get; set; }

        [Display(Name = "How long living/leasing/owning the above address")]
        [Column(Order = 119)]
        public string LivingAddressPeriod { get; set; }

        [Display(Name = "Landlord/Agent/Family")]
        [Column(Order = 120)]
        public string AgentName { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Agent number is not valid")]
        [Display(Name = "Tel")]
        [Column(Order = 121)]
        [StringLength(15)]
        public string AgentPhoneNumber { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Company number is not valid")]
        [Display(Name = "Tel")]
        [Column(Order = 122)]
        [StringLength(15)]
        public string CoPhoneNumber { get; set; }

        [Display(Name = "Fax")]
        [Column(Order = 123)]
        public string CoFax { get; set; }

        [Display(Name = "Period Working")]
        [Column(Order = 124)]
        public string CoPeriodWorking { get; set; }

        [Display(Name = "Consent Date")]
        [Column(Order = 125)]
        public DateTime? ConsentDate { get; set; }

        [Display(Name = "RefundProcessPassword")]
        [Column(Order = 126)]
        public string RefundProcessPassword { get; set; }
        [Display(Name = "IsMigrated")]
        [Column(Order = 127)]
        public bool IsMigrated { get; set; }
        //
        [Display(Name = "How long living/leasing/owning the above address")]
        public bool PreUnitInspectionCompleted { get; set; }
        public bool PreUnitMaintenanceRequired { get; set; }
        [Display(Name = "How long living/leasing/owning the above address")]
        public bool ExitInspectionCompleted { get; set; }
        public bool ExitUnitMaintenanceRequired { get; set; }

        [Display(Name = "Present Employer (Company)")]
        [Column(Order = 128)]
        public string PresentEmployer { get; set; }
        [Display(Name = "Employer Address")]
        [Column(Order = 129)]
        public string PresentEmployerAddress { get; set; }
        [Display(Name = "PeriodWorking")]
        [Column(Order = 130)]
        public string PresentEmployerPeriodWorking { get; set; }
        [Display(Name = "Occupation/Title/If Self Employed")]
        [Column(Order = 131)]
        public string PresentEmployerOccupation { get; set; }

        [Display(Name = "Present Employer (Company)")]
        [Column(Order = 132)]
        public string SecPresentEmployer { get; set; }
        [Display(Name = "Employer Address")]
        [Column(Order = 133)]
        public string SecPresentEmployerAddress { get; set; }
        [Display(Name = "PeriodWorking")]
        [Column(Order = 134)]
        public string SecPresentEmployerPeriodWorking { get; set; }
        [Display(Name = "Occupation/Title/If Self Employed")]
        [Column(Order = 135)]
        public string SecPresentEmployerOccupation { get; set; }

        [Display(Name = "Date Of Occupation")]
        [Column(TypeName = "date", Order = 136)]
        public DateTime? DateOfOccupation { get; set; }
        [Display(Name = "Parking Required")]
        [Column(Order = 137)]
        public string ParkingRequired { get; set; }
        [Display(Name = "Car Registration")]
        [Column(Order = 138)]
        public string CarRegistration { get; set; }

        [Display(Name = "Enable DSTV Service")]
        [Column(Order = 139)]
        public bool? HasDSTV { get; set; }

        [Display(Name = "DSTV Activation Fee")]
        [Column(Order = 140, TypeName = "decimal")]
        public decimal? DSTVActivationFee { get; set; }

        [Display(Name = "DSTV Monthly Levy")]
        [Column(Order = 141, TypeName = "decimal")]
        public decimal? DSTVMonthlyLevy { get; set; }

        [Display(Name = "Access Card Deposit")]
        [Column(Order = 142, TypeName = "decimal")]
        public decimal? AccessCardDeposit { get; set; }

        [Display(Name = "Key Deposit")]
        [Column(Order = 143, TypeName = "decimal")]
        public decimal? KeyDeposit { get; set; }

        public bool IsFullyMigrated { get; set; }
    }
}