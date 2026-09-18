# ? EXAMINATION FLOW - VERIFICATION & TESTING

## ?? **CURRENT LOGIC (VERIFIED CORRECT)**

### **? Passing (15/15 = 100%)**
```csharp
if (passed) // passed = (correctCount == questions.Count)
{
    training.IsExamPassed = true;
    training.ExamPassedDate = DateTime.Now;
    
    // Status changes to PASSED
    training.PropertyLeaseApplication.StatusId = db.Status
        .FirstOrDefault(s => s.Key == StatusKeys.ExaminationPassed)?.Id;
    
    // Send pass email
    SendExamPassEmail(...);
}
```

**Result:**
- ? Status: `s_examination_passed`
- ? Email: "Congratulations! You Passed"
- ? Next Step: Generate Lease Agreement (NOT inspection slots)

---

### **? Failing (<15/15)**
```csharp
else // Failed
{
    // Status changes to FAILED
    training.PropertyLeaseApplication.StatusId = db.Status
        .FirstOrDefault(s => s.Key == StatusKeys.ExaminationFailed)?.Id;
    
    // Send fail email
    SendExamFailEmail(...);
}
```

**Result:**
- ? Status: `s_examination_failed`
- ? Email: "Unfortunately, you did not pass"
- ?? Can retake if attempts < 3

---

## ?? **TESTING SCENARIOS**

### **Scenario 1: Pass on First Attempt (15/15)**

**Steps:**
1. Complete all 24 training slides
2. Take examination
3. Answer all 15 questions correctly (use answer key)
4. Submit

**Expected Results:**
```sql
-- Verify in database:
SELECT 
    pla.ApplicationReferenceNumber,
    s.Name AS Status,
    s.[Key] AS StatusKey,
    tt.IsExamPassed,
    tt.ExamAttempts,
    tt.ExamScore,
    tt.ExamPassedDate
FROM TenantTrainings tt
INNER JOIN PropertyLeaseApplications pla ON tt.PropertyLeaseApplicationId = pla.Id
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.ApplicationReferenceNumber = 'EHC2023101800005'
```

**Should Show:**
- StatusKey: `s_examination_passed` ?
- IsExamPassed: `1` (TRUE)
- ExamAttempts: `1`
- ExamScore: `100.00`
- ExamPassedDate: `2026-02-01 ...`

**? Should NOT be:** `s_awaiting_inspection_schedule_slots`

---

### **Scenario 2: Fail on First Attempt (14/15)**

**Steps:**
1. Complete training
2. Take examination
3. Answer 14 correctly, 1 wrong
4. Submit

**Expected Results:**
- StatusKey: `s_examination_failed` ?
- IsExamPassed: `0` (FALSE)
- ExamAttempts: `1`
- ExamScore: `93.33` (14/15)
- ExamPassedDate: `NULL`

**Results Page Should Show:**
- ? "Unfortunately, you did not pass"
- ?? Score: 14/15 (93%)
- ?? "You have 2 attempt(s) remaining"
- ?? List of incorrect answers with correct answers shown
- ?? "Retake Examination" button (visible)

---

### **Scenario 3: Retake and Pass (Attempt 2)**

**Steps:**
1. From results page, click "Retake Examination"
2. Review correct answers from previous attempt
3. Answer all 15 correctly
4. Submit

**Expected Results:**
- StatusKey: `s_examination_passed` ?
- IsExamPassed: `1`
- ExamAttempts: `2`
- ExamScore: `100.00`

---

### **Scenario 4: Max Attempts Reached (3 Fails)**

**Steps:**
1. Fail attempt 1
2. Fail attempt 2
3. Fail attempt 3
4. Try to access exam again

**Expected Results After 3rd Fail:**
- StatusKey: `s_examination_failed`
- IsExamPassed: `0`
- ExamAttempts: `3`
- "Retake Examination" button: Hidden
- Message: "Maximum attempts reached. Please contact EHC."

**Trying to access `/Examination?trainingId=X` again:**
- Should redirect to `_MaxAttemptsReached` view
- Shows: "You have reached the maximum number of attempts (3)"

---

## ?? **VERIFICATION QUERIES**

### **Check Current Status:**
```sql
SELECT 
    pla.ApplicationReferenceNumber,
    pla.FirstName + ' ' + pla.LastName AS Applicant,
    s.Name AS Status,
    s.[Key] AS StatusKey,
    tt.IsTrainingCompleted,
    tt.IsExamPassed,
    tt.ExamAttempts,
    tt.ExamScore,
    CASE 
        WHEN s.[Key] = 's_examination_passed' THEN '? CORRECT - Passed exam'
        WHEN s.[Key] = 's_examination_failed' THEN '? Failed - Can retake'
        WHEN s.[Key] = 's_awaiting_inspection_schedule_slots' THEN '?? WRONG STATUS!'
        ELSE 'Other: ' + s.[Key]
    END AS StatusCheck
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
INNER JOIN TenantTrainings tt ON pla.Id = tt.PropertyLeaseApplicationId
WHERE pla.ApplicationReferenceNumber = 'EHC2023101800005'
```

