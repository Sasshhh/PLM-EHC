using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace C8.eServices.Mvc.Models
{
    public class Agent : BaseModel
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
        public int ? IdentificationTypeId { get; set; }
        [ForeignKey("IdentificationTypeId")]
        public IdentificationType IdentificationType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Country Of Issue")]
        public int ? CountryOfIssueTypeId { get; set; }
        [ForeignKey("CountryOfIssueTypeId")]
        public Country Country { get; set; }

        [Column(Order = 14)]
        [Display(Name = "ID/ Passport No")]
        [MaxLength(100)]
        public string IdentificationNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Title")]
        public int ? TitleTypeId { get; set; }
        [ForeignKey("TitleTypeId")]
        public TitleType TitleType { get; set; }

        [Column(Order = 16)]
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Gender")]
        [MaxLength(100)]
        public string Gender { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Email")]
        [MaxLength(100)]
        public string EmailAddress { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Work Phone")]
        public string WorkPhoneNumber { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Home Phone")]
        [MaxLength(100)]
        public string HomePhoneNumber { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Cell Phone")]
        [MaxLength(100)]
        public string CellPhoneNumber { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Physical Address 1")]
        [MaxLength(128)]
        public string PhysicalAddress1 { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Physical Address 2")]
        [MaxLength(128)]
        public string PhysicalAddress2 { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Physical Address 3")]
        [MaxLength(128)]
        public string PhysicalAddress3 { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Physical Address 4")]
        [MaxLength(128)]
        public string PhysicalAddress4 { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Physical Address 5")]
        [MaxLength(128)]
        public string PhysicalAddress5 { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Physical Area Code")]
        public int PhysicalAddressCode { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Postal Address 1")]
        [MaxLength(128)]
        public string PostalAddress1 { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Postal Address 2")]
        [MaxLength(128)]
        public string PostalAddress2 { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Postal Address 3")]
        [MaxLength(128)]
        public string PostalAddress3 { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Postal Address 4")]
        [MaxLength(128)]
        public string PostalAddress4 { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Postal Address 5")]
        [MaxLength(128)]
        public string PostalAddress5 { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Postal Area Code")]
        public int PostalAddressCode { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 36)]
        [MaxLength(100)]
        [Display(Name = "Attorney Code")]
        public string AttorneyCode { get; set; }


        [Display(Name = "Customer")]
        public IEnumerable<LinkedAccount> LinkedAccounts { get; set; }

        [Display(Name = "Customer")]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}