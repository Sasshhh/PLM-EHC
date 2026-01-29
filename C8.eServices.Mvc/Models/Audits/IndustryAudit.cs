using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class IndustryAudit: BaseModelAudit
    {
        [Display(Name = "Sub-Sector")]
        public int SubSectorId { get; set; }
        [ForeignKey("SubSectorId")]
        public SubSector SubSector { get; set; }

        [Display(Name = "Industry ")]
        public string IndustryName { get; set; }

        [Display(Name = "Industry Description")]
        public string IndustryDescription { get; set; }

        public string Key { get; set; }
    }
}
