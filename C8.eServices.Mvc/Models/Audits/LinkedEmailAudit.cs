using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class LinkedEmailAudit : BaseModelAudit
    {
        [Required]
        [Column( Order = 10 )]
        [Display( Name = "Linked Account" )]
        public int LinkedAccountId { get; set; }
        [ForeignKey( "LinkedAccountId" )]
        public LinkedAccount LinkedAccount { get; set; }

        [Required]
        [Column( Order = 11 )]
        [Display( Name = "Recipient Type" )]
        public int RecipientTypeId { get; set; }
        [ForeignKey( "RecipientTypeId" )]
        public RecipientType RecipientType { get; set; }

        [Required]
        [Column(Order = 12)]
        [MaxLength(100)]
        [Display( Name = "Email Address" )]
        public string EmailAddress { get; set; }

        [Required]
        [Column( Order = 13 )]
        [Display( Name = "Status" )]
        public int StatusId { get; set; }
        [ForeignKey( "StatusId" )]
        public Status Status { get; set; }

    }
}