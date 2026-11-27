using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    [Table("RE_WaitingListAudits")]
    public class RE_WaitingListEntryAudit : BaseModelAudit
    {
        public int CustomerId { get; set; }
        public int CCCId { get; set; }
        public int? FacilityId { get; set; }
        [MaxLength(60)] public string LeaseType { get; set; }
        [MaxLength(60)] public string PurposeOfLease { get; set; }
        [MaxLength(20)] public string WaitingStatus { get; set; }
        public DateTime? OfferedDateTime { get; set; }
        [MaxLength(500)] public string OutcomeReason { get; set; }
    }
}
