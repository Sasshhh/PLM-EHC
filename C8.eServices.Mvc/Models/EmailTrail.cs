using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class EmailTrail
    {
        [Key]
        public int EmailQueueId { get; set; }
        public int ApplicationId { get; set; }
        public int EmailAccountId { get; set; }
        public DateTime QueueDateTime { get; set; }
        public string ToList { get; set; }
        public string CcList { get; set; }
        public string BccList { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public int FailureCount { get; set; }
        public string ReferenceId { get; set; }
        public int ReferenceTypeId { get; set; }
        public bool HasAttachments { get; set; }
        public string CaseReferenceNo { get; set; }
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Display(Name = "Refund Application Id")]
        public int? RefundApplicationId { get; set; }
        [ForeignKey("RefundApplicationId")]
        public RefundApplication RefundApplication { get; set; }
    }
}