using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Support : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Account")]
        public int? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Query Type")]
        [Required]
        public int QueryTypeId { get; set; }
        [ForeignKey("QueryTypeId")]
        public QueryType QueryType { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Description")]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Status")]
        [Required]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Case No.")]
        public string CaseNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}