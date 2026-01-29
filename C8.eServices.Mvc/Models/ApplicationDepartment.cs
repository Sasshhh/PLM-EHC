using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ApplicationDepartment: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "PropertyApplicationRef")]
        public string PropertyApplicationRef { get; set; }

        [Column(Order = 11)]
        [Display(Name = "DepartmentType")]
        public string DepartmentType { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 13)]
        [Display(Name = "PropertyLeaseApplication")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Action Due Date")]
        public DateTime ActionDueDate { get; set; }

        [Column(Order = 15)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 16)]
        [Display(Name = "LeaseDetailsReference")]
        public string LeaseDetailsReference { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Applicant Type")]
        public int? PurchaserTypeId { get; set; }
        [ForeignKey("PurchaserTypeId")]
        public PurchaserType PurchaserType { get; set; }
    }
}