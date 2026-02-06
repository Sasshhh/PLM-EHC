using System;
using System.Collections.Generic;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class TrainingProgrammeViewModel
    {
        public TrainingProgrammeViewModel()
        {
            Slides = new List<TrainingSlide>();
        }

        public int TrainingId { get; set; }
        public string Token { get; set; }
        public string ApplicantName { get; set; }
        public PropertyLeaseApplication Application { get; set; }
        public TenantTraining TenantTraining { get; set; }
        public List<TrainingSlide> Slides { get; set; }
        public TrainingSlide CurrentSlide { get; set; }
        public int TotalSlides { get; set; }
        public int CurrentSlideIndex { get; set; }
        public bool IsTrainingCompleted { get; set; }
        public bool CanProceedToExam { get; set; }
    }

    public class ExaminationViewModel
    {
        public ExaminationViewModel()
        {
            Questions = new List<ExaminationQuestion>();
            UserAnswers = new Dictionary<int, string>();
        }

        public int TrainingId { get; set; }
        public string ApplicantName { get; set; }
        public PropertyLeaseApplication Application { get; set; }
        public TenantTraining TenantTraining { get; set; }
        public ExaminationQuestion ExampleQuestion { get; set; }
        public List<ExaminationQuestion> Questions { get; set; }
        public Dictionary<int, string> UserAnswers { get; set; }
        public int AttemptNumber { get; set; }
        public bool CanRetakeExam { get; set; }
        public decimal? LastExamScore { get; set; }
        public DateTime? LastExamDate { get; set; }
    }

    public class ExamResultsViewModel
    {
        public ExamResultsViewModel()
        {
            ExamAnswers = new List<TenantExamAnswer>();
        }

        public int TrainingId { get; set; }
        public string ApplicantName { get; set; }
        public PropertyLeaseApplication Application { get; set; }
        public TenantTraining TenantTraining { get; set; }
        public decimal Score { get; set; }
        public bool Passed { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public int AttemptNumber { get; set; }
        public int MaxAttempts { get; set; }
        public bool CanRetake { get; set; }
        public List<TenantExamAnswer> ExamAnswers { get; set; }
    }

    public class ExamResultViewModel
    {
        public PropertyLeaseApplication Application { get; set; }
        public TenantTraining TenantTraining { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public decimal ScorePercentage { get; set; }
        public bool Passed { get; set; }
        public int AttemptsRemaining { get; set; }
        public List<ExamQuestionResult> QuestionResults { get; set; }
    }

    public class ExamQuestionResult
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
