using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class CompanyDirectors : BaseModel
    {
        [Display(Name = "Surname")]
        [Column(Order = 10)]
        public string LastName { get; set; }

        [Display(Name = "First Name")]
        [Column(Order = 11)]
        public string FirstNames { get; set; }

        [Column(Order = 12)]
        [Display(Name = "PropertyLeaseApplication")]
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Display(Name = "ID Number")]
        [Column(Order = 15)]
        [StringLength(15)]
        public string IDNo { get; set; }

        [Display(Name = "Supporting Doccuments")]
        [Column(Order = 16)]
        [StringLength(25)]
        public File SupportingDoccuments { get; set; }
    }
}