using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
    public class IncentivePolicy : BaseModel
    {
        [Column(Order = 2)]
        [Display(Name = "Category")]
        public int CategoryTypeId { get; set; }
        [ForeignKey("CategoryTypeId")]
        public CategoryType CategoryType { get; set; }

        [Column(Order = 3)]
        [Display(Name = "Application Number")]
        public string ApplicationNumber { get; set; }

        [Column(Order = 4)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order=5)]
        [Display(Name="Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order=6)]
        [Display(Name="Industry Sector")]
        public int? IndustryId { get; set; }
        [ForeignKey("IndustryId")]
        public Industry Industry { get; set; }

        [Column(Order = 7)]
        public int? ClerkUserId { get; set; }
        [ForeignKey("ClerkUserId")]
        [Display(Name = "Clerk Id")]
        public SystemUser ClerkId { get; set; }

       [Column(Order=8)]
       public bool HasElectronicSignature { get; set; } 

        [Column(Order=9)]
        [Display(Name = "Declared as ")]
        public string DeclarationName { get; set; }

        [Column(Order=10)]
        [Display(Name="Application Date")]
        public DateTime? ApplicationDateTime { get; set; }
    }
}
