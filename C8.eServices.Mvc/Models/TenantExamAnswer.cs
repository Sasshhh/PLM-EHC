using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// Stores tenant's examination answers
    /// </summary>
    public class TenantExamAnswer : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Property Lease Application")]
        public int PropertyLeaseApplicationId { get; set; }
        
        [ForeignKey("PropertyLeaseApplicationId")]
        public virtual PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Examination Question")]
        public int ExaminationQuestionId { get; set; }
        
        [ForeignKey("ExaminationQuestionId")]
        public virtual ExaminationQuestion ExaminationQuestion { get; set; }

        [Column(Order = 12)]
        [Required]
        [Display(Name = "Selected Answer")]
        [MaxLength(1)]
        public string SelectedAnswer { get; set; } // A, B, C, or D

        [Column(Order = 13)]
        [Display(Name = "Is Correct")]
        public bool IsCorrect { get; set; } = false;

        [Column(Order = 14)]
        [Display(Name = "Answered Date Time")]
        public DateTime AnsweredDateTime { get; set; } = DateTime.Now;

        [Column(Order = 15)]
        [Display(Name = "Attempt Number")]
        public int AttemptNumber { get; set; } = 1;
    }
}
