using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class SubSectorAudit: BaseModelAudit
    {
        [Display(Name = "Focus Area")]
        public int FocusAreaId { get; set; }
        [ForeignKey("FocusAreaId")]
        public FocusArea FocusArea { get; set; }

        [Display(Name = "Sub-Sector")]
        [MaxLength(100)]
        public string SubSectorName { get; set; }

        [Display(Name = "Sub-Sector Description")]
        [MaxLength(100)]
        public string SubSectorDescription { get; set; }

        public string Key { get; set; }
    }
}
