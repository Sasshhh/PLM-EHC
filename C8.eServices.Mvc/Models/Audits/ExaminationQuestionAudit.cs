using System;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.Models.Audits
{
    public class ExaminationQuestionAudit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public int QuestionOrder { get; set; }
        public bool IsExampleQuestion { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public int? CreatedBySystemUserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public int? ModifiedBySystemUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
