using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class InvestmentOperationalExpenditureAudit: BaseModelAudit
    {
        public int PropertyInvestmentId { get; set; }
        [ForeignKey("PropertyInvestmentId")]
        public PropertyInvestment PropertyInvestment { get; set; }

        [Display(Name = "Outside RSA")]
        public int Foreign { get; set; }

        [Display(Name = "Inside RSA: Outside eThekwini")]
        public int OutBound { get; set; }

        [Display(Name = "Inside RSA: Within eThekwini")]
        public int InBound { get; set; }
    }
}
