using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C8.eServices.Mvc.Models
{
 public class FocusArea: BaseModel
    {
        [Column(Order=2)]
        [Display(Name="Focus Area Name")]
        public string FocusAreaName { get; set; }

        [Column(Order=3)]
        [Display(Name = "Focus Area Description")]
        public string FocusAreaDescription { get; set; }

        [Column(Order=4)]
        public string Key { get; set; }
    }
}
