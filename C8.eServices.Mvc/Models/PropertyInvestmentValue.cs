using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
   public class PropertyInvestmentValue: BaseModel
    {
        [Column(Order=2)]
        public int PropertyInvestmentId { get; set; }
        [ForeignKey("PropertyInvestmentId")]
        public PropertyInvestment PropertyInvestment { get; set; }

        [Column(Order=3)]
        public int InvestmentValueId { get; set; }
        [ForeignKey("InvestmentValueId")]
        public InvestmentValue InvestmentValue { get; set; }
    }
}
