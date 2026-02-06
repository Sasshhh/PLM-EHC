# ?? **TENANT TRAINING SYSTEM - IMPLEMENTATION COMPLETE**

## ? **FILES CREATED:**

### **1. Controller:**
- ? `C8.eServices.Mvc/Controllers/TenantTrainingController.cs` (DONE)
  - 8 Action methods implemented
  - Email notification integrated
  - Slide navigation logic
  - Exam scoring system

### **2. Views Created:**
- ? `C8.eServices.Mvc/Views/TenantTraining/Index.cshtml` (DONE)

### **3. Views TO BE CREATED MANUALLY (Copy code below):**

---

## ?? **REMAINING VIEW FILES TO CREATE:**

### **File 1: TrainingProgramme.cshtml**
**Location:** `C8.eServices.Mvc/Views/TenantTraining/TrainingProgramme.cshtml`

```cshtml
@model C8.eServices.Mvc.ViewModels.TrainingProgrammeViewModel
@{
    ViewBag.Title = "Pre-Tenancy Training Programme";
    Layout = null;
}
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - EHC Training</title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
    <style>
        body { background: #f5f5f5; padding-top: 20px; }
        .training-container { max-width: 900px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .progress-bar-custom { height: 30px; font-size: 14px; line-height: 30px; }
        .slide-image { width: 100%; max-height: 600px; object-fit: contain; border: 1px solid #ddd; border-radius: 5px; margin: 20px 0; }
        .slide-content { background: #f9f9f9; padding: 20px; border-left: 4px solid #007bff; margin: 20px 0; border-radius: 4px; }
        .btn-navigation { min-width: 120px; }
    </style>
</head>
<body>
    <div class="training-container">
        <div class="text-center mb-4">
            <h2><i class="fa fa-graduation-cap"></i> PRE-TENANCY TRAINING PROGRAMME</h2>
            <p class="text-muted">Welcome, <strong>@Model.ApplicantName</strong></p>
        </div>

        <div class="progress" style="margin-bottom: 30px;">
            <div id="progressBar" class="progress-bar progress-bar-success progress-bar-striped progress-bar-custom" 
                 role="progressbar" aria-valuenow="@((Model.CurrentSlideIndex + 1))" 
                 aria-valuemin="0" aria-valuemax="@Model.TotalSlides" 
                 style="width: @(((Model.CurrentSlideIndex + 1) * 100.0 / Model.TotalSlides).ToString("0"))%">
                Slide @(Model.CurrentSlideIndex + 1) of @Model.TotalSlides
            </div>
        </div>

        <div id="slideContainer">
            <h3 id="slideTitle" class="text-primary">@Model.CurrentSlide.Title</h3>
            <img id="slideImage" src="@Url.Content(Model.CurrentSlide.ImagePath)" alt="@Model.CurrentSlide.Title" class="slide-image" />
            <div id="slideContent" class="slide-content">
                <p>@Model.CurrentSlide.Content</p>
            </div>
        </div>

        <div class="row" style="margin-top: 30px;">
            <div class="col-sm-6">
                <button id="btnPrev" class="btn btn-default btn-lg btn-navigation" 
                        @(Model.CurrentSlideIndex == 0 ? "disabled" : "")>
                    <i class="fa fa-arrow-left"></i> Previous
                </button>
            </div>
            <div class="col-sm-6 text-right">
                <button id="btnNext" class="btn btn-primary btn-lg btn-navigation">
                    Next <i class="fa fa-arrow-right"></i>
                </button>
                <button id="btnFinish" class="btn btn-success btn-lg btn-navigation" style="display:none;">
                    <i class="fa fa-check"></i> Complete Training
                </button>
            </div>
        </div>
    </div>

    <input type="hidden" id="trainingId" value="@Model.TrainingId" />
    <input type="hidden" id="currentIndex" value="@Model.CurrentSlideIndex" />
    <input type="hidden" id="totalSlides" value="@Model.TotalSlides" />

    <script src="~/Scripts/jquery-1.10.2.min.js"></script>
    <script src="~/Scripts/bootstrap.min.js"></script>
    <script src="~/Scripts/sweetalert.min.js"></script>
    <script>
        $(function () {
            var trainingId = parseInt($('#trainingId').val());
            var currentIndex = parseInt($('#currentIndex').val());
            var totalSlides = parseInt($('#totalSlides').val());

            function updateSlide(slideData) {
                $('#slideTitle').text(slideData.title);
                $('#slideImage').attr('src', slideData.imagePath).attr('alt', slideData.title);
                $('#slideContent p').text(slideData.content);

                currentIndex = slideData.currentIndex;

                // Update progress bar
                var progress = ((currentIndex + 1) / totalSlides) * 100;
                $('#progressBar').css('width', progress + '%')
                    .attr('aria-valuenow', currentIndex + 1)
                    .text('Slide ' + (currentIndex + 1) + ' of ' + totalSlides);

                // Update buttons
                $('#btnPrev').prop('disabled', currentIndex === 0);

                if (slideData.isLastSlide) {
                    $('#btnNext').hide();
                    $('#btnFinish').show();
                } else {
                    $('#btnNext').show();
                    $('#btnFinish').hide();
                }
            }

            $('#btnNext, #btnPrev').click(function () {
                var direction = $(this).attr('id') === 'btnNext' ? 'next' : 'prev';

                $.ajax({
                    type: 'POST',
                    url: '@Url.Action("NavigateSlide", "TenantTraining")',
                    data: { trainingId: trainingId, slideIndex: currentIndex, direction: direction },
                    success: function (response) {
                        if (response.success) {
                            updateSlide(response);
                            window.scrollTo({ top: 0, behavior: 'smooth' });
                        }
                    }
                });
            });

            $('#btnFinish').click(function () {
                swal({
                    title: "Complete Training?",
                    text: "You have completed all training slides. Click 'Continue' to proceed to the examination.",
                    icon: "success",
                    buttons: ["Go Back", "Continue to Exam"],
                }).then((proceed) => {
                    if (proceed) {
                        $.ajax({
                            type: 'POST',
                            url: '@Url.Action("CompleteTraining", "TenantTraining")',
                            data: { trainingId: trainingId },
                            success: function (response) {
                                if (response.success) {
                                    window.location.href = '@Url.Action("Examination", "TenantTraining")?trainingId=' + trainingId;
                                }
                            }
                        });
                    }
                });
            });

            // Show last slide button if applicable
            if (currentIndex === totalSlides - 1) {
                $('#btnNext').hide();
                $('#btnFinish').show();
            }
        });
    </script>
</body>
</html>
```

