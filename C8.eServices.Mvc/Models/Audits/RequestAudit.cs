using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RequestAudit : BaseModelAudit
    {
        [Column(Order = 54)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 55)]
        [Display(Name = "Reference Type")]
        public int? ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }

        [Column(Order = 56)]
        [Display(Name = "Numeric Reference")]
        public int ReferenceNumeric { get; set; }

        [Column(Order = 57)]
        [Display(Name = "Municipal Account")]
        public string ReferenceAlpha { get; set; }

        [Column(Order = 58)]
        [Display(Name = "Comment")]
        public int? CommentId { get; set; }
        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }

        [Column(Order = 59)]
        [MaxLength(100)]
        [Display(Name = "Recipient Email/Mobile")]
        public string RecipientContact { get; set; }

        [Column(Order = 60)]
        [Display(Name = "Recipient Customer")]
        public int? RecipientCustomerId { get; set; }
        [ForeignKey("RecipientCustomerId")]
        public Customer RecipientCustomer { get; set; }
        //[Column(Order = 61)]
        //[Display(Name = "Recipient Comment")]
        //public int? ReceipientCommentId { get; set; }
        //[ForeignKey("ReceipientCommentId")]
        //public Comment ReceipientComment { get; set; }
        [Column(Order = 62)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }
    }
}