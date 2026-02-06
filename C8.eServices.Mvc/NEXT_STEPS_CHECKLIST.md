# ?? NEXT STEPS - QUICK ACTION CHECKLIST

## ? **COMPLETED (Database Setup)**
- [x] Entity Framework models created (8 models)
- [x] DbContext updated with DbSets
- [x] Migrations run successfully
- [x] 24 training slides seeded
- [x] 16 exam questions seeded (with correct answers)
- [x] 5 status keys created
- [x] StatusKeys.cs constants added
- [x] Build successful (no errors)
- [x] All verifications passed

---

## ?? **IMMEDIATE ACTIONS (Do This Today)**

### **1. Complete Slide Images (15 minutes)** ?? HIGH PRIORITY

**What to do:**
1. Open your PowerPoint presentation with the 24 training slides
2. Export each slide as a JPG image
3. Name files: `slide-02.jpg` through `slide-24.jpg` (slide-01.jpg already exists)
4. Save to: `C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Content\Training Slides\`

**Quick PowerPoint Export Steps:**
```
1. Open PowerPoint
2. File ? Export ? Change File Type ? JPEG
3. Export All Slides
4. Rename files to match slide numbers (slide-02.jpg, slide-03.jpg, etc.)
5. Copy to C8.eServices.Mvc\Content\Training Slides\
```

**Verify:**
```
? slide-01.jpg (exists)
? slide-02.jpg
? slide-03.jpg
? slide-04.jpg
...
? slide-24.jpg
```

---

### **2. Commit Database Changes (5 minutes)** ?? MEDIUM PRIORITY

**Git Commands:**
```bash
cd "C:\REPO\PLM V1\PLM-EHC"
git add .
git commit -m "Add Tenant Training System - Database Complete

- Created 8 tables (4 main + 4 audit)
- Seeded 24 training slides
- Seeded 16 examination questions with correct answers
- Added 5 status keys for training workflow
- Updated StatusKeys.cs constants
- All verifications passed"

git push origin main
```

---

## ?? **UPCOMING TASKS (This Week)**

### **3. Implement Controller** ? Estimated: 3-4 hours

**File to create:** `C8.eServices.Mvc/Controllers/TenantTrainingController.cs`

**Required Actions (8 methods):**
```csharp
public class TenantTrainingController : Controller
{
    // 1. Admin Dashboard
    public ActionResult Index() { }

    // 2. Send Training Invitation
    [HttpPost]
    public ActionResult InviteToTraining(int applicationId) { }

    // 3. Tenant Starts Training (via email link)
    public ActionResult StartTraining(string token) { }

    // 4. Navigate Training Slides
    public ActionResult ViewSlide(int slideIndex) { }

    // 5. Complete Training
    [HttpPost]
    public ActionResult CompleteTraining() { }

    // 6. Start Examination
    public ActionResult StartExamination() { }

    // 7. Submit Examination Answers
    [HttpPost]
    public ActionResult SubmitExamination(FormCollection form) { }

    // 8. View Exam Results
    public ActionResult ViewExamResults(int attemptNumber) { }
}
```

**Reference:** See existing `PropertyLeaseApplicationController.cs` for patterns

---

### **4. Create Views** ? Estimated: 2-3 hours

**Files to create:**

1. **`Views/TenantTraining/Index.cshtml`**
   - Admin dashboard table
   - "Invite to Training" buttons

2. **`Views/TenantTraining/TrainingProgramme.cshtml`**
   - Slide carousel (Bootstrap)
   - Progress bar
   - Previous/Next buttons

3. **`Views/TenantTraining/Examination.cshtml`**
   - Question list
   - Radio buttons (A, B, C, D)
   - Submit button

4. **`Views/TenantTraining/ExamResults.cshtml`**
   - Score display
   - Pass/Fail message
   - Retake button

**Reference:** Use Bootstrap 3 components (already in project)

---

### **5. Configure Email Templates** ? Estimated: 1 hour

**Files to create/update:**

1. **Email Service Method:**
```csharp
// Add to your email helper class
public void SendTrainingInvitation(PropertyLeaseApplication app, string token)
{
    var trainingLink = $"{BaseUrl}/TenantTraining/StartTraining?token={token}";
    
    var subject = "Your Pre-Tenancy Training Invitation - EHC";
    var body = $@"
        <html>
        <body>
            <h2>Pre-Tenancy Training Invitation</h2>
            <p>Dear {app.FirstName} {app.LastName},</p>
            <p>Click the link below to start your training:</p>
            <p><a href='{trainingLink}'>Start Training</a></p>
            <p>This link expires in 7 days.</p>
        </body>
        </html>
    ";
    
    SendEmail(app.PurEmail, subject, body);
}
```

2. **Pass/Fail Email Templates:**
   - SendExamPassEmail()
   - SendExamFailEmail()

---

### **6. Testing** ? Estimated: 2 hours

**Test Scenarios:**
- [ ] Admin can invite tenant
- [ ] Tenant receives email with valid link
- [ ] Training link validates token
- [ ] All 24 slides display correctly
- [ ] Cannot skip slides
- [ ] Training completion tracked
- [ ] Examination loads 16 questions
- [ ] Scoring calculates correctly (15/15 = pass)
- [ ] Can retake exam (max 3 attempts)
- [ ] Application status updates correctly

---

## ?? **QUICK REFERENCE - STATUS WORKFLOW**

```
Assessment Fee Paid
    ?
