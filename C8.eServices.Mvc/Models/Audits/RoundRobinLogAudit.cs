using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RoundRobinLogAudit:BaseModelAudit
    {
      

            [MaxLength(250)]
            [Column(Order = 10)]
            public string LogEntry { get; set; }

            [Column(Order = 11)]
            [Display(Name = "CCCId")]
            public int CCCId { get; set; }


            [Column(Order = 12)]
            [Display(Name = "CCCName")]
            public string CCCName { get; set; }

            [Column(Order = 13)]
            [Display(Name = "RoleID")]
            public string RoleID { get; set; }

            [Column(Order = 14)]
            [Display(Name = "RoleName")]
            public string RoleName { get; set; }

            [Column(Order = 15)]
            [Display(Name = "Department Approvals Id")]
            public int? DepartmentApprovalId { get; set; }
            [ForeignKey("DepartmentApprovalId")]
            public DepartmentsApproval DepartmentsApproval { get; set; }

            [Column(Order = 16)]
            [Display(Name = "Refund Application Id")]
            public int? RefundApplicationId { get; set; }
            [ForeignKey("RefundApplicationId")]
            public RefundApplication RefundApplication { get; set; }

            [Column(Order = 17)]
            [Display(Name = "RCSApplication Id")]
            public int? RCSApplicationStatusId { get; set; }
            [ForeignKey("RCSApplicationStatusId")]
            public RCSApplicationStatus RCSApplicationStatus { get; set; }

            [Column(Order = 18)]
            [Display(Name = "Responsibility Type")]
            public int ResponsibilityTypeId { get; set; }
            [ForeignKey("ResponsibilityTypeId")]
            public ResponsibilityType ResponsibilityType { get; set; }



        
    }
}