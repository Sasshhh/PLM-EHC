using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
    public class PropertyInvestmentMeasurement : BaseModel
    {
        [Column(Order = 2)]
        public int PropertyInvestmentId { get; set; }
        [ForeignKey("PropertyInvestmentId")]
        public PropertyInvestment PropertyInvestment { get; set; }

        [Column(Order = 3)]
        public int Size { get; set; }

        [Column(Order = 4)]
        [Display(Name = "Unit Of Measure")]
        public int UnitOfMeasureTypeId { get; set; } 
        [ForeignKey("UnitOfMeasureTypeId")]
        public UnitOfmeasureType UnitOfmeasureType { get; set; }

        [Column(Order = 5)]
        [Display(Name = "Measurement Type")]
        public int MeasurementTypeId { get; set; }
        [ForeignKey("MeasurementTypeId")]
        public MeasurementType MeasurementType { get; set; }
    }
}
