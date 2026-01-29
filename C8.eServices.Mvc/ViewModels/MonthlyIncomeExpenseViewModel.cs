using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace C8.eServices.Mvc.ViewModels
{
    public class MonthlyIncomeExpenseViewModel
    {
        //Monthly Income
        [Display(Name = "Gross Income")]
        [Column(Order = 10)]
        public decimal? GrossIncome { get; set; }
        [Display(Name = "Allowances")]
        [Column(Order = 11)]
        public decimal? Allowances { get; set; }
        [Display(Name = "Fringe Benefits")]
        [Column(Order = 12)]
        public decimal? FringeBenefits { get; set; }
        [Display(Name = "Other Regular Income")]
        [Column(Order = 13)]
        public decimal? OtherRegularIncome { get; set; }
        [Display(Name = "Total Gross Income")]
        [Column(Order = 14)]
        public decimal? TotalGrossIncome { get; set; }
        [Display(Name = "Paye Tax Less Deductions")]
        [Column(Order = 15)]
        public decimal? PayeTaxLessDeductions { get; set; }
        [Display(Name = "Pension Provident Less Deductions")]
        [Column(Order = 16)]
        public decimal? PensionProvidentLessDeductions { get; set; }
        [Display(Name = "UIF Less Deductions")]
        [Column(Order = 17)]
        public decimal? UIFLessDeductions { get; set; }
        [Display(Name = "Medical Aid Less Deductions")]
        [Column(Order = 18)]
        public decimal? MedicalAidLessDeductions { get; set; }
        [Display(Name = "Other Less Deductions")]
        [Column(Order = 19)]
        public decimal? OtherLessDeductions { get; set; }
        [Display(Name = "Total Deductions")]
        [Column(Order = 20)]
        public decimal? TotalDeductions { get; set; }
        [Display(Name = "Net Income")]
        [Column(Order = 21)]
        public decimal? NetIncome { get; set; }
        [Display(Name = "Other Dividends Income")]
        [Column(Order = 22)]
        public decimal? OtherDividendsIncome { get; set; }
        [Display(Name = "Total Net Income")]
        [Column(Order = 23)]
        public decimal? TotalNetIncome { get; set; }


        [Display(Name = "Gross Income")]
        [Column(Order = 24)]
        public decimal? SecGrossIncome { get; set; }
        [Display(Name = "Allowances")]
        [Column(Order = 25)]
        public decimal? SecAllowances { get; set; }
        [Display(Name = "Fringe Benefits")]
        [Column(Order = 26)]
        public decimal? SecFringeBenefits { get; set; }
        [Display(Name = "Other Regular Income")]
        [Column(Order = 27)]
        public decimal? SecOtherRegularIncome { get; set; }
        [Display(Name = "Total Gross Income")]
        [Column(Order = 28)]
        public decimal? SecTotalGrossIncome { get; set; }
        [Display(Name = "Paye Tax Less Deductions")]
        [Column(Order = 29)]
        public decimal? SecPayeTaxLessDeductions { get; set; }
        [Display(Name = "Pension Provident Less Deductions")]
        [Column(Order = 30)]
        public decimal? SecPensionProvidentLessDeductions { get; set; }
        [Display(Name = "UIF Less Deductions")]
        [Column(Order = 31)]
        public decimal? SecUIFLessDeductions { get; set; }
        [Display(Name = "Medical Aid Less Deductions")]
        [Column(Order = 32)]
        public decimal? SecMedicalAidLessDeductions { get; set; }
        [Display(Name = "Other Less Deductions")]
        [Column(Order = 33)]
        public decimal? SecOtherLessDeductions { get; set; }
        [Display(Name = "Total Deductions")]
        [Column(Order = 34)]
        public decimal? SecTotalDeductions { get; set; }
        [Display(Name = "Net Income")]
        [Column(Order = 35)]
        public decimal? SecNetIncome { get; set; }
        [Display(Name = "Other Dividends Income")]
        [Column(Order = 36)]
        public decimal? SecOtherDividendsIncome { get; set; }
        [Display(Name = "Total Net Income")]
        [Column(Order = 37)]
        public decimal? SecTotalNetIncome { get; set; }


        //Monthly Expenses

        //House Expenses
        [Display(Name = "Accommodation: Home Rental")]
        [Column(Order = 38)]
        public decimal? HEAccommodation { get; set; }
        [Display(Name = "Insurances")]
        [Column(Order = 39)]
        public decimal? HEInsurances { get; set; }
        [Display(Name = "Rates and Taxes")]
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
        [Display(Name = "Utilities: Water")]
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

        //Second Applicant House Expenses
        [Display(Name = "Accommodation: Home loan Board Rent ")]
        [Column(Order = 38)]
        public decimal? SecHEAccommodation { get; set; }
        [Display(Name = "Insurances")]
        [Column(Order = 39)]
        public decimal? SecHEInsurances { get; set; }
        [Display(Name = "Rates and Taxes")]
        [Column(Order = 40)]
        public decimal? SecHERatesTaxes { get; set; }
        [Display(Name = "Security")]
        [Column(Order = 41)]
        public decimal? SecHESecurity { get; set; }
        [Display(Name = "Upkeep (e.g. house and garden)")]
        [Column(Order = 42)]
        public decimal? SecHEUpkeep { get; set; }
        [Display(Name = "Utilities: Electricity")]
        [Column(Order = 43)]
        public decimal? SecHEUtilitiesElectricity { get; set; }
        [Display(Name = "Utilities: Water")]
        [Column(Order = 44)]
        public decimal? SecHEUtilitiesWater { get; set; }
        [Display(Name = "Other")]
        [Column(Order = 45)]
        public decimal? SecHEOthers { get; set; }

        //Vehicle Expenses
        [Display(Name = "Fuel/Petrol/Diesel")]
        [Column(Order = 46)]
        public decimal? SecVEFuel { get; set; }
        [Display(Name = "Insurance ")]
        [Column(Order = 47)]
        public decimal? SecVEInsurance { get; set; }
        [Display(Name = "Maintenance")]
        [Column(Order = 48)]
        public decimal? SecVEMaintenance { get; set; }
        [Display(Name = "Vehicle finance - Car loan")]
        [Column(Order = 49)]
        public decimal? SecVEVehicleFinance { get; set; }

        //Policies
        [Display(Name = "Life Assurances")]
        [Column(Order = 50)]
        public decimal? SecELifeAssurances { get; set; }
        [Display(Name = "Short Term Insurances ")]
        [Column(Order = 51)]
        public decimal? SecEShortTermInsurances { get; set; }
        [Display(Name = "Other Insurances Funeral")]
        [Column(Order = 52)]
        public decimal? SecEOtherInsurancesFuneral { get; set; }

        //Living expenses
        [Display(Name = "Support/Alimony/Maintenance")]
        [Column(Order = 53)]
        public decimal? SecLESupportMaintenance { get; set; }
        [Display(Name = "Bank charges/costs")]
        [Column(Order = 54)]
        public decimal? SecLEBankCharges { get; set; }
        [Display(Name = "Cellular/Airtime/Data")]
        [Column(Order = 55)]
        public decimal? SecLECellularAirtimeData { get; set; }
        [Display(Name = "Clothing")]
        [Column(Order = 56)]
        public decimal? SecLEClothing { get; set; }
        [Display(Name = "Credit Cards")]
        [Column(Order = 57)]
        public decimal? SecLECreditCards { get; set; }
        [Display(Name = "Domestic Employees")]
        [Column(Order = 58)]
        public decimal? SecLEDomesticEmployees { get; set; }
        [Display(Name = "Donations")]
        [Column(Order = 59)]
        public decimal? SecLEDonations { get; set; }
        [Display(Name = "Education/School Fees")]
        [Column(Order = 60)]
        public decimal? SecLEEducationSchool { get; set; }
        [Display(Name = "Entertainment")]
        [Column(Order = 61)]
        public decimal? SecLEEntertainment { get; set; }
        [Display(Name = "Groceries")]
        [Column(Order = 62)]
        public decimal? SecLEGroceries { get; set; }
        [Display(Name = "Instalment Accounts")]
        [Column(Order = 63)]
        public decimal? SecLEInstalmentAccounts { get; set; }
        [Display(Name = "Medical Aid, Health Professionals, Homeopaths,Chemists")]
        [Column(Order = 64)]
        public decimal? SecLEMedicalAid { get; set; }
        [Display(Name = "Memberships")]
        [Column(Order = 65)]
        public decimal? SecLEMemberships { get; set; }
        [Display(Name = "Personal Loans")]
        [Column(Order = 66)]
        public decimal? SecLEPersonalLoans { get; set; }
        [Display(Name = "Pet Care")]
        [Column(Order = 67)]
        public decimal? SecLEPetCare { get; set; }
        [Display(Name = "Retail Accounts")]
        [Column(Order = 68)]
        public decimal? SecLERetailAccounts { get; set; }
        [Display(Name = "Security")]
        [Column(Order = 69)]
        public decimal? SecLESecurity { get; set; }
        [Display(Name = "Subscriptions")]
        [Column(Order = 70)]
        public decimal? SecLESubscriptions { get; set; }
        [Display(Name = "Telephones")]
        [Column(Order = 71)]
        public decimal? SecLETelephones { get; set; }
        [Display(Name = "Transport")]
        [Column(Order = 72)]
        public decimal? SecLETransport { get; set; }
        [Display(Name = "TV / Mnet / DSTV / Netflix")]
        [Column(Order = 73)]
        public decimal? SecLETV { get; set; }
        [Display(Name = "Any other expenses not listed")]
        [Column(Order = 74)]
        public decimal? SecLEOtherExpenses { get; set; }
        [Display(Name = "Total expenses per month")]
        [Column(Order = 75)]
        public decimal? SecLETotalExpenses { get; set; }
    }
}
