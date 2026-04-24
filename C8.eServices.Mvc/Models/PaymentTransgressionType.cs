using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("PaymentTransgressionTypes")]
    public class PaymentTransgressionType : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int PaymentTransgressionCategoryId { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("PaymentTransgressionCategoryId")]
        public virtual PaymentTransgressionCategory Category { get; set; }
    }
}
