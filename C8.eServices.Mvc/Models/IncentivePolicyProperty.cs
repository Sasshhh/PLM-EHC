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
   public class IncentivePolicyProperty:BaseModel
    {
        [Column(Order=2)]
        public int IncentivePolicyId { get; set; }
        [ForeignKey("IncentivePolicyId")]
        public IncentivePolicy IncentivePolicy { get; set; }

        [Column(Order = 3)]
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        [Column(Order=4)]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

    }
}
