using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LinkedMobile : BaseModel
    {
        [Required]
        [Column(Order = 10)]
        [Display(Name = "Linked Account")]
        public int LinkedAccountId { get; set; }
        [ForeignKey("LinkedAccountId")]
        public LinkedAccount LinkedAccount { get; set; }

        [Required]
        [MaxLength(11)]
        [Column(Order = 12)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required]
        [Column(Order = 13)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Required]
        [Column(Order = 14)]
        [Display(Name = "Recipient Type")]
        public int RecipientTypeId { get; set; }
        [ForeignKey("RecipientTypeId")]
        public RecipientType RecipientType { get; set; }
    }
}