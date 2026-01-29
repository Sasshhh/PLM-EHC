using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models.Audits
{
   public class FocusAreaAudit: BaseModelAudit
    {
        [Column(Order = 4)]
        public string FocusAreaName { get; set; }

        [Column(Order = 5)]
        public string FocusAreaDescription { get; set; }

        [Column(Order = 6)]
        public string Key { get; set; }
    }
}
