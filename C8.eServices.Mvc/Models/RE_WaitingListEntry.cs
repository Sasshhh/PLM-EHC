using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// DPRE BR07 / DPRE-Q50: an applicant waiting for a Business Hub unit when none is free.
    /// First come, first served within a Customer Care Area (and facility, when the applicant picked one).
    /// WaitingStatus: Waiting, Offered (a Property Officer told the applicant a unit is free), Removed.
    /// </summary>
    [Table("RE_WaitingList")]
    public class RE_WaitingListEntry : BaseModel
    {
        public const string StatusWaiting = "Waiting";
        public const string StatusOffered = "Offered";
        public const string StatusRemoved = "Removed";

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public int CCCId { get; set; }
        [ForeignKey("CCCId")]
        public virtual CCC CCC { get; set; }

        public int? FacilityId { get; set; }
        [ForeignKey("FacilityId")]
        public virtual RE_Facility Facility { get; set; }

        [MaxLength(60)] public string LeaseType { get; set; }
        [MaxLength(60)] public string PurposeOfLease { get; set; }
        [MaxLength(20)] public string WaitingStatus { get; set; }
        public DateTime? OfferedDateTime { get; set; }
        [MaxLength(500)] public string OutcomeReason { get; set; }
    }
}
