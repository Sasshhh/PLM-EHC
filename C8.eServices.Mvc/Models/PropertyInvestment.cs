using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace C8.eServices.Mvc.Models
{
   public class PropertyInvestment: BaseModel
    {
        [Column(Order=2)]
        [Display(Name= "Provide a description of your investment and how it conforms to the provisions as mentioned in the EDIP guideline")]
        public string InvestmentDescription { get; set; }

        [Column(Order=3)]
        [Display(Name = "Type of Services")]
        [DataType(DataType.MultilineText)]
        public string TypeOfServices { get; set; }

        [Column(Order=4)]
        [Display(Name="BBBEE Status")]
        public string BbbeeStatus { get; set; }

        [Column(Order=5)]
        [Display(Name= "eThekwini Municipality Supplier Number")]
        public string SupplierNumber { get; set; }

        [Column(Order=6)]
        [Display(Name="Standard Industrial Classification")]
        public string SicCode { get; set; }

        [Column(Order=7)]
        [Display(Name="Duration of Development")]
        public string DevelopmentPeriod { get; set; }

        [Column(Order = 8)]
        [Display(Name = "VAT Number")]
        public string VatNumber { get; set; }

        [Column(Order=10)]
        [DataType(DataType.MultilineText)]
        public string IncentiveProgrammeCurrentBenefits { get; set; } 

        [Column(Order=11)]
        [DataType(DataType.MultilineText)]
        public string ImpactOnSkillDevelopment { get; set; }

        [Column(Order=12)]
        [DataType(DataType.MultilineText)]
        public string SupportServices { get; set; }

        [Column(Order=13)]
        [Required]
        [Display(Name="Projected Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ProjectedStartDateTime { get; set; }
        [Column(Order = 14)]
        [Required]
        [Display(Name = "Projected End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ProjectedEndDateTime { get; set; }
         
        [Column(Order=15)]
        public int IncentivePolicyPropertyId { get; set; }
        [ForeignKey("IncentivePolicyPropertyId")]
        public IncentivePolicyProperty IncentivePolicyProperty { get; set; }

        [Column(Order = 16)]
        [Display(Name="Tax Number")]
        public string TaxNumber { get; set; }
    }
}