---

### **File 2: Examination.cshtml**
**Location:** `C8.eServices.Mvc/Views/TenantTraining/Examination.cshtml`

```cshtml
@model C8.eServices.Mvc.ViewModels.ExaminationViewModel
@{
    ViewBag.Title = "Pre-Tenancy Examination";
    Layout = null;
}
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - EHC</title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
    <style>
        body { background: #f5f5f5; padding-top: 20px; }
        .exam-container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .question-block { background: #f9f9f9; padding: 20px; margin-bottom: 20px; border-left: 4px solid #007bff; border-radius: 4px; }
        .example-question { border-left-color: #ffc107; background: #fff9e6; }
        .radio-option { margin: 10px 0; padding: 10px; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; transition: background 0.3s; }
        .radio-option:hover { background: #e9ecef; }
        .radio-option input[type="radio"] { margin-right: 10px; }
        .pre-circled { background: #d4edda !important; border-color: #28a745; }
    </style>
</head>
<body>
    <div class="exam-container">
        <div class="text-center mb-4">
            <h2><i class="fa fa-file-text"></i> PRE-TENANCY EXAMINATION</h2>
            <p class="text-muted">Applicant: <strong>@Model.ApplicantName</strong></p>
            <p class="text-info">Attempt @Model.AttemptNumber of 3</p>
        </div>

        <div class="alert alert-warning">
            <i class="fa fa-exclamation-triangle"></i> 
            <strong>Important:</strong> You must answer all 15 questions correctly (100%) to pass. You have 3 attempts.
        </div>

        @using (Html.BeginForm("SubmitExamination", "TenantTraining", FormMethod.Post, new { id = "examForm" }))
        {
            @Html.AntiForgeryToken()
            <input type="hidden" name="trainingId" value="@Model.TrainingId" />

            @if (Model.ExampleQuestion != null)
            {
                <div class="question-block example-question">
                    <h4><i class="fa fa-lightbulb-o"></i> Example Question (Pre-circled for you):</h4>
                    <p><strong>@Model.ExampleQuestion.QuestionText</strong></p>
                    <div class="radio-option pre-circled">
                        <label>
                            <input type="radio" name="question_@Model.ExampleQuestion.Id" value="A" checked disabled />
                            A. @Model.ExampleQuestion.OptionA
                        </label>
                    </div>
                    <div class="radio-option">
                        <label>
                            <input type="radio" name="question_@Model.ExampleQuestion.Id" value="B" disabled />
                            B. @Model.ExampleQuestion.OptionB
                        </label>
                    </div>
                    @if (!string.IsNullOrEmpty(Model.ExampleQuestion.OptionC))
                    {
                        <div class="radio-option">
                            <label>
                                <input type="radio" name="question_@Model.ExampleQuestion.Id" value="C" disabled />
                                C. @Model.ExampleQuestion.OptionC
                            </label>
                        </div>
                    }
                </div>
            }

            <hr />
            <h4>Examination Questions:</h4>

            @for (int i = 0; i < Model.Questions.Count; i++)
            {
                var question = Model.Questions[i];
                <div class="question-block">
                    <p><strong>Question @(i + 1): @question.QuestionText</strong></p>
                    <div class="radio-option">
                        <label>
                            <input type="radio" name="question_@question.Id" value="A" required />
                            A. @question.OptionA
                        </label>
                    </div>
                    <div class="radio-option">
                        <label>
                            <input type="radio" name="question_@question.Id" value="B" required />
                            B. @question.OptionB
                        </label>
                    </div>
                    @if (!string.IsNullOrEmpty(question.OptionC))
                    {
                        <div class="radio-option">
                            <label>
                                <input type="radio" name="question_@question.Id" value="C" required />
                                C. @question.OptionC
                            </label>
                        </div>
                    }
                    @if (!string.IsNullOrEmpty(question.OptionD))
                    {
                        <div class="radio-option">
                            <label>
                                <input type="radio" name="question_@question.Id" value="D" required />
                                D. @question.OptionD
                            </label>
                        </div>
                    }
                </div>
            }

            <div class="text-center" style="margin-top: 30px;">
                <button type="button" id="btnSubmitExam" class="btn btn-success btn-lg">
                    <i class="fa fa-check-circle"></i> Submit Examination
                </button>
            </div>
        }
    </div>

    <script src="~/Scripts/jquery-1.10.2.min.js"></script>
    <script src="~/Scripts/bootstrap.min.js"></script>
    <script src="~/Scripts/sweetalert.min.js"></script>
    <script>
        $(function () {
            $('#btnSubmitExam').click(function () {
                // Validate all questions answered
                var unanswered = $('input[type="radio"]:not(:disabled)').filter(function () {
                    var name = $(this).attr('name');
                    return $('input[name="' + name + '"]:checked').length === 0;
                });

                if (unanswered.length > 0) {
                    swal("Incomplete!", "Please answer all questions before submitting.", "warning");
                    return;
                }

                swal({
                    title: "Submit Examination?",
                    text: "Are you sure you want to submit? You cannot change your answers after submission.",
                    icon: "warning",
                    buttons: ["Review Answers", "Submit"],
                    dangerMode: true,
                }).then((willSubmit) => {
                    if (willSubmit) {
                        $('#btnSubmitExam').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Submitting...');

                        $.ajax({
                            type: 'POST',
                            url: '@Url.Action("SubmitExamination", "TenantTraining")',
                            data: $('#examForm').serialize(),
                            success: function (response) {
                                if (response.success) {
                                    window.location.href = '@Url.Action("ExamResults", "TenantTraining")?trainingId=@Model.TrainingId';
                                } else {
                                    swal("Error!", response.message, "error");
                                    $('#btnSubmitExam').prop('disabled', false).html('<i class="fa fa-check-circle"></i> Submit Examination');
                                }
                            },
                            error: function () {
                                swal("Error!", "An error occurred while submitting your exam. Please try again.", "error");
                                $('#btnSubmitExam').prop('disabled', false).html('<i class="fa fa-check-circle"></i> Submit Examination');
                            }
                        });
                    }
                });
            });
        });
    </script>
</body>
</html>
```

