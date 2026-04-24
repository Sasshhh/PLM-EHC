using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("ServiceRequestPriorities")]
    public class ServiceRequestPriority : BaseModel
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

        public int Level { get; set; } // 1=Emergency/Critical, 2=High, 3=Medium, 4=Low

        /// <summary>
        /// Response time in minutes (BR34)
        /// Emergency: 30, High: 60, Medium: 1440 (24hr), Low: 2880 (48hr)
        /// </summary>
        public int ResponseTimeMinutes { get; set; }

        /// <summary>
        /// Resolution time in hours (BR34)
        /// Emergency: 24, High: 72, Medium: 120 (5 business days), Low: 720 (30 business days)
        /// </summary>
        public int ResolutionTimeHours { get; set; }

        public int DisplayOrder { get; set; }
    }
}
