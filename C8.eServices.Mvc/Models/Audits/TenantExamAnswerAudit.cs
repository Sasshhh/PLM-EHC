using System;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.Models.Audits
{
    public class TenantExamAnswerAudit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        public int PropertyLeaseApplicationId { get; set; }
        public int ExaminationQuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AnsweredDateTime { get; set; }
        public int AttemptNumber { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public int? CreatedBySystemUserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? ModifiedBySystemUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