[Admin clicks "Invite to Training"]
    ?
s_awaiting_online_training (Status Key)
    ?
[Tenant clicks email link]
    ?
s_training_in_progress
    ?
[Completes 24 slides]
    ?
s_training_completed
    ?
[Takes examination]
    ?
    ?? Score 15/15 ? s_examination_passed ? Generate Lease
    ?? Score <15/15 ? s_examination_failed ? Retake (max 3x)
```

---

## ?? **EXAM GRADING LOGIC**

**Server-Side Validation (Controller):**
```csharp
// Get all 15 actual questions (exclude example)
var questions = db.ExaminationQuestions
    .Where(q => !q.IsExampleQuestion && q.IsActive)
    .OrderBy(q => q.QuestionOrder)
    .ToList();

int correctCount = 0;

foreach (var question in questions)
{
    var userAnswer = form[$"question_{question.Id}"];
    var isCorrect = userAnswer == question.CorrectAnswer;
    
    if (isCorrect) correctCount++;
    
    // Save answer
    db.TenantExamAnswers.Add(new TenantExamAnswer
    {
        PropertyLeaseApplicationId = applicationId,
        ExaminationQuestionId = question.Id,
        SelectedAnswer = userAnswer,
        IsCorrect = isCorrect,
        AttemptNumber = training.ExamAttempts + 1
    });
}

var score = (decimal)correctCount / 15 * 100;
var passed = correctCount == 15; // Must be 100%

training.ExamAttempts++;
training.ExamScore = score;

if (passed)
{
    training.IsExamPassed = true;
    training.ExamPassedDate = DateTime.Now;
    
    // Update application status
    application.StatusId = db.Status
        .First(s => s.Key == StatusKeys.ExaminationPassed).Id;
}
else
{
    application.StatusId = db.Status
        .First(s => s.Key == StatusKeys.ExaminationFailed).Id;
}

db.SaveChanges();
```

---

## ?? **CORRECT ANSWERS CHEAT SHEET**

```
Example: B
Q1:  B  (EHC owns unit)
Q2:  A  (Rental forever)
Q3:  B  (Call Complex Supervisor)
Q4:  C  (Both arrears & nuisance)
Q5:  A  (Deposit before signing)
Q6:  D  (All of the above)
Q7:  B  (Tenant responsible for bed)
Q8:  A  (EHC outside, tenant inside)
Q9:  B  (July 1st rent increase)
Q10: B  (Pay by 1st of month)
Q11: C  (Preferred debit order)
Q12: B  (1 month notice)
Q13: B  (Deposit refunded)
Q14: C  (Ask EHC permission)
Q15: B  (Apply to EHC to sublet)
```

**Passing Score:** 15/15 = 100%

---

## ?? **KEY FILES TO REFERENCE**

**Models:**
- `Models/TenantTraining.cs`
- `Models/TrainingSlide.cs`
- `Models/ExaminationQuestion.cs`
- `Models/TenantExamAnswer.cs`

**ViewModels:**
- `ViewModels/TenantTrainingViewModels.cs`

**Keys:**
- `Keys/StatusKeys.cs` (use the constants!)

**Existing Patterns:**
- `Controllers/PropertyLeaseApplicationController.cs` (reference for patterns)
- `Views/PropertyLeaseApplication/Index.cshtml` (reference for tables)

---

## ?? **IMPORTANT REMINDERS**

1. **Never expose correct answers to frontend** (keep in database only)
2. **Always validate token expiry** (7 days from creation)
3. **Track exam attempts** (max 3)
4. **Must score 100% to pass** (15/15 correct)
5. **Example question doesn't count** (IsExampleQuestion = true)

---

## ?? **IF YOU GET STUCK**

**Documentation:**
- `IMPLEMENTATION_GUIDE.md` - Full guide
- `DATABASE_SETUP_COMPLETE.md` - What's been done
- `MIGRATIONS_GUIDE.md` - Database help

**SQL Verification:**
```sql
-- Check training data
SELECT COUNT(*) FROM TrainingSlides -- Should be 24
SELECT COUNT(*) FROM ExaminationQuestions -- Should be 16

-- Check correct answers
SELECT QuestionOrder, CorrectAnswer 
FROM ExaminationQuestions 
WHERE IsExampleQuestion = 0
ORDER BY QuestionOrder
```

---

## ? **TODAY'S GOAL**

- [ ] Complete all 24 slide images (15 min)
- [ ] Commit database changes to Git (5 min)
- [ ] Review `IMPLEMENTATION_GUIDE.md` (10 min)
- [ ] Plan controller implementation (tomorrow)

**Total Time Today:** ~30 minutes

---

**?? You're doing great! Database is solid, now let's build the UI!**

---

Generated: January 30, 2026
Phase: Database Complete ?
Next: Slide Images ? Controller ? Views
