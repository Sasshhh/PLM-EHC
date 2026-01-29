using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class DocumentsLeaseAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Reference Type")]
        public int ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }

        [Column(Order = 12)]
        public int ReferenceId { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Location Type")]
        public int LocationTypeId { get; set; }
        [ForeignKey("LocationTypeId")]
        public LocationType LocationType { get; set; }

        [Column(Order = 14)]
        [MaxLength(250)]
        public string DocumentLocation { get; set; }

        [Column(Order = 15)]
        [MaxLength(500)]
        public string DocumentName { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 17)]
        [Display(Name = "DocumentCheckListId")]
        public int DocumentCheckListId { get; set; }
        [ForeignKey("DocumentCheckListId")]
        public DocumentCheckList DocumentCheckList { get; set; }

        [Column(Order = 18)]
        [Display(Name = "File")]
        public int? FileId { get; set; }
        [ForeignKey("FileId")]
        public File File { get; set; }

        [Column(Order = 19)]
        [Display(Name = "RCS Application")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Column(Order = 20)]
        [MaxLength(500)]
        public string Comment { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Refund Application")]
        public int? RefundApplicationId { get; set; }
        [ForeignKey("RefundApplicationId")]
        public RefundApplication RefundApplication { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Tenant Lease Application")]
        public int? TenantLeaseId { get; set; }
        [ForeignKey("TenantLeaseId")]
        public LeaseDetails leaseDetails { get; set; }


    }
}