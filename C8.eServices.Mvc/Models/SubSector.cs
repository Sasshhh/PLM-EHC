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
   public  class SubSector: BaseModel
    {
        [Column(Order=2)]
        [Display(Name="Focus Area")]
        public int FocusAreaId { get; set; }
        [ForeignKey("FocusAreaId")]
        public FocusArea FocusArea { get; set; }

        [Column(Order=3)]
        [Display(Name="Sub-Sector")]
        [MaxLength(100)]
        public string SubSectorName { get; set; }

        [Column(Order=4)]
        [Display(Name="Sub-Sector Description")]
        [MaxLength(100)]
        public string SubSectorDescription { get; set; }

        [Column(Order=5)]
        public string Key { get; set; }

    }
}
