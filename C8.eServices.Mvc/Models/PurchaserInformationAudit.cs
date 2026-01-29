using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using C8.eServices.Mvc.Models.Audits;

namespace C8.eServices.Mvc.Models
{
    public class PurchaserInformationAudit : BaseModelAudit
    {

        [Column(Order = 11)]
        [Display(Name = "RCSApplicationStatusId")]
        public int RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Purchase Type")]

        public int PurchaseType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Gender")]
        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "Title")]
        [Column(Order = 14)]
        [StringLength(10)]
        public string Title { get; set; }

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

        [Display(Name = "Age")]
        [Column(Order = 21)]

        public int? Age { get; set; }

        [Display(Name = "Passport Number")]
        [Column(Order = 22)]
        [StringLength(25)]
        public string Passport { get; set; }

        [Column(Order = 23)]
        [StringLength(100)]
        public string Pur_Country_OI { get; set; }

        [Display(Name = "Nationality")]
        [Column(Order = 24)]
        [StringLength(50)]
        public string Nationality { get; set; }

        [Display(Name = "Passport Expiry Date")]
        [Column(Order = 25)]
        public DateTime? PurDateExpiry { get; set; }

        [Display(Name = "Status")]
        [Column(Order = 26)]
        [StringLength(20)]
        public string PurResStatus { get; set; }

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

        [Display(Name = "Juristic Type")]
        [Column(Order = 31)]
        [StringLength(50)]
        public string JuristicType { get; set; }

        [Display(Name = "Deemed Name")]
        [Column(Order = 32)]
        [StringLength(100)]
        public string DeemedName { get; set; }

        [Display(Name = "Deemed ID")]
        [Column(Order = 33)]
        [StringLength(50)]
        public string DeemedID { get; set; }

        [Display(Name = "Deemed Trade Name")]
        [Column(Order = 34)]
        [StringLength(100)]
        public string DeemedTradeName { get; set; }

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

        [Display(Name = "Address")]
        [Column(Order = 38)]
        [StringLength(50)]
        public string PostAddress { get; set; }

        [Display(Name = "Postal City")]
        [Column(Order = 39)]
        [StringLength(50)]
        public string PostalCity { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 40)]
        [StringLength(50)]
        public string PostSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 41)]
        [StringLength(50)]
        public string PostPostalCode { get; set; }

        [Display(Name = "Address")]
        [Column(Order = 42)]
        [StringLength(50)]
        public string DomAddress { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 43)]
        [StringLength(50)]
        public string DomSub { get; set; }

        [Display(Name = "City")]
        [Column(Order = 44)]
        [StringLength(50)]
        public string DomCity { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 45)]
        [StringLength(50)]
        public string DomiciliumPostalCode { get; set; }

        [Column(Order = 46)]
        [StringLength(50)]
        [Display(Name = "Application Number")]
        public string AppNumber { get; set; }

        [Display(Name = "Property Information")]
        [Column(Order = 47)]
        public int? PropertyInfo { get; set; }

        [Display(Name = "Rate Number")]
        [Column(Order = 48)]
        [StringLength(20)]
        public string RateNumEntered { get; set; }


        [Column(Order = 49)]
        public int? sub_AgentData { get; set; }

        [Display(Name = "Application Status")]
        [Column(Order = 50)]
        [StringLength(10)]
        public string AppStatus { get; set; }


        [Column(Order = 51)]
        [StringLength(50)]
        public string con_Com { get; set; }

        [Display(Name = "Firm Owner")]
        [Column(Order = 52)]
        public int? FirmOwner { get; set; }

        [Display(Name = "CC Reg No")]
        [Column(Order = 53)]
        [StringLength(100)]
        public string CCRegNo { get; set; }

        [Display(Name = "CC Name")]
        [Column(Order = 54)]
        [StringLength(100)]
        public string CCName { get; set; }

        [Display(Name = "CC Name Trading As")]
        [Column(Order = 55)]
        [StringLength(100)]
        public string CCNameTradingAs { get; set; }

        [Display(Name = "Business Registration No")]
        [Column(Order = 56)]
        [StringLength(100)]
        public string BusinessRegNo { get; set; }

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

        [Display(Name = "Trust Name Trading As")]
        [Column(Order = 61)]
        [StringLength(100)]
        public string TrustNameTradingAs { get; set; }

        [Display(Name = "Specify")]
        [Column(Order = 62)]
        [StringLength(100)]
        public string Specify { get; set; }

        [Display(Name = "Name")]
        [Column(Order = 63)]
        [StringLength(100)]
        public string SpecifyName { get; set; }


        [Display(Name = "Registration No")]
        [Column(Order = 64)]
        [StringLength(100)]
        public string SpecifyRegistrationNo { get; set; }

        [Display(Name = "City")]
        [Column(Order = 65)]
        [StringLength(10)]
        public string ResCity { get; set; }

        [Column(Order = 66)]
        [Display(Name = "Purchaser Type")]

        public int? PurchaserTypeId { get; set; }
        [ForeignKey("PurchaserTypeId")]
        public PurchaserType PurchaserTypes { get; set; }


        [Display(Name = "Nominated Address")]
        [Column(Order = 67)]
        [StringLength(10)]
        public string NominatedAddress { get; set; }



        [Display(Name = "Registered Name")]
        [Column(Order = 68)]
        [StringLength(300)]
        public string RegisteredName { get; set; }

        [Display(Name = "Registration Number")]
        [Column(Order = 69)]
        [StringLength(300)]
        public string RegistrationNumber { get; set; }


        [Display(Name = "VAT Registration Number")]
        [Column(Order = 70)]
        [StringLength(300)]
        public string VATRegistrationNumber { get; set; }

        [Display(Name = "PurchaserTypeKey")]
        [Column(Order = 71)]
        [StringLength(300)]
        public string PurchaserTypeKey { get; set; }
    }
}