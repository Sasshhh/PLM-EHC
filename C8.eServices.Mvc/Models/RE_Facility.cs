using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("RE_Facilities")]
    public class RE_Facility : BaseModel
    {
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [Required]
        public int CCCId { get; set; }
        [ForeignKey("CCCId")]
        public virtual CCC CCC { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }
    }
}
