using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LinkedAccount:BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [MaxLength(100)]
        [Column(Order = 12)]
        public string EmailAddress { get; set; }
        
        //[Column(Order = 13)]
        //public bool EmailBill { get; set; }

        [Column(Order = 13)]
        [Display(Name = "RecipientType")]
        public int RecipientTypeId { get; set; }
        [ForeignKey("RecipientTypeId")]
        public RecipientType RecipientType { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 15)]
        [Display(Name = "LinkedAccountType")]
        public int LinkedAccountTypeId { get; set; }
        [ForeignKey("LinkedAccountTypeId")]
        public LinkedAccountType LinkedAccountType { get; set; }

        [MaxLength(16)]
        [Column(Order = 16)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }
    }
}