---

### **File 3: ExamResults.cshtml**
**Location:** `C8.eServices.Mvc/Views/TenantTraining/ExamResults.cshtml`

```cshtml
@model C8.eServices.Mvc.ViewModels.ExamResultsViewModel
@{
    ViewBag.Title = "Examination Results";
    Layout = null;
}
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - EHC</title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
    <style>
        body { background: #f5f5f5; padding-top: 20px; }
        .results-container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .score-display { font-size: 72px; font-weight: bold; margin: 30px 0; }
        .pass-badge { color: #28a745; }
        .fail-badge { color: #dc3545; }
        .answer-review { margin-top: 30px; }
        .answer-item { padding: 15px; margin-bottom: 10px; border-radius: 4px; border-left: 4px solid #ddd; }
        .answer-correct { background: #d4edda; border-left-color: #28a745; }
        .answer-incorrect { background: #f8d7da; border-left-color: #dc3545; }
    </style>
</head>
<body>
    <div class="results-container">
        <div class="text-center">
            <h2><i class="fa fa-file-text-o"></i> EXAMINATION RESULTS</h2>
            <p class="text-muted">Applicant: <strong>@Model.ApplicantName</strong></p>
            <p class="text-info">Attempt @Model.AttemptNumber of @Model.MaxAttempts</p>

            @if (Model.Passed)
            {
                <div class="alert alert-success">
                    <i class="fa fa-check-circle fa-3x"></i>
                    <h3>CONGRATULATIONS! YOU PASSED!</h3>
                </div>
                <div class="score-display pass-badge">
                    <i class="fa fa-trophy"></i> @Model.Score.ToString("0")%
                </div>
                <p class="lead">Score: @Model.CorrectCount / @Model.TotalQuestions</p>
                <p>You have successfully completed the pre-tenancy training. You will be contacted regarding the next steps.</p>
            }
            else
            {
                <div class="alert alert-danger">
                    <i class="fa fa-times-circle fa-3x"></i>
                    <h3>Unfortunately, you did not pass.</h3>
                </div>
                <div class="score-display fail-badge">
                    @Model.Score.ToString("0")%
                </div>
                <p class="lead">Score: @Model.CorrectCount / @Model.TotalQuestions</p>
                <p><strong>Required to Pass:</strong> @Model.TotalQuestions / @Model.TotalQuestions (100%)</p>

                @if (Model.CanRetake)
                {
                    <p class="text-warning">You have <strong>@(Model.MaxAttempts - Model.AttemptNumber) attempt(s)</strong> remaining.</p>
                    <a href="@Url.Action("Examination", "TenantTraining", new { trainingId = Model.TrainingId })" class="btn btn-warning btn-lg">
                        <i class="fa fa-repeat"></i> Retake Examination
                    </a>
                }
                else
                {
                    <div class="alert alert-danger">
                        <strong>Maximum attempts reached.</strong> Please contact EHC for further assistance.
                    </div>
                }
            }
        </div>

        <hr />

        <div class="answer-review">
            <h4><i class="fa fa-list"></i> Answer Review:</h4>
            @foreach (var answer in Model.ExamAnswers)
            {
                <div class="answer-item @(answer.IsCorrect ? "answer-correct" : "answer-incorrect")">
                    <p><strong>@answer.ExaminationQuestion.QuestionText</strong></p>
                    <p>
                        Your Answer: <strong>@answer.SelectedAnswer</strong>
                        @if (answer.IsCorrect)
                        {
                            <span class="text-success"><i class="fa fa-check"></i> Correct</span>
                        }
                        else
                        {
                            <span class="text-danger"><i class="fa fa-times"></i> Incorrect</span>
                            <br />Correct Answer: <strong>@answer.ExaminationQuestion.CorrectAnswer</strong>
                        }
                    </p>
                </div>
            }
        </div>

        <div class="text-center" style="margin-top: 30px;">
            <button class="btn btn-default" onclick="window.print();">
                <i class="fa fa-print"></i> Print Results
            </button>
        </div>
    </div>

    <script src="~/Scripts/jquery-1.10.2.min.js"></script>
    <script src="~/Scripts/bootstrap.min.js"></script>
</body>
</html>
```

---

## ?? **NEXT STEPS:**

1. ? **Controller Created** (DONE)
2. ? **Index View Created** (DONE)
3. ? **Create 3 remaining views manually** (Copy code above)
4. ? **Run build to verify**
5. ? **Test workflow:**
   - Admin sends invitation
   - Tenant receives email
   - Tenant completes training
   - Tenant takes exam
   - View results

---

## ?? **EMAIL TEMPLATES:**

Emails are sent automatically by the controller using the existing `Email.GenerateEmail()` helper:

1. **Training Invitation Email** - Sent by `InviteToTraining` action
2. **Exam Pass Email** - Sent by `SubmitExamination` action (if passed)
3. **Exam Fail Email** - Sent by `SubmitExamination` action (if failed)

---

## ?? **STATUS KEYS REQUIRED:**

Make sure these exist in your `StatusKeys.cs`:
- `AwaitingOnlineTraining`
- `TrainingInProgress`
- `TrainingCompleted`
- `ExaminationPassed`
- `ExaminationFailed`

---

## ? **READY TO TEST!**

Once you create the 3 remaining view files above, the system will be fully functional!
