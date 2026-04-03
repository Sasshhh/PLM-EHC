using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyLeaseAgreementMaster: BaseModel
    {
        [Display(Name = "LeaseDetails")]
        [Column(Order = 10)]
        public int? LeaseDetailsId { get; set; }
        public LeaseDetails LeaseDetails { get; set; }

        [Display(Name = "PropertyLeaseApplication")]
        [Column(Order = 11)]
        public int? PropertyLeaseApplicationId { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 12)]
        [Display(Name = "RepresentedBy")]
        public string RepresentedBy { get; set; }

        [Column(Order = 13)]
        [Display(Name = "ApplicantFullName")]
        public string ApplicantFullName { get; set; }

        [Column(Order = 14)]
        [Display(Name = "PreparationFee")]
        public double PreparationFee { get; set; }
        [Column(Order = 15)]
        [Display(Name = "CreditCheckFee")]
        public double CreditCheckFee { get; set; }

        //refundable initial deposit
        [Column(Order = 16)]
        [Display(Name = "CalculatedAsFolllows")]
        public double CalculatedAsFolllows { get; set; }

        [Column(Order = 17)]
        [Display(Name = "InitialDepositPremises")]
        public double InitialDepositPremises { get; set; }

        [Column(Order = 18)]
        [Display(Name = "DepositTenantContribution")]
        public double DepositTenantContribution { get; set; }

        //monthly payments
        [Column(Order = 19)]
        [Display(Name = "MonthlyUnitRental")]
        public double MonthlyUnitRental { get; set; }

        [Column(Order = 20)]
        [Display(Name = "ShadePortParking")]
        public double ShadePortParking { get; set; }

        [Column(Order = 21)]
        [Display(Name = "ShadePortParking")]
        public bool? SPP { get; set; }

        [Column(Order = 22)]
        [Display(Name = "OpenParking")]
        public double OpenParking { get; set; }

        [Column(Order = 23)]
        [Display(Name = "OpenParking")]
        public bool? OPP { get; set; }

        [Column(Order = 24)]
        [Display(Name = "StoreRooms")]
        public double StoreRooms { get; set; }

        [Column(Order = 25)]
        [Display(Name = "OpenParking")]
        public bool? STR { get; set; }

        //Monthly contributions
        [Column(Order = 26)]
        [Display(Name = "Electricity")]
        public double Electricity { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Electricity")] 
        public bool? ELEC { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Refuse")]
        public double Refuse { get; set; }

        [Column(Order = 29)]
        [Display(Name = "SecurityFee")]
        public double SecurityFee { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Electricity")]
        public bool? SEC { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Sewerage")]
        public double Sewerage { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Water")]
        public double Water { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Electricity")]
        public bool? WTR { get; set; }

        [Column(Order = 34)]
        [Display(Name = "UnitNumber")]
        public string UnitNumber { get; set; }

        [Column(Order = 35)]
        [Display(Name = "UnitNumber")]
        public int BedRooms { get; set; }

        [Column(Order = 36)]
        [Display(Name = "FloorNumber")]
        public string FloorNumber { get; set; }

        [Column(Order = 37)]
        [Display(Name = "BlockNumber")]
        public string BlockNumber { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Day")]
        public string Day { get; set; }

        [Column(Order = 39)]
        [Display(Name = "Date")]
        public string CommencementDate { get; set; }

        [Column(Order = 40)]
        [Display(Name = "Date")]
        public string EndDate { get; set; }

        [Column(Order =41)]
        [Display(Name = "Month")]
        public string NoPenaltyMonth { get; set; }

        [Column(Order = 42)]
        [Display(Name = "Rental Due Untill")]
        public string RentalDueUntill { get; set; }

        [Column(Order = 43)]
        [Display(Name = "PenaltyMonth")]
        public string PenaltyMonth { get; set; }

        [Column(Order = 44)]
        [Display(Name = "PenaltyMonth")]
        public double InitialDepositAmonunt { get; set; }

        [Column(Order = 45)]
        [Display(Name = "PenaltyMonth")]
        public double LeaseAdministrationFee { get; set; }

        [Column(Order = 46)]
        [Display(Name = "PenaltyMonth")]
        public double UnitRentalAmountPM { get; set; }

        [Column(Order = 47)]
        [Display(Name = "PenaltyMonth")]
        public string UnitRentalDay { get; set; }

        [Column(Order = 48)]
        [Display(Name = "PenaltyMonth")]
        public string UnitRentalDate { get; set; }

        [Column(Order = 49)]
        [Display(Name = "PenaltyMonth")]
        public string RentalIncreaseDate { get; set; }

        //parking rental
        [Column(Order = 50)]
        [Display(Name = "PenaltyMonth")]
        public string CarportParkingBayNumber { get; set; }

        [Column(Order = 51)]
        [Display(Name = "PenaltyMonth")]
        public string OPenParkingBayNumber { get; set; }

        [Column(Order = 52)]
        [Display(Name = "PenaltyMonth")]
        public double OPenParkingBayRental { get; set; }

        [Column(Order = 53)]
        [Display(Name = "PenaltyMonth")]
        public string ShadePortBayNumber { get; set; }

        [Column(Order = 54)]
        [Display(Name = "PenaltyMonth")]
        public double ShadePortBayRental { get; set; }

        [Column(Order = 55)]
        [Display(Name = "PenaltyMonth")]
        public string _Of1July { get; set; }

        [Column(Order = 56)]
        [Display(Name = "PenaltyMonth")]
        public string ParkingIncreaseDay { get; set; }

        [Column(Order = 57)]
        [Display(Name = "PenaltyMonth")]
        public string ParkingIncreaseMonth { get; set; }

        [Column(Order = 58)]
        [Display(Name = "PenaltyMonth")]
        public double _water { get; set; }

        [Column(Order = 59)]
        [Display(Name = "PenaltyMonth")]
        public double _refuse { get; set; }

        [Column(Order = 60)]
        [Display(Name = "PenaltyMonth")]
        public double _sewerage { get; set; }

        [Column(Order = 61)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantONE { get; set; }

        [Column(Order = 62)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantONEIdentityNo { get; set; }

        [Column(Order = 63)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantTWO { get; set; }

        [Column(Order = 64)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantTWOIdentityNo { get; set; }

        [Column(Order = 65)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantTHREE { get; set; }

        [Column(Order = 66)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantTHREEIdentityNo { get; set; }

        [Column(Order = 67)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantFOUR { get; set; }

        [Column(Order = 68)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantFOURIdentityNo { get; set; }

        [Column(Order = 69)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantFIVE { get; set; }

        [Column(Order = 70)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantFIVEIdentityNo { get; set; }

        [Column(Order = 71)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantSIX { get; set; }

        [Column(Order = 72)]
        [Display(Name = "PenaltyMonth")]
        public string OccupantSIXIdentityNo { get; set; }

        [Column(Order = 73)]
        [Display(Name = "PenaltyMonth")]
        public int PeopleAllowedOnPremises { get; set; }

        [Column(Order = 74)]
        [Display(Name = "PenaltyMonth")]
        public string LandlordAddress { get; set; }

        [Column(Order = 75)]
        [Display(Name = "PenaltyMonth")]
        public string TenantSignDay { get; set; }

        [Column(Order = 76)]
        [Display(Name = "PenaltyMonth")]
        public string TenantSignDate { get; set; }

        [Column(Order = 77)]
        [Display(Name = "PenaltyMonth")]
        public string TenantWitnessONE { get; set; }

        [Column(Order = 78)]
        [Display(Name = "PenaltyMonth")]
        public string TenantWitnessTWO { get; set; }

        [Column(Order = 79)]
        [Display(Name = "PenaltyMonth")]
        public string TenantSignature { get; set; }

        [Column(Order = 80)]
        [Display(Name = "PenaltyMonth")]
        public bool TenantSigned { get; set; }

        [Column(Order = 81)]
        [Display(Name = "PenaltyMonth")]
        public string ManagersSignDay { get; set; }

        [Column(Order = 82)]
        [Display(Name = "PenaltyMonth")]
        public string ManagersSignDate { get; set; }

        [Column(Order = 83)]
        [Display(Name = "PenaltyMonth")]
        public string ManagersWitnessONE { get; set; }

        [Column(Order = 84)]
        [Display(Name = "PenaltyMonth")]
        public string ManagersWitnessTWO { get; set; }

        [Column(Order = 85)]
        [Display(Name = "PenaltyMonth")]
        public string PropertyManagersSignature { get; set; }

        [Column(Order = 86)]
        [Display(Name = "PenaltyMonth")]
        public int? PropertyManagerId { get; set; }

        [Column(Order = 87)]
        [Display(Name = "PenaltyMonth")]
        public bool PropertyManagerSigned { get; set; }

        [Column(Order = 88)]
        [Display(Name = "Property Manager Signature Date")]
        public DateTime? PropertyManagerSignatureDate { get; set; }

        [Column(Order = 89)]
        [Display(Name = "PenaltyMonth")]
        public string RevenueManagersSignature { get; set; }

        [Column(Order = 90)]
        [Display(Name = "PenaltyMonth")]
        public int? RevenueManagerId { get; set; }

        [Column(Order = 91)]
        [Display(Name = "PenaltyMonth")]
        public bool RevenueManagerSigned { get; set; }

        [Column(Order = 92)]
        [Display(Name = "Revenue Manager Signature Date")]
        public DateTime? RevenueManagerSignatureDate { get; set; }

        [Column(Order = 93)]
        [Display(Name = "PenaltyMonth")]
        public string SignatureMainLessee { get; set; }

        [Column(Order = 94)]
        [Display(Name = "PenaltyMonth")]
        public bool MainLesseeSigned { get; set; }

        [Column(Order = 95)]
        [Display(Name = "PenaltyMonth")]
        public string SignatureOfSpouse { get; set; }

        [Column(Order = 96)]
        [Display(Name = "PenaltyMonth")]
        public bool SpouseSigned { get; set; }

        [Column(Order = 97)]
        [Display(Name = "PenaltyMonth")]
        public string ApplicantIdentityNumber { get; set; }

        [Column(Order = 108)]
        public string Witness1Signature { get; set; }

        [Column(Order = 109)]
        [StringLength(200)]
        public string Witness1Name { get; set; }

        [Column(Order = 110)]
        public DateTime? Witness1SignatureDate { get; set; }

        // LEASE AGREEMENT ENHANCEMENTS
        [Column(Order = 111)]
        [Display(Name = "Access Card Deposit")]
        public decimal AccessCardDeposit { get; set; }

        [Column(Order = 112)]
        [Display(Name = "Key Deposit")]
        public decimal KeyDeposit { get; set; }

        [Column(Order = 113)]
        [Display(Name = "DSTV Activation Fee")]
        public decimal DSTVActivationFee { get; set; }

        [Column(Order = 114)]
        [Display(Name = "DSTV Monthly Levy")]
        public decimal DSTVMonthlyLevy { get; set; }

        [Column(Order = 115)]
        [Display(Name = "Has DSTV")]
        public bool? HasDSTV { get; set; }

        [Column(Order = 116)]
        [Display(Name = "Commencement Day")]
        [StringLength(2)]
        public string CommencementDay { get; set; }

        // Banking Details for Debit Order (Added for banking details integration)
        [Column(Order = 117)]
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string TenantBankName { get; set; }

        [Column(Order = 118)]
        [Display(Name = "Account Number")]
        [StringLength(20)]
        public string TenantAccountNumber { get; set; }

        [Column(Order = 119)]
        [Display(Name = "Account Holder Name")]
        [StringLength(200)]
        public string TenantAccountHolderName { get; set; }

        [Column(Order = 120)]
        [Display(Name = "Account Type")]
        [StringLength(50)]
        public string TenantAccountType { get; set; }

        [Column(Order = 121)]
        [Display(Name = "Branch Code")]
        [StringLength(10)]
        public string TenantBranchCode { get; set; }
    }
}