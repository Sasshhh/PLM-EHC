using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class PropertyInvestmentAudit: BaseModelAudit
    {

        [Display(Name = "Investment Description")]
        public string InvestmentDescription { get; set; }

        [Display(Name = "Type of Services")]
        [DataType(DataType.MultilineText)]
        public string TypeOfServices { get; set; }

        [Display(Name = "BBBEE Status")]
        public string BbbeeStatus { get; set; }

        [Display(Name = "eThekwini Municipality Supplier Number")]
        public string SupplierNumber { get; set; }

        [Display(Name = "Standard Industrial Classification")]
        public string SicCode { get; set; }

        [Display(Name = "Duration of Development")]
        public string DevelopmentPeriod { get; set; }

        [Column(Order = 8)]
        [Display(Name = "VAT Number")]
        public string VatNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string IncentiveProgrammeCurrentBenefits { get; set; }

        [DataType(DataType.MultilineText)]
        public string ImpactOnSkillDevelopment { get; set; }

        [DataType(DataType.MultilineText)]
        public string SupportServices { get; set; }

        [Display(Name = "Projected Start Date")]
        public DateTime ProjectedStartDateTime { get; set; }

        [Display(Name = "Projected End Date")]
        public DateTime ProjectedEndDateTime { get; set; }

        public int IncentivePolicyPropertyId { get; set; }
        [ForeignKey("IncentivePolicyPropertyId")]
        public IncentivePolicyProperty IncentivePolicyProperty { get; set; }
        public string TaxNumber { get; set; }
    }
}
