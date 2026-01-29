using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;


namespace C8.eServices.Mvc.Models
{
    public class ConveyancingAttorneyDetail : BaseModel
    {
        [Column(Order = 11)]
        [Display(Name = "RCSApplicationStatusId")]
        public int RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Attorney Code")]
        [StringLength(50)]
        public string AttorneyCode { get; set; }


        [Column(Order = 13)]
        [Display(Name = "Firm Name")]
        [StringLength(100)]
        public string FirmName { get; set; }


        [Column(Order = 14)]
        [Display(Name = "Practice Number")]
        [StringLength(100)]
        public string PracticeNumber{ get; set; }

        [Display(Name = "Postal Address")]
        [Column(Order = 15)]
        [StringLength(100)]
        public string PostAddress { get; set; }

        [Display(Name = "City")]
        [Column(Order = 16)]
        [StringLength(100)]
        public string PostalCity { get; set; }

        [Display(Name = "Suburb")]
        [Column(Order = 17)]
        [StringLength(100)]
        public string PostSuburb { get; set; }

        [Display(Name = "Postal Code")]
        [Column(Order = 18)]
        [StringLength(50)]
        public string PostalCode { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Contact number is not valid")]
        [Display(Name = "Contact Number")]
        [Column(Order = 19)]
        [StringLength(15)]
        public string ContactNumber { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 20)]
        [StringLength(50)]
        public string Email { get; set; }


        [Column(Order = 21)]
        [Display(Name = "Contact Person 1")]
        [StringLength(100)]
        public string ContactPerson1 { get; set; }



        [Column(Order = 22)]
        [Display(Name = "Contact Person2 ")]
        [StringLength(100)]
        public string ContactPerson2 { get; set; }


    }
}