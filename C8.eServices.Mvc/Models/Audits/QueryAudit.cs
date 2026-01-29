using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class QueryAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        public int QueryTypeId { get; set; }

        [Column(Order = 11)]
        public string Comment { get; set; }

        [Column(Order = 12)]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "StatusId")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 16)]
        public string ResolutionComment { get; set; }

        [Column(Order = 17)]
        [MaxLength(100)]
        public string ContactNumber { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Date Resolved")]
        public DateTime? ResolvedDateTime { get; set; }
    }
}