using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MonthlyExpense : BaseModel
    {
        //House Expenses
        [Display(Name = "Accommodation: Home loan Board Rent ")]
        [Column(Order = 38)]
        public decimal? HEAccommodation { get; set; }
        [Display(Name = "Insurances")]
        [Column(Order = 39)]
        public decimal? HEInsurances { get; set; }
        [Display(Name = "Insurances")]
        [Column(Order = 40)]
        public decimal? HERatesTaxes { get; set; }
        [Display(Name = "Security")]
        [Column(Order = 41)]
        public decimal? HESecurity { get; set; }
        [Display(Name = "Upkeep (e.g. house and garden)")]
        [Column(Order = 42)]
        public decimal? HEUpkeep { get; set; }
        [Display(Name = "Utilities: Electricity")]
        [Column(Order = 43)]
        public decimal? HEUtilitiesElectricity { get; set; }
        [Display(Name = "Utilities: Electricity")]
        [Column(Order = 44)]
        public decimal? HEUtilitiesWater { get; set; }
        [Display(Name = "Other")]
        [Column(Order = 45)]
        public decimal? HEOthers { get; set; }

        //Vehicle Expenses
        [Display(Name = "Fuel/Petrol/Diesel")]
        [Column(Order = 46)]
        public decimal? VEFuel { get; set; }
        [Display(Name = "Insurance ")]
        [Column(Order = 47)]
        public decimal? VEInsurance { get; set; }
        [Display(Name = "Maintenance")]
        [Column(Order = 48)]
        public decimal? VEMaintenance { get; set; }
        [Display(Name = "Vehicle finance - Car loan")]
        [Column(Order = 49)]
        public decimal? VEVehicleFinance { get; set; }

        //Policies
        [Display(Name = "Life Assurances")]
        [Column(Order = 50)]
        public decimal? ELifeAssurances { get; set; }
        [Display(Name = "Short Term Insurances ")]
        [Column(Order = 51)]
        public decimal? EShortTermInsurances { get; set; }
        [Display(Name = "Other Insurances Funeral")]
        [Column(Order = 52)]
        public decimal? EOtherInsurancesFuneral { get; set; }
        //Living expenses
        [Display(Name = "Support/Alimony/Maintenance")]
        [Column(Order = 53)]
        public decimal? LESupportMaintenance { get; set; }
        [Display(Name = "Bank charges/costs")]
        [Column(Order = 54)]
        public decimal? LEBankCharges { get; set; }
        [Display(Name = "Cellular/Airtime/Data")]
        [Column(Order = 55)]
        public decimal? LECellularAirtimeData { get; set; }
        [Display(Name = "Clothing")]
        [Column(Order = 56)]
        public decimal? LEClothing { get; set; }
        [Display(Name = "Credit Cards")]
        [Column(Order = 57)]
        public decimal? LECreditCards { get; set; }
        [Display(Name = "Domestic Employees")]
        [Column(Order = 58)]
        public decimal? LEDomesticEmployees { get; set; }
        [Display(Name = "Donations")]
        [Column(Order = 59)]
        public decimal? LEDonations { get; set; }
        [Display(Name = "Education/School Fees")]
        [Column(Order = 60)]
        public decimal? LEEducationSchool { get; set; }
        [Display(Name = "Entertainment")]
        [Column(Order = 61)]
        public decimal? LEEntertainment { get; set; }
        [Display(Name = "Groceries")]
        [Column(Order = 62)]
        public decimal? LEGroceries { get; set; }
        [Display(Name = "Instalment Accounts")]
        [Column(Order = 63)]
        public decimal? LEInstalmentAccounts { get; set; }
        [Display(Name = "Medical Aid, Health Professionals, Homeopaths,Chemists")]
        [Column(Order = 64)]
        public decimal? LEMedicalAid { get; set; }
        [Display(Name = "Memberships")]
        [Column(Order = 65)]
        public decimal? LEMemberships { get; set; }
        [Display(Name = "Personal Loans")]
        [Column(Order = 66)]
        public decimal? LEPersonalLoans { get; set; }
        [Display(Name = "Pet Care")]
        [Column(Order = 67)]
        public decimal? LEPetCare { get; set; }
        [Display(Name = "Retail Accounts")]
        [Column(Order = 68)]
        public decimal? LERetailAccounts { get; set; }
        [Display(Name = "Security")]
        [Column(Order = 69)]
        public decimal? LESecurity { get; set; }
        [Display(Name = "Subscriptions")]
        [Column(Order = 70)]
        public decimal? LESubscriptions { get; set; }
        [Display(Name = "Telephones")]
        [Column(Order = 71)]
        public decimal? LETelephones { get; set; }
        [Display(Name = "Transport")]
        [Column(Order = 72)]
        public decimal? LETransport { get; set; }
        [Display(Name = "TV / Mnet / DSTV / Netflix")]
        [Column(Order = 73)]
        public decimal? LETV { get; set; }
        [Display(Name = "Any other expenses not listed")]
        [Column(Order = 74)]
        public decimal? LEOtherExpenses { get; set; }
        [Display(Name = "Total expenses per month")]
        [Column(Order = 75)]
        public decimal? LETotalExpenses { get; set; }

        [Column(Order = 24)]
        [Display(Name = "PropertyLeaseApplication")]
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 25)]
        [Display(Name = "ApplicantTypeId")]
        public int ApplicantTypeId { get; set; }
        [ForeignKey("ApplicantTypeId")]
        public ApplicantType ApplicantType { get; set; }
    }
}