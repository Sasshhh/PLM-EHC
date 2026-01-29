using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
   public class Industry:BaseModel
    {
        [Column(Order=2)]
        [Display(Name="Sub-Sector")]
        public int SubSectorId { get; set; }
        [ForeignKey("SubSectorId")]
        public SubSector SubSector { get; set; }

        [Column(Order=3)]
        [Display(Name="Industry ")]
        public string IndustryName { get; set; }

        [Column(Order=4)]
        [Display(Name = "Industry Description")]
        public string IndustryDescription { get; set; } 
        [Column(Order=5)]
        public string Key { get; set; }

        public string IndustrySector => string.Format("{0} : {1} - {2}", SubSector.FocusArea.FocusAreaName, SubSector.SubSectorName,
            IndustryName);
    }
}
