using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class AccountAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [MaxLength(30)]
        public string AccountNumber { get; set; }

        [Column(Order = 11)]
        [MaxLength(100)]
        [Display(Name = "Identification Number")]
        public string ReferenceNumber { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Property")]
        public int? PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        public string RateNumber { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 15)]
        [MaxLength(250)]
        public string AccountHolder { get; set; }

        [Column(Order = 16)]
        public bool IsRMSAccount { get; set; }

        [Column(Order = 17)]
        [MaxLength(100)]
        public string Description { get; set; }
    }
}