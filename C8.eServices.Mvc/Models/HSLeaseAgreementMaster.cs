using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HSLeaseAgreementMaster: BaseModel
    {
        [Display(Name = "HumanSettlementApplication")]
        [Column(Order = 10)]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 12)]
        [Display(Name = "RepresentedBy")]
        public string ApplicantFullName { get; set; }

        [Column(Order = 13)]
        [Display(Name = "ApplicantFullName")]
        public string IdentityNumber { get; set; }

        [Column(Order = 14)]
        [Display(Name = "PreparationFee")]
        public string TheUnit { get; set; }

        [Column(Order = 15)]
        [Display(Name = "CreditCheckFee")]
        public string CommenceDate { get; set; }

        [Column(Order = 16)]
        [Display(Name = "CalculatedAsFolllows")]
        public string ExpireryDate { get; set; }

        [Column(Order = 17)]
        [Display(Name = "InitialDepositPremises")]
        public bool Nominated { get; set; }

        [Column(Order = 18)]
        [Display(Name = "DepositTenantContribution")]
        public string NoTitle { get; set; }

        [Column(Order = 19)]
        [Display(Name = "MonthlyUnitRental")]
        public string NoName { get; set; }

        [Column(Order = 20)]
        [Display(Name = "ShadePortParking")]
        public string NoSurname { get; set; }

        [Column(Order = 21)]
        [Display(Name = "ShadePortParking")]
        public string NoIdentityNumber { get; set; }

        [Column(Order = 22)]
        [Display(Name = "OpenParking")]
        public string NoRelationship { get; set; }

        [Column(Order = 23)]
        [Display(Name = "OpenParking")]
        public string NoContact { get; set; }

        [Column(Order = 24)]
        [Display(Name = "StoreRooms")]
        public string ApplicantSignature { get; set; }

        [Column(Order = 25)]
        [Display(Name = "OpenParking")]
        public string NomineeSignature { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Electricity")]
        public string aa_Witness_one { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Electricity")]
        public string aa_Witness_two { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Refuse")]
        public string LessorSignature { get; set; }

        [Column(Order = 29)]
        [Display(Name = "SecurityFee")]
        public string rr_Witness_one { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Electricity")]
        public string rr_Witness_two { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Electricity")]
        public double PayableAmount { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Electricity")]
        public string Lessor_sign_date { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Electricity")]
        public string Applicant_sign_date { get; set; }
    }
}