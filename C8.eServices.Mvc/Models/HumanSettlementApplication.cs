using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HumanSettlementApplication: BaseModel
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
        public int? GenderId { get; set; }
        [ForeignKey("GenderId")]
        public RCSType Gender { get; set; }

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
        [Column(Order = 21)]
        public int? IdentificationTypeId { get; set; }
        [ForeignKey("IdentificationTypeId")]
        public IdentificationType IdentificationType { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 22)]
        [StringLength(15)]
        public string CellNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Home number is not valid")]
        [Display(Name = "Home Number")]
        [Column(Order = 23)]
        [StringLength(15)]
        public string HomeNo { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Work number is not valid")]
        [Display(Name = "Work Number")]
        [Column(Order = 24)]
        [StringLength(15)]
        public string WorkNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 25)]
        [StringLength(50)]
        public string PurEmail { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 26)]
        [StringLength(500)]
        public string ResAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 27)]
        [StringLength(250)]
        public string ResSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 28)]
        [StringLength(10)]
        public string ResPostal { get; set; }


        [Column(Order = 29)]
        [Display(Name = "Income Source")]
        public int? IncomeSourceId { get; set; }
        [ForeignKey("IncomeSourceId")]
        public IncomeSource IncomeSource { get; set; }

        [Column(Order = 30)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SassaNumber { get; set; }


        [Display(Name = "Gross Income")]
        [Column(Order = 31)]
        public decimal GrossIncome { get; set; }

        [Display(Name = "Net Income")]
        [Column(Order = 32)]
        public decimal NetIncome { get; set; }

        [Display(Name = "Total Combined Income")]
        [Column(Order = 33)]
        public decimal TotalCombinedIncome { get; set; }

        [Column(Order = 36)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexAreaId { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea { get; set; }

        [Column(Order = 37)]
        [Display(Name = "Preferred ComplexArea")]
        public int? PreferredComplexArea2Id { get; set; }
        [ForeignKey("PreferredComplexAreaId")]
        public PreferredComplexArea PreferredComplexArea2 { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 39)]
        [Display(Name = "SystemUser")]
        public int SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }

        [Column(Order = 42)]
        [Display(Name = "Applicant Name")]
        public string ApplicantFullName { get; set; }

        [Column(Order = 43)]
        [Display(Name = "Second Applicant")]
        public bool SecondApplicant { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 44)]
        [StringLength(50)]
        public string SecAppFirstName { get; set; }

        [Display(Name = "Last Name")]
        [Column(Order = 45)]
        [StringLength(50)]
        public string SecAppLastName { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 46)]
        [StringLength(25)]
        public string SecAppIDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [Column(TypeName = "date", Order = 47)]
        public DateTime? SecAppDOB { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Mobile number is not valid")]
        [Display(Name = "Mobile Number")]
        [Column(Order = 48)]
        [StringLength(15)]
        public string SecAppCellNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 49)]
        [StringLength(50)]
        public string SecAppEmail { get; set; }

        [Display(Name = "Gross Income")]
        [Column(Order = 50)]
        public decimal? SecAppGrossIncome { get; set; }

        [Display(Name = "Net Income")]
        [Column(Order = 51)]
        public decimal? SecAppNetIncome { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 52)]
        public string SecAppAddress { get; set; }

        [Display(Name = "Postal")]
        [Column(Order = 53)]

        public string SecAppPostal { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 54)]
        public string SecAppSuburb { get; set; }

        [Column(Order = 55)]
        [Display(Name = "Income Source")]
        public int? SecAppIncomeSourceId { get; set; }
        [ForeignKey("SecAppIncomeSourceId")]
        public IncomeSource SecAppIncomeSource { get; set; }


        [Column(Order = 56)]
        [StringLength(50)]
        [Display(Name = "Sassa Number")]
        public string SecAppSassaNumber { get; set; }

        [Column(Order = 57)]
        [Display(Name = "Gender")]
        public int? SecAppGenderId { get; set; }
        [ForeignKey("SecAppGenderId")]
        public RCSType SecAppGender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 58)]
        public int? SecAppTitleTypeId { get; set; }
        [ForeignKey("SecAppTitleTypeId")]
        public TitleType SecAppTitleType { get; set; }

        [Display(Name = "Serve Notice")]
        [Column(Order = 60)]
        public DateTime? ServeNoticeDate { get; set; }

        [Display(Name = "Walk-In")]
        [Column(Order = 61)]
        public bool IsWalkIn { get; set; }

        [Display(Name = "Walk-In By User")]
        [Column(Order = 62)]
        public int? WalkInBySystemUserId { get; set; }
        [ForeignKey("WalkInBySystemUserId")]
        public SystemUser WalkInBySystemUser { get; set; }

        [Display(Name = "Walk-In Date")]
        [Column(Order = 63)]
        public DateTime? WalkInDateCaptured { get; set; }

        [Display(Name = "Walk-In")]
        [Column(Order = 64)]
        public bool IsMaster { get; set; }

        [Column(Order = 65)]
        [Display(Name = "Nominate Spouse")]
        public bool NominateSpouse { get; set; }

        [Display(Name = "Unit Category")]
        [Column(Order = 66)]
        public int? UnitCategoryId { get; set; }
        [ForeignKey("UnitCategoryId")]
        public HSUnitCategory UnitCategory { get; set; }

        [Display(Name = "Unit Typology")]
        [Column(Order = 67)]
        public int? UnitTypologyId { get; set; }
        [ForeignKey("UnitTypologyId")]
        public HSUnitTypology UnitTypology { get; set; }

        [Column(Order = 68)]
        [Display(Name = "HSIncomeBracket")]
        public int? HSIncomeBracketId { get; set; }
        [ForeignKey("HSIncomeBracketId")]
        public HSIncomeBrackets HSIncomeBracket { get; set; }

        [Display(Name = "Walk-In")]
        [Column(Order = 69)]
        public bool IsTransferred { get; set; }

        [Display(Name = "Walk-In")]
        [Column(Order = 70)]
        public string OTP { get; set; }

        [Column(Order = 71)]
        [Display(Name = "Relationship")]
        public int? RelationshipId { get; set; }
        [ForeignKey("RelationshipId")]
        public RCSType Relationship { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 72)]
        public string CoFirstName { get; set; }

        [Display(Name = "Surname")]
        [Column(Order = 73)]
        public string CoLastName { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 74)]
        public int? CoTitleTypeId { get; set; }
        [ForeignKey("CoTitleTypeId")]
        public TitleType CoTitleType { get; set; }

        [Display(Name = "Mobile Number")]
        [Column(Order = 75)]
        public string CoCell { get; set; }

        [Display(Name = "Email")]
        [Column(Order = 76)]
        //[EmailAddress]
        public string CoEmail { get; set; }

        [Column(Order = 77)]
        [Display(Name = "Special Needs Required")]
        public int? SpecialNeedId { get; set; }
        [ForeignKey("SpecialNeedId")]
        public RCSActionType SpecialNeed { get; set; }

        [Display(Name = "Motivate")]
        [Column(Order = 78)]
        public string MotivateNeed { get; set; }

        [Display(Name = "CommitteeDate")]
        [Column(Order = 79)]
        public DateTime? CommitteeDate { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
    }
}