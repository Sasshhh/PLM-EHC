using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// UC025 — Tracks eviction notice service and proof of service records.
    /// </summary>
    [Table("EvictionServiceRecords")]
    public class EvictionServiceRecord : BaseModel
    {
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public virtual PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public virtual LeaseDetails LeaseDetails { get; set; }

        [StringLength(50)]
        public string EvictionReferenceNumber { get; set; }

        // UC025-S1: Serve Notice
        [StringLength(100)]
        public string ServiceMethod { get; set; } // Sheriff of the Court / Hand delivery / Registered post / Email

        public DateTime? ServiceDate { get; set; }

        [StringLength(50)]
        public string OfficialNumber { get; set; }

        public bool NoticeServed { get; set; }

        // UC025-S2: Proof of Service
        [StringLength(100)]
        public string ProofOfServiceType { get; set; } // Sheriff return / Signed ack / Tracking / Email / Failed

        public DateTime? ProofServiceDate { get; set; }

        [StringLength(2000)]
        public string ProofComments { get; set; }

        public bool ProofCaptured { get; set; }

        // Status tracking
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }
    }
}
