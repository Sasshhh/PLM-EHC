using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DepartmentalComments: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Department")]
        public string DepartmentName  { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Surname")]
        public string LastName { get; set; }
        [Column(Order = 12)]
        [Display(Name = "Last Name")]
        public string FirstName { get; set; }
        [Column(Order = 13)]
        [Display(Name = "Official Number")]
        public string OfficialNumber { get; set; }
        [Column(Order = 14)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Column(Order = 16)]
        [Display(Name = "ApplicationReferenceNumber")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Supporting Doccuments")]
        public File SupportingDoccuments { get; set; }

        [DataType(DataType.Date)]
        [Column(Order = 18)]
        [Display(Name = "Date Stamp")]
        public DateTime? DateStamp { get; set; }

        [Column(Order = 19)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }


    }
}