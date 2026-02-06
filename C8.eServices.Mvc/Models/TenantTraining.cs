using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// Tracks tenant training progress and examination results
    /// </summary>
    public class TenantTraining : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Property Lease Application")]
        public int PropertyLeaseApplicationId { get; set; }
        
        [ForeignKey("PropertyLeaseApplicationId")]
        public virtual PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Invitation Token")]
        [MaxLength(500)]
        public string InvitationToken { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Token Expiry Date")]
        public DateTime? TokenExpiryDate { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Invitation Sent Date")]
        public DateTime? InvitationSentDate { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Training Started Date")]
        public DateTime? TrainingStartedDate { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Training Completed Date")]
        public DateTime? TrainingCompletedDate { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Current Slide Number")]
        public int CurrentSlideNumber { get; set; } = 0;

        [Column(Order = 17)]
        [Display(Name = "Training Completed")]
        public bool IsTrainingCompleted { get; set; } = false;

        [Column(Order = 18)]
        [Display(Name = "Exam Attempts")]
        public int ExamAttempts { get; set; } = 0;

        [Column(Order = 19)]
        [Display(Name = "Exam Score (%)")]
        public decimal? ExamScore { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Exam Passed")]
        public bool IsExamPassed { get; set; } = false;

        [Column(Order = 21)]
        [Display(Name = "Exam Passed Date")]
        public DateTime? ExamPassedDate { get; set; }
    }
}
