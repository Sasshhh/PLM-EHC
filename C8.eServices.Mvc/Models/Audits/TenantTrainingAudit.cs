using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class TenantTrainingAudit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        public int PropertyLeaseApplicationId { get; set; }
        public DateTime? TrainingInvitedDate { get; set; }
        public DateTime? TrainingCompletedDate { get; set; }
        public int ExamAttempts { get; set; }
        public DateTime? ExamPassedDate { get; set; }
        public decimal? ExamScore { get; set; }
        public int CurrentSlideIndex { get; set; }
        public bool IsTrainingCompleted { get; set; }
        public bool IsExamPassed { get; set; }
        public string TrainingLinkToken { get; set; }
        public DateTime? TokenExpiryDate { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public int? CreatedBySystemUserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? ModifiedBySystemUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
