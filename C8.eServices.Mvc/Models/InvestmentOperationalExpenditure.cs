using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
   public class InvestmentOperationalExpenditure: BaseModel
    {
        [Column(Order=2)]
        public int PropertyInvestmentId { get; set; }
        [ForeignKey("PropertyInvestmentId")]
        public PropertyInvestment PropertyInvestment { get; set; }

        [Column(Order=3)]
        [Display(Name = "Outside RSA")]
        public decimal Foreign { get; set; }

        [Column(Order=4)]
        [Display(Name="Inside RSA: Outside eThekwini")]
        public decimal OutBound { get; set; }

        [Column(Order=5)]
        [Display(Name="Inside RSA: Within eThekwini")]
        public decimal InBound { get; set; }
    }
}
