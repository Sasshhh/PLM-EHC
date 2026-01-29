using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HumanSettlementAgreementMaster : BaseModel
    {
        [Display(Name = "HumanSettlementLeaseMaster")]
        [Column(Order = 10)]
        public int? HumanSettlementLeaseMasterId { get; set; }
        [ForeignKey("HumanSettlementLeaseMasterId")]
        public HumanSettlementLeaseMaster HumanSettlementLeaseMaster { get; set; }

        [Display(Name = "HumanSettlementApplication")]
        [Column(Order = 11)]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementLeaseMasterId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Display(Name = "HumanSettlementLeaseDetails")]
        [Column(Order = 12)]
        public int? HumanSettlementLeaseDetailsId { get; set; }
        [ForeignKey("HumanSettlementLeaseDetailsId")]
        public HumanSettlementLeaseDetails HumanSettlementLeaseDetails { get; set; }

        [Column(Order = 13)]
        [Display(Name = "ApplicantFullName")]
        public string ApplicantFullName { get; set; }

        [Column(Order = 14)]
        [Display(Name = "IdentityNumber")]
        public string IdentityNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "TheUnit")]
        public string TheUnit { get; set; }

        [Column(Order = 16)]
        [Display(Name = "StartDate")]
        public string  StartDate { get; set; }

        [Column(Order = 17)]
        [Display(Name = "EndDate")]
        public string EndDate { get; set; }

        [Column(Order = 18)]
        [Display(Name = "PayableRent")]
        public double PayableRent { get; set; }

        [Column(Order = 19)]
        [Display(Name = "NomineeFirstName")]
        public string NomineeFirstName { get; set; }

        [Column(Order = 20)]
        [Display(Name = "NomineeTitle")]
        public string NomineeTitle { get; set; }

        [Column(Order = 21)]
        [Display(Name = "NomineeLastName")]
        public string NomineeLastName { get; set; }

        [Column(Order = 22)]
        [Display(Name = "NomineeIDNo")]
        public string NomineeIDNo { get; set; }

        [Column(Order = 23)]
        [Display(Name = "NomineeRelationship")]
        public string NomineeRelationship { get; set; }

        [Column(Order = 24)]
        [Display(Name = "NomineeContact")]
        public string NomineeContact { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Singnature")]
        public string Singnature { get; set; }

        [Column(Order = 26)]
        [Display(Name = "NomineeSignature")]
        public string NomineeSignature { get; set; }

        [Column(Order = 27)]
        [Display(Name = "FirstWitness")] 
        public string  FirstWitness { get; set; }

        [Column(Order = 28)]
        [Display(Name = "LastWitness")]
        public string LastWitness { get; set; }

        [Column(Order = 29)]
        [Display(Name = "SignedAt")]
        public string SignedAt { get; set; }

        [Column(Order = 30)]
        [Display(Name = "LessorSignature")]
        public string LessorSignature { get; set; }

        [Column(Order = 31)]
        [Display(Name = "LessorFirstWitness")]
        public string LessorFirstWitness { get; set; }

        [Column(Order = 32)]
        [Display(Name = "LessorLasttWitness")]
        public string LessorLastWitness { get; set; }
    }
}