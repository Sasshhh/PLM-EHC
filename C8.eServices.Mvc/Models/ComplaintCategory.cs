using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintCategory : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Column(Order = 11)]
        [StringLength(50)]
        [Display(Name = "Key")]
        public string Key { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Display Order")]
        public int? DisplayOrder { get; set; }

        public virtual ICollection<ComplaintType> ComplaintTypes { get; set; }
        public virtual ICollection<TenantComplaint> Complaints { get; set; }
    }
}
