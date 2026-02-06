using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// Stores the 15 examination questions with multiple choice options
    /// </summary>
    public class ExaminationQuestion : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Option A")]
        [MaxLength(500)]
        public string OptionA { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Option B")]
        [MaxLength(500)]
        public string OptionB { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Option C")]
        [MaxLength(500)]
        public string OptionC { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Option D")]
        [MaxLength(500)]
        public string OptionD { get; set; }

        [Column(Order = 15)]
        [Required]
        [Display(Name = "Correct Answer")]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } // A, B, C, or D

        [Column(Order = 16)]
        [Required]
        [Display(Name = "Question Order")]
        public int QuestionOrder { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Is Example Question")]
        public bool IsExampleQuestion { get; set; } = false;
    }
}
