using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RefundApplicationAudit:BaseModelAudit
    {


        [Column(Order = 10)]
        [Display(Name = "Application Reference Number")]
        [MaxLength(50)]
        public string ApplicationReferenceNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        [ScriptIgnore]
        [JsonIgnore]
        public Customer Customer { get; set; }

        

        [Column(Order = 13)]
        [Display(Name = "RCS Application Status")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Clerk")]
        public int? ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        public Customer Clerk { get; set; }
        [Column(Order = 15)]
        public decimal? RefundAmount { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Collection Date")]
        [MaxLength(50)]
        public string CollectionDate { get; set; }
    }
}