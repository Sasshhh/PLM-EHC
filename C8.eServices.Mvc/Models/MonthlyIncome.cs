using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MonthlyIncome : BaseModel
    {
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