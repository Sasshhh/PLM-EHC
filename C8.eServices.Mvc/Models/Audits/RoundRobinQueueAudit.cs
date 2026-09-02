using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RoundRobinQueueAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Clerk")]
        public int ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        public Customer Clerk { get; set; }


        [Column(Order = 11)]
        [Display(Name = "RCSApplication Id")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Responsibility Type")]
        public int ResponsibilityTypeId { get; set; }
        [ForeignKey("ResponsibilityTypeId")]
        public ResponsibilityType ResponsibilityType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Department Approvals Id")]
        public int? DepartmentApprovalId { get; set; }
        [ForeignKey("DepartmentApprovalId")]
        public DepartmentsApproval DepartmentsApproval { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Refund Application Id")]
        public int? RefundApplicationId { get; set; }
        [ForeignKey("RefundApplicationId")]
        public RefundApplication RefundApplication { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Current Task Start Date")]
        [Column(Order = 16)]
        public DateTime? CurrentTaskDateTime { get; set; }


        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Current Task Start Date")]
        [Column(Order = 17)]
        public DateTime? EndTaskDateTime { get; set; }

        [Column(Order = 18)]
        [Display(Name = "PropertyLeaseApplication Id")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 19)]
        [Display(Name = "LeaseDetails Id")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        public int? AssignedFrom { get; set; }

        [Column(Order = 20)]
        [Display(Name = "HumanSettlementApplicationId")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Tenant Complaint Id")]
        public int? TenantComplaintId { get; set; }

        [Column(Order = 23)]
        [Display(Name = "RealEstateApplication Id")]
        public int? RealEstateApplicationId { get; set; }
        [ForeignKey("RealEstateApplicationId")]
        public virtual RE_Application RealEstateApplication { get; set; }
    }
}