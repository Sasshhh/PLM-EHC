using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class LeaseTerminationAudit: BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "PropertyLeaseApplication")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Reason For Termination")]
        public string ReasonForTermination { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Lease Reference Number")]
        public string LeaseReferenceNumber { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Termination Date")]
        public DateTime TerminationDate { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 16)]
        [Display(Name = "HumanSettlementApplication")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

    }
}