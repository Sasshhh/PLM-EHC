using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RCSApplicationStatusAudit : BaseModelAudit
    {

        [Column(Order = 10)]
        [Display(Name = "Application Reference Number")]
        [MaxLength(50)]
        public string ApplicationReferenceNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        [ScriptIgnore]
        [JsonIgnore]
        public Customer Customer { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Transfer Information")]
        public int TransferInformationId { get; set; }
        [ForeignKey("TransferInformationId")]
        public TransferInformation TransferInformation { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Municipal Information")]
        public int MunicipalAccountInformationId { get; set; }
        [ForeignKey("MunicipalAccountInformationId")]
        public MunicipalAccountInformation MunicipalAccountInformation { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Clerk")]
        public int? ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        public Customer Clerk { get; set; }
        [Column(Order = 16)]
        public decimal amount { get; set; }
        [Column(Order = 17)]
        [Display(Name = "CCC")]
        public int? CCCId { get; set; }
        [ForeignKey("CCCId")]
        public CCC CCC { get; set; }


        [Column(Order = 18)]
        [Display(Name = "Assessment Figure Upload Date ")]
        public DateTime? AssessmentFigureUploadDate { get; set; }

        [Column(Order = 19)]

        [Display(Name = " Assessment Figures Expired")]
        public bool AssessmentFiguresExpired { get; set; }


        [Column(Order = 20)]

        [Display(Name = "Check Assessment Figure Expiry")]
        public bool CheckAssessmentFigureExpiry { get; set; }


        [Column(Order = 21)]
        [Display(Name = "Assessment Figure Expiry Date ")]

        public DateTime? AssessmentFigureExpiryDate { get; set; }
        [Column(Order = 22)]
        [Display(Name = "Assessment Figure amount outstanding ")]

        public decimal AssessmentFigureAmountOutstanding { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Assessment Figure Amount Paid ")]

        public decimal AssessmentFigureAmountPaid { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Assessment Figure Total Amount ")]

        public decimal AssessmentFigureTotalAmount { get; set; }

        [Column(Order = 25)]

        [Display(Name = " Assessment Figures Fully Paid")]
        public bool AssessmentFiguresFullyPaid { get; set; }


        [Column(Order = 26)]

        [Display(Name = "Figures Paid Online")]
        public bool FiguresPaymentOnline { get; set; }

        [Column(Order = 27)]

        [Display(Name = "Application Fee Paid Online")]
        public bool ApplicationFeePaymentOnline { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Payment Method")]
        [MaxLength(50)]
        public string ApplicationFeePayMethod { get; set; }


        [Column(Order = 29)]
        [Display(Name = "Payment date")]

        public string ApplicationFeePaymentDate { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Amount")]
        [MaxLength(50)]
        public string ApplicationFeeAmount { get; set; }


        [Column(Order = 31)]
        [Display(Name = "Amount")]
        [MaxLength(50)]
        public string ApplicationFeeStatus { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Amount")]
        [MaxLength(100)]
        public string ApplicationFeeReceiptNumber { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Assessment Figure End Date")]
        public DateTime? AssessmentFiguresEndDate { get; set; }


    }
}