---

### **Check Exam Answers (Latest Attempt):**
```sql
DECLARE @AppId INT = (
    SELECT Id FROM PropertyLeaseApplications 
    WHERE ApplicationReferenceNumber = 'EHC2023101800005'
);

DECLARE @LatestAttempt INT = (
    SELECT MAX(AttemptNumber) FROM TenantExamAnswers 
    WHERE PropertyLeaseApplicationId = @AppId
);

SELECT 
    q.QuestionOrder AS [Q#],
    LEFT(q.QuestionText, 60) AS [Question],
    'A: ' + q.OptionA AS [Option A],
    'B: ' + q.OptionB AS [Option B],
    'C: ' + q.OptionC AS [Option C],
    'D: ' + q.OptionD AS [Option D],
    ea.SelectedAnswer AS [Your Answer],
    q.CorrectAnswer AS [Correct Answer],
    CASE WHEN ea.IsCorrect = 1 THEN '? Correct' ELSE '? Wrong' END AS [Result]
FROM TenantExamAnswers ea
INNER JOIN ExaminationQuestions q ON ea.ExaminationQuestionId = q.Id
WHERE ea.PropertyLeaseApplicationId = @AppId
    AND ea.AttemptNumber = @LatestAttempt
    AND q.IsExampleQuestion = 0
ORDER BY q.QuestionOrder;

-- Summary
SELECT 
    @LatestAttempt AS [Attempt Number],
    COUNT(*) AS [Total Questions],
    SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS [Correct Answers],
    SUM(CASE WHEN IsCorrect = 0 THEN 1 ELSE 0 END) AS [Wrong Answers],
    CAST(SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS [Score %]
FROM TenantExamAnswers
WHERE PropertyLeaseApplicationId = @AppId
    AND AttemptNumber = @LatestAttempt;
```

---

## ?? **ANSWER KEY FOR TESTING**

### **? 100% Pass Answers:**
```
Example: B
Q1:  B
Q2:  A
Q3:  B
Q4:  C
Q5:  A
Q6:  D
Q7:  B
Q8:  A
Q9:  B
Q10: B
Q11: C
Q12: B
Q13: B
Q14: C
Q15: B
```

### **? Intentional Fail (93% - 14/15 correct):**
Change Q1 from B to A (all others correct)

### **? Intentional Fail (86% - 13/15 correct):**
Change Q1 to A and Q2 to B (all others correct)

---

## ?? **RETAKE LOGIC**

### **Can Retake If:**
```csharp
CanRetake = (attemptNumber < 3) && !training.IsExamPassed
```

**Examples:**
- Attempt 1 Failed: ? Can retake (2 attempts left)
- Attempt 2 Failed: ? Can retake (1 attempt left)
- Attempt 3 Failed: ? Cannot retake (max reached)
- Attempt 1 Passed: ? Cannot retake (already passed)

---

## ?? **EMAIL CONTENT**

### **Pass Email (15/15):**
```
Subject: Congratulations! You Passed the Pre-Tenancy Examination

Dear [Name],

Congratulations! You have successfully passed the pre-tenancy examination.

Your Score: 15/15 (100%)

You will be contacted shortly regarding the next steps in your application process.

Best regards,
Ekurhuleni Housing Company
```

### **Fail Email (Attempt 1, 14/15):**
```
Subject: Pre-Tenancy Examination Results

Dear [Name],

Thank you for completing the pre-tenancy examination.

Your Score: 14/15 (93%)
Required to Pass: 15/15 (100%)
Attempt: 1 of 3

You have 2 attempt(s) remaining. Please review the training material and try again.

Best regards,
Ekurhuleni Housing Company
```

### **Fail Email (Attempt 3, max reached):**
```
Subject: Pre-Tenancy Examination Results

Dear [Name],

Thank you for completing the pre-tenancy examination.

Your Score: 12/15 (80%)
Required to Pass: 15/15 (100%)
Attempt: 3 of 3

Maximum attempts reached. Please contact EHC for further assistance.

Best regards,
Ekurhuleni Housing Company
```

---

## ? **TESTING CHECKLIST**

### **Progress Tracker:**
- [ ] Navigate through all 24 slides
- [ ] Verify progress bar shows "Slide X of 24"
- [ ] Verify database `CurrentSlideNumber` updates
- [ ] Verify slide content displays correctly
- [ ] Verify images load correctly

### **Examination:**
- [ ] All 15 questions display
- [ ] Example question shows (not counted)
- [ ] Can select A, B, C, or D for each
- [ ] Submit button works
- [ ] Loading indicator shows during grading

