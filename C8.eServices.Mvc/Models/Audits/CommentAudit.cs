using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class CommentAudit : BaseModelAudit
    {
        [Column(Order = 49)]
        [Display(Name = "Reference Type")]
        public int? ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }

        [Column(Order = 50)]
        [Display(Name = "Sender Customer Id")]
        public int? SenderCustomerId { get; set; }
        [ForeignKey("SenderCustomerId")]
        public Customer SenderCustomer { get; set; }

        [Column(Order = 51)]
        [Display(Name = "Numeric Reference")]
        public int ReferenceNumeric { get; set; }

        [Column(Order = 52)]
        [Display(Name = "Account Reference")]
        public string ReferenceAlpha { get; set; }

        [Column(Order = 53)]
        [Display(Name = "Comment")]
        public string Comments { get; set; }

        [Column(Order = 54)]
        [Display(Name = "CommunicationId")]
        public int? CommunicationId { get; set; }
        [ForeignKey("CommunicationId")]
        public Communication Communication { get; set; }
    }
}