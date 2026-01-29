using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class PropertyInvestmentMeasurementAudit: BaseModelAudit
    {
        public int PropertyInvestmentId { get; set; }
        [ForeignKey("PropertyInvestmentId")]
        public PropertyInvestment PropertyInvestment { get; set; }

        public int Size { get; set; }

        [Display(Name = "Unit Of Measure")]
        public int UnitOfMeasureTypeId { get; set; }
        [ForeignKey("UnitOfMeasureTypeId")]
        public UnitOfmeasureType UnitOfmeasureType { get; set; }

        [Display(Name = "Measurement Type")]
        public int MeasurementTypeId { get; set; }
        [ForeignKey("MeasurementTypeId")]
        public MeasurementType MeasurementType { get; set; }
    }
}
