using System;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.Models.Audits
{
    public class TrainingSlideAudit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        public int SlideNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public int? CreatedBySystemUserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? ModifiedBySystemUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