### **Pass Scenario (15/15):**
- [ ] Results page shows "CONGRATULATIONS!"
- [ ] Score shows: 15/15 (100%)
- [ ] Status: `s_examination_passed`
- [ ] "Continue" button visible
- [ ] "Retake" button hidden
- [ ] Pass email sent

### **Fail Scenario (<15/15):**
- [ ] Results page shows "Unfortunately..."
- [ ] Score shows: X/15 (Y%)
- [ ] Attempts remaining shown (3 - X)
- [ ] Incorrect answers highlighted
- [ ] Correct answers shown
- [ ] "Retake Examination" button visible
- [ ] Status: `s_examination_failed`
- [ ] Fail email sent

### **Retake Scenario:**
- [ ] Can click "Retake Examination"
- [ ] Exam reloads with fresh questions
- [ ] Previous answers NOT pre-filled
- [ ] Attempt counter increments
- [ ] Can submit again

### **Max Attempts:**
- [ ] After 3 fails, "Retake" button hidden
- [ ] Message: "Maximum attempts reached"
- [ ] Cannot access `/Examination` URL
- [ ] Redirects to `_MaxAttemptsReached` view

### **Status Transitions:**
- [ ] Training complete: `s_training_completed`
- [ ] Exam passed: `s_examination_passed`
- [ ] Exam failed: `s_examination_failed`
- [ ] ? Should NEVER be `s_awaiting_inspection_schedule_slots` until after passing

---

## ?? **COMMON ISSUES & FIXES**

### **Issue 1: Status Changed to Inspection Slots**
**Symptom:** After failing exam, status is `s_awaiting_inspection_schedule_slots`

**Root Cause:** Wrong status key used in controller

**Fix:** Verify this code in `SubmitExamination`:
```csharp
// ? CORRECT:
.FirstOrDefault(s => s.Key == StatusKeys.ExaminationPassed)

// ? WRONG:
.FirstOrDefault(s => s.Key == StatusKeys.AwaitingInspectionScheduleSlots)
```

---

### **Issue 2: Can't Retake After Fail**
**Symptom:** "Retake" button not showing after fail

**Check:**
```sql
SELECT ExamAttempts, IsExamPassed, MaxAttempts = 3
FROM TenantTrainings
WHERE PropertyLeaseApplicationId = YOUR_APP_ID
```

**Should Show:**
- ExamAttempts: < 3
- IsExamPassed: 0

---

### **Issue 3: Progress Bar Shows Wrong Count**
**Symptom:** Says "Slide 1 of 23" instead of "Slide 1 of 24"

**Check:**
```sql
SELECT COUNT(*) AS TotalSlides
FROM TrainingSlides
WHERE IsActive = 1 AND IsDeleted = 0
```

**Should return:** 24

---

## ?? **FINAL VERIFICATION**

After completing all testing, run this comprehensive check:

```sql
-- Complete System Verification
SELECT 
    '=== TRAINING RECORD ===' AS Section,
    pla.ApplicationReferenceNumber AS AppRef,
    tt.Id AS TrainingId,
    tt.InvitationSentDate AS Invited,
    tt.TrainingStartedDate AS Started,
    tt.TrainingCompletedDate AS Completed,
    tt.CurrentSlideNumber AS [Current Slide],
    tt.IsTrainingCompleted AS [Training Done],
    tt.ExamAttempts AS [Exam Attempts],
    tt.ExamScore AS [Exam Score],
    tt.IsExamPassed AS [Exam Passed],
    s.Name AS [Current Status],
    s.[Key] AS [Status Key],
    CASE 
        WHEN s.[Key] = 's_examination_passed' AND tt.IsExamPassed = 1 THEN '? SUCCESS'
        WHEN s.[Key] = 's_examination_failed' AND tt.IsExamPassed = 0 AND tt.ExamAttempts < 3 THEN '?? CAN RETAKE'
        WHEN s.[Key] = 's_examination_failed' AND tt.ExamAttempts >= 3 THEN '? MAX ATTEMPTS'
        WHEN s.[Key] = 's_awaiting_inspection_schedule_slots' THEN '?? WRONG STATUS!'
        ELSE 'In Progress'
    END AS ValidationResult
FROM TenantTrainings tt
INNER JOIN PropertyLeaseApplications pla ON tt.PropertyLeaseApplicationId = pla.Id
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.ApplicationReferenceNumber = 'EHC2023101800005';
```

---

**? System is working correctly if:**
1. Can complete all 24 slides
2. Progress tracker shows "Slide X of 24"
3. Passing (15/15) sets status to `s_examination_passed`
4. Failing (<15/15) sets status to `s_examination_failed`
5. Can retake up to 3 times
6. Correct answers shown after fail
7. Email notifications sent
8. ? **NEVER** goes to `s_awaiting_inspection_schedule_slots` unless passed

---

**Created:** 2026-02-01  
**Purpose:** Comprehensive examination flow testing  
**Status:** Ready for testing ?
