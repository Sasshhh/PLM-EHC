using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace C8.eServices.Mvc.Models
{
    public class DepartmentsApproval: BaseModel
    {
        [Column(Order = 11)]
        [Display(Name = "RCSApplicationStatusId")]
        public int RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 12)]
        [Display(Name = "DepartmentId")]
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public RCSDepartmentType RCSDepartment { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Failure Reason")]
        [StringLength(300)]
        public string FailureReason { get; set; }

      

        [Column(Order = 14)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Display(Name = "Comment")]
        [Column(Order = 15)]
        [StringLength(300)]
        public string Comment { get; set; }

        [Display(Name = "Assigned To")]
        [Column(Order = 16)]
        public int? AssignedToCustomerId { get; set; }
        [ForeignKey("AssignedToCustomerId")]
        public Customer AssignedToCustomer { get; set; }

        [Column(Order = 17)]
        [Display(Name = "System User")]
        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public SystemUser SystemUser { get; set; }


        [Display(Name = "Captured Date")]
        [Column(Order = 18)]
       
        public DateTime CapturedDate { get; set; }

    }
}