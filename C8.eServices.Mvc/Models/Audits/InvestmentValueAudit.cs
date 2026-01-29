using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
    public class InvestmentValueAudit:BaseModelAudit
    {
        public int InvestmentValueTypeId { get; set; }
        [ForeignKey("InvestmentValueTypeId")]
        public InvestmentValueType InvestmentValueType { get; set; }

        [Display(Name = "Year 1")]
        public decimal ValueYear1 { get; set; }

        [Display(Name = "Year 2")]
        public decimal? ValueYear2 { get; set; }

        [Display(Name = "Year 3")]
        public decimal? ValueYear3 { get; set; }

        [Display(Name = "Year 4")]
        public decimal? ValueYear4 { get; set; }

        [Display(Name = "Year 5")]
        public decimal? ValueYear5 { get; set; }

        [Display(Name = "Total")]
        public decimal ValueTotal { get; set; }
    }
}
