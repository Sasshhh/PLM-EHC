using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Executor : BaseModel
    {
        [Column( Order = 10 )]
        [Display( Name = "Customer" )]
        public int CustomerId { get; set; }
        [ForeignKey( "CustomerId" )]
        public Customer Customer { get; set; }

        [Column( Order = 11 )]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Column( Order = 12 )]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Column( Order = 13 )]
        [MaxLength(100)]
        [Display(Name = "Contact No.")]
        public string ContactNumber { get; set; }

        [Column( Order = 14 )]
        [MaxLength(100)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Column( Order = 15 )]
        [MaxLength(100)]
        [Display(Name = "Door No.")]
        public string PostalAddress1 { get; set; }

        [Column( Order = 16 )]
        [MaxLength(100)]
        [Display(Name = "Block/ Complex Name")]
        public string PostalAddress2 { get; set; }

        [Column( Order = 17 )]
        [MaxLength(100)]
        [Display(Name = "Street No.")]
        public string PostalAddress3 { get; set; }

        [Column( Order = 18 )]
        [MaxLength(100)]
        [Display(Name = "Street Name")]
        public string PostalAddress4 { get; set; }

        [Column( Order = 19 )]
        [MaxLength(100)]
        [Display(Name = "Suburb")]
        public string PostalAddress5 { get; set; }

        [Column( Order = 20 )]
        [MaxLength(30)]
        [Display(Name = "Postal Code")]
        public string PostalAddressCode { get; set; }
    }
}