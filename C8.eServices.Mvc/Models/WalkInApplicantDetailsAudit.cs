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
    public class WalkInApplicantDetailsAudit : BaseModelAudit
    {
        [Column(Order = 11)]
        [Display(Name = "RCSApplicationStatusId")]
        public int RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Display(Name = "Applicant Name")]
        [Column(Order = 12)]
        [StringLength(100)]
        public string ApplicanttName { get; set; }


        [Column(Order = 13)]
        [Display(Name = "Act on Behalf of")]

        public int ActOnBehalfOf { get; set; }


        [Display(Name = "Business Address")]
        [Column(Order = 14)]
        [StringLength(50)]
        public string BusinessAddress { get; set; }


        [DataType(DataType.PhoneNumber, ErrorMessage = "Contact number is not valid")]
        [Display(Name = "Contact Number")]
        [Column(Order = 15)]
        [StringLength(15)]
        public string ContactNo { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail address is not valid")]
        [Display(Name = "Email Address")]
        [Column(Order = 16)]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "Contact Person")]
        [Column(Order = 17)]
        [StringLength(50)]
        public string ContactPerson { get; set; }



    }
}