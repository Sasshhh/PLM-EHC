using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace C8.eServices.Mvc.Models.Audits
{
    public class AgentAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Type Of Customer")]
        public int CustomerTypeId { get; set; }
        [ForeignKey("CustomerTypeId")]
        public CustomerType CustomerType { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Type Of ID")]
        public int IdentificationTypeId { get; set; }
        [ForeignKey("IdentificationTypeId")]
        public IdentificationType IdentificationType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Country Of Issue")]
        public int ? CountryOfIssueTypeId { get; set; }
        [ForeignKey("CountryOfIssueTypeId")]
        public Country Country { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "ID/ Passport No")]
        public string IdentificationNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Title")]
        public int TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Column(Order = 16)]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Column(Order = 17)]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Column(Order = 18)]
        [MaxLength(25)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Column(Order = 19)]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        [Column(Order = 20)]
        [MaxLength(100)]
        [Display(Name = "Work Phone")]
        public string WorkPhoneNumber { get; set; }

        [Column(Order = 21)]
        [MaxLength(100)]
        [Display(Name = "Home Phone")]
        public string HomePhoneNumber { get; set; }

        [Column(Order = 22)]
        [MaxLength(100)]
        [Display(Name = "Cell Phone")]
        public string CellPhoneNumber { get; set; }

        [Column(Order = 23)]
        [MaxLength(100)]
        [Display(Name = "Physical Address 1")]
        public string PhysicalAddress1 { get; set; }

        [Column(Order = 24)]
        [MaxLength(100)]
        [Display(Name = "Physical Address 2")]
        public string PhysicalAddress2 { get; set; }

        [Column(Order = 25)]
        [MaxLength(100)]
        [Display(Name = "Physical Address 3")]
        public string PhysicalAddress3 { get; set; }

        [Column(Order = 26)]
        [MaxLength(100)]
        [Display(Name = "Physical Address 4")]
        public string PhysicalAddress4 { get; set; }

        [Column(Order = 27)]
        [MaxLength(100)]
        [Display(Name = "Physical Address 5")]
        public string PhysicalAddress5 { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Physical Area Code")]
        public int PhysicalAddressCode { get; set; }

        [Column(Order = 29)]
        [MaxLength(100)]
        [Display(Name = "Postal Address 1")]
        public string PostalAddress1 { get; set; }

        [Column(Order = 30)]
        [MaxLength(100)]
        [Display(Name = "Postal Address 2")]
        public string PostalAddress2 { get; set; }

        [Column(Order = 31)]
        [MaxLength(100)]
        [Display(Name = "Postal Address 3")]
        public string PostalAddress3 { get; set; }

        [Column(Order = 32)]
        [MaxLength(100)]
        [Display(Name = "Postal Address 4")]
        public string PostalAddress4 { get; set; }

        [Column(Order = 33)]
        [MaxLength(100)]
        [Display(Name = "Postal Address 5")]
        public string PostalAddress5 { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Postal Area Code")]
        public int PostalAddressCode { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Display(Name = "Customer")]
        public IEnumerable<LinkedAccount> LinkedAccounts { get; set; }

        [Display(Name = "Customer")]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}