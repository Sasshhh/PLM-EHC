using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class IncentivePolicyAudit: BaseModelAudit
    {
        [Display(Name = "Category")]
        public int CategoryTypeId { get; set; }
        [ForeignKey("CategoryTypeId")]
        public CategoryType CategoryType { get; set; }

        [Display(Name = "Application Number")]
        public string ApplicationNumber { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Display(Name = "Industry Sector")]
        public int? IndustryId { get; set; }
        [ForeignKey("IndustryId")]
        public Industry Industry { get; set; }

        public int? ClerkUserId { get; set; }
        [ForeignKey("ClerkUserId")]
        [Display(Name = "Clerk Id")]
        public SystemUser ClerkId { get; set; }
        public bool HasElectronicSignature { get; set; }
        public string DeclarationName { get; set; }
        [Display(Name = "Application Date")]
        public DateTime? ApplicationDateTime { get; set; }
    }
}
