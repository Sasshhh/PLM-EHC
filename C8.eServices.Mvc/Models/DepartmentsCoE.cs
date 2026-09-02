using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DepartmentsCoE: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Represented By")]
        public string RepresentedBy { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Representative User ID")]
        public int? RepresentativeSystemUserId { get; set; }

        [ForeignKey("RepresentativeSystemUserId")]
        public virtual SystemUser RepresentativeSystemUser { get; set; }
    }
}