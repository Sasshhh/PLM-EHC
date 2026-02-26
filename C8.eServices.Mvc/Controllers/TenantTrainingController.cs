using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class TenantTrainingController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }

        private void Initialise()
        {
            try
            {
                IdentityManager = new IdentityManager(db);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    SystemUser = IdentityManager.CurrentUser(User);
                }

                if (SystemUser != null)
                {
                    Customer = db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                            .Include(o => o.CustomerType)
                            .Include(o => o.Status)
                            .FirstOrDefault();

                    if (Customer != null)
                    {
                        CustomerId = Customer.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }

        #region Admin Dashboard
        // GET: TenantTraining/Index
        [Authorize(Roles = "Community Development Officer,Super Administrators,Back Office System Administrator")]
        public ActionResult Index()
        {
            try
            {
                Initialise();

                // Get applications ready for training invitation
                var applications = db.PropertyLeaseApplications
                    .Where(x => x.IsDeleted == false &&
                                x.StatusId == db.Status.FirstOrDefault(s => s.Key == StatusKeys.AssessmentFeePaymentApproved).Id)
                    .Include(r => r.Customer)
                    .Include(r => r.Status)
                    .Include(r => r.HumanEHCOptions)
                    .ToList();

                foreach (var item in applications)
                {
                    item.Data = SecureActionLinkExtension.Encrypt(string.Format("applicationId={0}", item.Id));
                }

                return View(applications);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }
        #endregion

        #region Invite to Training
        // POST: TenantTraining/InviteToTraining
        [HttpPost]
        [Authorize(Roles = "Community Development Officer,Super Administrators,Back Office System Administrator")]
        public ActionResult InviteToTraining(int applicationId)
        {
            try
            {
                Initialise();

                var application = db.PropertyLeaseApplications
                    .Include(a => a.Customer)
                    .FirstOrDefault(a => a.Id == applicationId);

                if (application == null)
                    return Json(new { success = false, message = "Application not found" });

                // Generate unique token
                var token = Guid.NewGuid().ToString();
                var expiryDate = DateTime.Now.AddDays(7);

                // Create training record
                var training = new TenantTraining
                {
                    PropertyLeaseApplicationId = applicationId,
                    InvitationToken = token,
                    TokenExpiryDate = expiryDate,
                    InvitationSentDate = DateTime.Now,
                    CurrentSlideNumber = 0,
                    IsTrainingCompleted = false,
                    IsExamPassed = false,
                    ExamAttempts = 0,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                };

                db.TenantTrainings.Add(training);

                // Update application status
                var awaitingTrainingStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.AwaitingOnlineTraining);
                if (awaitingTrainingStatus != null)
                {
                    application.StatusId = awaitingTrainingStatus.Id;
                }

                db.SaveChanges();

                // Send email invitation
                SendTrainingInvitationEmail(application, token);

                // Activity log
                var activityMessage = db.ActivityTrackerMessages
                    .FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantInvitedToTraining)?.Description ??
                    "Tenant invited to online training";
                MatchingHelper.ActivityTrackerAudit(db, applicationId, activityMessage, CustomerId);

                return Json(new { success = true, message = "Training invitation sent successfully!" });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
        #endregion

        #region Start Training (Tenant clicks email link)
        // GET: TenantTraining/StartTraining?token=xxx
        [AllowAnonymous]
        public ActionResult StartTraining(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return View("_TokenInvalid");

                var training = db.TenantTrainings
                    .Include(t => t.PropertyLeaseApplication)
                    .Include(t => t.PropertyLeaseApplication.Customer)
                    .FirstOrDefault(t => t.InvitationToken == token && !t.IsDeleted);

                if (training == null)
                    return View("_TokenInvalid");

                // Check if token expired
                if (training.TokenExpiryDate < DateTime.Now)
                    return View("_TokenExpired");

                // Load first slide
                var firstSlide = db.TrainingSlides
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .FirstOrDefault();

                if (firstSlide == null)
                    return View("_Error");

                // Update training status
                if (training.TrainingStartedDate == null)
                {
                    training.TrainingStartedDate = DateTime.Now;
                    training.PropertyLeaseApplication.StatusId = db.Status
                        .FirstOrDefault(s => s.Key == StatusKeys.TrainingInProgress)?.Id ?? training.PropertyLeaseApplication.StatusId;
                    db.SaveChanges();
                }

                var viewModel = new TrainingProgrammeViewModel
                {
                    TrainingId = training.Id,
                    Token = token,
                    CurrentSlide = firstSlide,
                    CurrentSlideIndex = 0,
                    TotalSlides = db.TrainingSlides.Count(s => s.IsActive && !s.IsDeleted),
                    ApplicantName = $"{training.PropertyLeaseApplication.FirstName} {training.PropertyLeaseApplication.LastName}"
                };

                return View("TrainingProgramme", viewModel);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }
        #endregion

        #region Navigate Slides
        // POST: TenantTraining/NavigateSlide
        [AllowAnonymous]
        [HttpPost]
        public ActionResult NavigateSlide(int trainingId, int slideIndex, string direction)
        {
            try
            {
                var training = db.TenantTrainings.Find(trainingId);
                if (training == null)
                    return Json(new { success = false, message = "Training session not found" });

                var allSlides = db.TrainingSlides
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .ToList();

                int newIndex = slideIndex;
                if (direction == "next" && slideIndex < allSlides.Count - 1)
                    newIndex = slideIndex + 1;
                else if (direction == "prev" && slideIndex > 0)
                    newIndex = slideIndex - 1;

                // Update current slide number
                training.CurrentSlideNumber = newIndex;
                training.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();

                var currentSlide = allSlides[newIndex];

                return Json(new
                {
                    success = true,
                    slideNumber = currentSlide.SlideNumber,
                    title = currentSlide.Title,
                    content = currentSlide.Content,
                    imagePath = Url.Content(currentSlide.ImagePath),
                    currentIndex = newIndex,
                    totalSlides = allSlides.Count,
                    isLastSlide = newIndex == allSlides.Count - 1
                });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
        #endregion

        #region Complete Training
        // POST: TenantTraining/CompleteTraining
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CompleteTraining(int trainingId)
        {
            try
            {
                var training = db.TenantTrainings
                    .Include(t => t.PropertyLeaseApplication)
                    .FirstOrDefault(t => t.Id == trainingId);

                if (training == null)
                    return Json(new { success = false, message = "Training session not found" });

                training.IsTrainingCompleted = true;
                training.TrainingCompletedDate = DateTime.Now;
                training.ModifiedDateTime = DateTime.Now;

                // Update application status
                training.PropertyLeaseApplication.StatusId = db.Status
                    .FirstOrDefault(s => s.Key == StatusKeys.TrainingCompleted)?.Id ?? training.PropertyLeaseApplication.StatusId;

                db.SaveChanges();

                return Json(new { success = true, message = "Training completed successfully!" });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return Json(new { success = false, message = "An error occurred" });
            }
        }
        #endregion

        #region Start Examination
        // GET: TenantTraining/Examination?trainingId=xxx
        [AllowAnonymous]
        public ActionResult Examination(int trainingId)
        {
            try
            {
                var training = db.TenantTrainings
                    .Include(t => t.PropertyLeaseApplication)
                    .FirstOrDefault(t => t.Id == trainingId);

                if (training == null || !training.IsTrainingCompleted)
                    return View("_Error");

                // No max attempts limit - can retake unlimited times until pass

                // Get all questions (example + actual questions)
                var exampleQuestion = db.ExaminationQuestions
                    .FirstOrDefault(q => q.IsExampleQuestion && q.IsActive && !q.IsDeleted);

                var actualQuestions = db.ExaminationQuestions
                    .Where(q => !q.IsExampleQuestion && q.IsActive && !q.IsDeleted)
                    .OrderBy(q => q.QuestionOrder)
                    .ToList();

                var viewModel = new ExaminationViewModel
                {
                    TrainingId = trainingId,
                    ExampleQuestion = exampleQuestion,
                    Questions = actualQuestions,
                    AttemptNumber = training.ExamAttempts + 1,
                    ApplicantName = $"{training.PropertyLeaseApplication.FirstName} {training.PropertyLeaseApplication.LastName}"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }
        #endregion

        #region Submit Examination
        // POST: TenantTraining/SubmitExamination
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SubmitExamination(int trainingId, FormCollection form)
        {
            try
            {
                var training = db.TenantTrainings
                    .Include(t => t.PropertyLeaseApplication)
                    .FirstOrDefault(t => t.Id == trainingId);

                if (training == null)
                    return Json(new { success = false, message = "Training session not found" });

                // Get all actual questions (exclude example)
                var questions = db.ExaminationQuestions
                    .Where(q => !q.IsExampleQuestion && q.IsActive && !q.IsDeleted)
                    .OrderBy(q => q.QuestionOrder)
                    .ToList();

                int correctCount = 0;

                // Grade each answer
                foreach (var question in questions)
                {
                    var userAnswer = form[$"question_{question.Id}"];
                    var isCorrect = userAnswer == question.CorrectAnswer;

                    if (isCorrect) correctCount++;

                    // Save answer
                    var examAnswer = new TenantExamAnswer
                    {
                        PropertyLeaseApplicationId = training.PropertyLeaseApplicationId,
                        ExaminationQuestionId = question.Id,
                        SelectedAnswer = userAnswer,
                        IsCorrect = isCorrect,
                        AttemptNumber = training.ExamAttempts + 1,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    };

                    db.TenantExamAnswers.Add(examAnswer);
                }

                // Calculate score
                var score = (decimal)correctCount / questions.Count * 100;
                var passed = correctCount == questions.Count; // Must be 100%

                training.ExamAttempts++;
                training.ExamScore = score;

                if (passed)
                {
                    training.IsExamPassed = true;
                    training.ExamPassedDate = DateTime.Now;

                    // Update application status to awaiting inspection schedule slots (next step in workflow)
                    training.PropertyLeaseApplication.StatusId = db.Status
                        .FirstOrDefault(s => s.Key == StatusKeys.AwaitingInspectionScheduleSlots)?.Id ?? training.PropertyLeaseApplication.StatusId;

                    // Schedule inspection slots via Round Robin (moved from ClientTraining)
                    using (var cxt = new eServicesDbContext())
                    {
                        var controller = new PropertyLeaseApplicationController(cxt);
                        controller.EHCRoundRobin(
                            training.PropertyLeaseApplicationId,
                            false, false, false, false, false, false, false, false,
                            true,
                            false, false, false, false, false, false, false, false, false, false, false, false, false, false,
                            1,
                            false, false,
                            1);
                    }

                    // Send pass email
                    SendExamPassEmail(training.PropertyLeaseApplication, score, correctCount, questions.Count);
                }
                else
                {
                    // Update application status to failed
                    training.PropertyLeaseApplication.StatusId = db.Status
                        .FirstOrDefault(s => s.Key == StatusKeys.ExaminationFailed)?.Id ?? training.PropertyLeaseApplication.StatusId;

                    // Send fail email
                    SendExamFailEmail(training.PropertyLeaseApplication, score, correctCount, questions.Count, training.ExamAttempts);
                }

                training.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    passed = passed,
                    score = score,
                    correctCount = correctCount,
                    totalQuestions = questions.Count,
                    attemptNumber = training.ExamAttempts
                });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return Json(new { success = false, message = "An error occurred while submitting your exam" });
            }
        }
        #endregion

        #region View Exam Results
        // GET: TenantTraining/ExamResults?trainingId=xxx
        [AllowAnonymous]
        public ActionResult ExamResults(int trainingId)
        {
            try
            {
                var training = db.TenantTrainings
                    .Include(t => t.PropertyLeaseApplication)
                    .FirstOrDefault(t => t.Id == trainingId);

                if (training == null)
                    return View("_Error");

                // Get latest exam answers
                var latestAttempt = training.ExamAttempts;
                var examAnswers = db.TenantExamAnswers
                    .Where(a => a.PropertyLeaseApplicationId == training.PropertyLeaseApplicationId &&
                                a.AttemptNumber == latestAttempt)
                    .Include(a => a.ExaminationQuestion)
                    .OrderBy(a => a.ExaminationQuestion.QuestionOrder)
                    .ToList();

                var viewModel = new ExamResultsViewModel
                {
                    TrainingId = trainingId,
                    Score = training.ExamScore ?? 0,
                    Passed = training.IsExamPassed,
                    CorrectCount = examAnswers.Count(a => a.IsCorrect),
                    TotalQuestions = examAnswers.Count,
                    AttemptNumber = latestAttempt,
                    MaxAttempts = 999, // Unlimited attempts
                    CanRetake = !training.IsExamPassed, // Can always retake if not passed
                    ExamAnswers = examAnswers,
                    ApplicantName = $"{training.PropertyLeaseApplication.FirstName} {training.PropertyLeaseApplication.LastName}"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }
        #endregion

        #region Customer - My Training Dashboard
        // GET: TenantTraining/MyTraining
        [Authorize(Roles = "Customers")]
        public ActionResult MyTraining()
        
{
            try
            {
                Initialise();

                if (Customer == null)
                    return RedirectToAction("Login", "Account");

                // Get customer's applications that need training or are in training
                var applications = db.PropertyLeaseApplications
                    .Where(x => x.CustomerId == Customer.Id &&
                               !x.IsDeleted &&
                               (x.Status.Key == StatusKeys.AwaitingOnlineTraining ||
                                x.Status.Key == StatusKeys.TrainingInProgress ||
                                x.Status.Key == StatusKeys.TrainingCompleted ||
                                x.Status.Key == StatusKeys.ExaminationFailed ||
                                x.Status.Key == StatusKeys.ExaminationPassed))
                    .Include(x => x.Status)
                    .OrderByDescending(x => x.CreatedDateTime)
                    .ToList();

                // Get training records
                var trainings = new List<TenantTraining>();
                foreach (var app in applications)
                {
                    var training = db.TenantTrainings
                        .FirstOrDefault(t => t.PropertyLeaseApplicationId == app.Id &&
                                           !t.IsDeleted &&
                                           t.IsActive);
                    if (training != null)
                    {
                        trainings.Add(training);
                    }
                }

                ViewBag.Applications = applications;
                ViewBag.Trainings = trainings;

                return View();
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return View("_Error");
            }
        }
        #endregion

        #region Helper Methods

        private void SendTrainingInvitationEmail(PropertyLeaseApplication application, string token)
        {
            try
            {
                var email = new Email();
                var trainingLink = Url.Action("StartTraining", "TenantTraining", new { token = token }, Request.Url.Scheme);

                var subject = "Your Pre-Tenancy Training Invitation - EHC";
                var body = $@"
                    <html>
                    <body>
                        <h2>Pre-Tenancy Training Invitation</h2>
                        <p>Dear {application.FirstName} {application.LastName},</p>
                        <p>Congratulations! You have been invited to complete your mandatory pre-tenancy training.</p>
                        <p>Please click the link below to start your training:</p>
                        <p><a href='{trainingLink}' style='background-color:#4CAF50;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Start Training</a></p>
                        <p><strong>Note:</strong> This link will expire in 7 days.</p>
                        <br/>
                        <p>Best regards,<br/>Ekurhuleni Housing Company</p>
                    </body>
                    </html>
                ";

                email.GenerateEmail(application.PurEmail, subject, body, application.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, $"{application.FirstName} {application.LastName}");
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }

        private void SendExamPassEmail(PropertyLeaseApplication application, decimal score, int correctCount, int totalQuestions)
        {
            try
            {
                var email = new Email();
                var subject = "Congratulations! You Passed the Pre-Tenancy Examination";
                var body = $@"
                    <html>
                    <body>
                        <h2>Examination Results - PASSED</h2>
                        <p>Dear {application.FirstName} {application.LastName},</p>
                        <p><strong>Congratulations!</strong> You have successfully passed the pre-tenancy examination.</p>
                        <p><strong>Your Score:</strong> {correctCount}/{totalQuestions} ({score:F0}%)</p>
                        <p>You will be contacted shortly regarding the next steps in your application process.</p>
                        <br/>
                        <p>Best regards,<br/>Ekurhuleni Housing Company</p>
                    </body>
                    </html>
                ";

                email.GenerateEmail(application.PurEmail, subject, body, application.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, $"{application.FirstName} {application.LastName}");
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }

        private void SendExamFailEmail(PropertyLeaseApplication application, decimal score, int correctCount, int totalQuestions, int attemptNumber)
        {
            try
            {
                var email = new Email();
                var attemptsRemaining = 3 - attemptNumber;
                var subject = "Pre-Tenancy Examination Results";
                var body = $@"
                    <html>
                    <body>
                        <h2>Examination Results</h2>
                        <p>Dear {application.FirstName} {application.LastName},</p>
                        <p>Thank you for completing the pre-tenancy examination.</p>
                        <p><strong>Your Score:</strong> {correctCount}/{totalQuestions} ({score:F0}%)</p>
                        <p><strong>Required to Pass:</strong> 15/15 (100%)</p>
                        <p><strong>Attempt:</strong> {attemptNumber} of 3</p>
                        {(attemptsRemaining > 0 ? $"<p>You have {attemptsRemaining} attempt(s) remaining. Please review the training material and try again.</p>" : "<p><strong>Maximum attempts reached.</strong> Please contact EHC for further assistance.</p>")}
                        <br/>
                        <p>Best regards,<br/>Ekurhuleni Housing Company</p>
                    </body>
                    </html>
                ";

                email.GenerateEmail(application.PurEmail, subject, body, application.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, $"{application.FirstName} {application.LastName}");
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
