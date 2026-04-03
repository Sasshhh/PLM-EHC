using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintType : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Category")]
        public int ComplaintCategoryId { get; set; }
        [ForeignKey("ComplaintCategoryId")]
        public ComplaintCategory ComplaintCategory { get; set; }

        [Column(Order = 11)]
        [Required]
        [StringLength(100)]
        [Display(Name = "Type Name")]
        public string Name { get; set; }

        [Column(Order = 12)]
        [StringLength(50)]
        [Display(Name = "Key")]
        public string Key { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Display Order")]
        public int? DisplayOrder { get; set; }

        public virtual ICollection<TenantComplaint> Complaints { get; set; }
    }
}
