# ?? TENANT TRAINING SYSTEM - TESTING GUIDE

## ?? **PRE-REQUISITES**

Before testing, ensure you have:
- ? Built the solution successfully
- ? Run database migration scripts
- ? Seeded training data (24 slides + 16 questions)
- ? Uploaded at least 11 slide images to `/Content/Training Slides/`
- ? Logged in as **Community Development Officer** or **Super Administrator**

---

## ?? **QUICK START - OPTION 1: Use SQL Script**

### **Step 1: Run the Quick Test Script**

Open SSMS and run:
```sql
-- File: SQL Scripts/QuickTest_Training.sql
```

This will:
- Find an existing application
- Update it to status: "Assessment Fee Payment Approved"
- Clean up any old training records
- Display the Application ID

### **Step 2: Start Testing**

1. Navigate to: `http://localhost:PORT/TenantTraining/Index`
2. You should see the test application in the table
3. Click **"Invite to Training"** button
4. System sends email and creates training record

### **Step 3: Get Training Link**

Run this query to get the token:
```sql
SELECT 
    InvitationToken,
    'Navigate to: /TenantTraining/StartTraining?token=' + InvitationToken AS TrainingLink,
    TokenExpiryDate
FROM TenantTrainings
WHERE PropertyLeaseApplicationId = YOUR_APP_ID
ORDER BY CreatedDateTime DESC
```

### **Step 4: Complete Training**

1. Copy the token from query above
2. Navigate to: `/TenantTraining/StartTraining?token=YOUR_TOKEN`
3. View slides (use Next/Previous buttons)
4. Click "Complete Training" on last slide
5. Proceed to examination

### **Step 5: Take Exam**

Answer all 15 questions correctly using this answer key:

| Question | Answer |
|----------|--------|
| Example  | B      |
| Q1       | B      |
| Q2       | A      |
| Q3       | B      |
| Q4       | C      |
| Q5       | A      |
| Q6       | D      |
| Q7       | B      |
| Q8       | A      |
| Q9       | B      |
| Q10      | B      |
| Q11      | C      |
| Q12      | B      |
| Q13      | B      |
| Q14      | C      |
| Q15      | B      |

### **Step 6: View Results**

- ? If 15/15: See "PASSED" message
- ? If < 15/15: See "FAILED" message with retake option

---

## ?? **MONITORING QUERIES**

### **Check Training Progress:**
```sql
SELECT 
    a.ApplicationReferenceNumber,
    a.FirstName + ' ' + a.LastName AS Applicant,
    t.InvitationSentDate,
    t.TrainingStartedDate,
    t.TrainingCompletedDate,
    t.CurrentSlideNumber,
    t.IsTrainingCompleted,
    t.ExamAttempts,
    t.ExamScore,
    t.IsExamPassed,
    s.Name AS CurrentStatus
FROM TenantTrainings t
INNER JOIN PropertyLeaseApplications a ON t.PropertyLeaseApplicationId = a.Id
INNER JOIN Status s ON a.StatusId = s.Id
ORDER BY t.CreatedDateTime DESC
```

### **Check Exam Answers:**
```sql
SELECT 
    a.ApplicationReferenceNumber,
    q.QuestionOrder,
    LEFT(q.QuestionText, 50) AS Question,
    ea.SelectedAnswer AS YourAnswer,
    q.CorrectAnswer,
    ea.IsCorrect,
    ea.AttemptNumber
FROM TenantExamAnswers ea
INNER JOIN PropertyLeaseApplications a ON ea.PropertyLeaseApplicationId = a.Id
INNER JOIN ExaminationQuestions q ON ea.ExaminationQuestionId = q.Id
WHERE ea.PropertyLeaseApplicationId = YOUR_APP_ID
ORDER BY ea.AttemptNumber, q.QuestionOrder
```

### **Check Status Transitions:**
```sql
SELECT 
    a.ApplicationReferenceNumber,
    s.Name AS Status,
    s.[Key] AS StatusKey,
    a.ModifiedDateTime
FROM PropertyLeaseApplications a
INNER JOIN Status s ON a.StatusId = s.Id
WHERE a.Id = YOUR_APP_ID
ORDER BY a.ModifiedDateTime DESC
```

---

## ?? **EMAIL TESTING**

### **Training Invitation Email**
**Sent When:** Admin clicks "Invite to Training"
**Recipient:** Application email (PurEmail)
**Contains:** 
- Greeting with applicant name
- Training link with token
- Expiry notice (7 days)

### **Exam Pass Email**
**Sent When:** Tenant scores 15/15 (100%)
**Contains:**
- Congratulations message
- Score display
- Next steps information

### **Exam Fail Email**
**Sent When:** Tenant scores < 15/15
**Contains:**
- Current score
- Attempts remaining (max 3)
- Retry instructions

---

## ?? **TESTING SCENARIOS**

### **Scenario 1: Happy Path (Pass on First Attempt)**
1. ? Admin sends invitation
2. ? Tenant clicks email link
3. ? Tenant views all 24 slides
4. ? Tenant completes training
5. ? Tenant takes exam and scores 15/15
6. ? Status changes to "Examination Passed"
7. ? Pass email sent

**Expected Results:**
- `TenantTrainings.IsExamPassed = 1`
- `TenantTrainings.ExamScore = 100`
- `TenantTrainings.ExamAttempts = 1`
- `PropertyLeaseApplications.StatusId` = "Examination Passed"

---

### **Scenario 2: Fail and Retake**
1. ? Complete training
2. ? Answer some questions incorrectly (score < 100%)
3. ? Status changes to "Examination Failed"
4. ? Fail email sent with attempts remaining
5. ? Click "Retake Examination"
6. ? Answer all correctly this time
7. ? Status changes to "Examination Passed"

**Expected Results:**
- `TenantTrainings.ExamAttempts = 2`
- `TenantTrainings.IsExamPassed = 1`
- Two records in `TenantExamAnswers` (one per attempt)

---

### **Scenario 3: Maximum Attempts Reached**
1. ? Complete training
2. ? Fail exam (Attempt 1)
3. ? Fail exam (Attempt 2)
4. ? Fail exam (Attempt 3)
5. ? See "Maximum attempts reached" message
6. ? No retake button displayed
7. ? Contact EHC message shown

**Expected Results:**
- `TenantTrainings.ExamAttempts = 3`
- `TenantTrainings.IsExamPassed = 0`
- Cannot retake (button disabled)

---

### **Scenario 4: Token Expiry**
1. ? Admin sends invitation
2. ? Wait 7+ days (or manually update `TokenExpiryDate` in DB)
3. ? Try to access training link
4. ? See "Token Expired" message

**To Test Quickly:**
```sql
-- Expire the token immediately
UPDATE TenantTrainings
SET TokenExpiryDate = DATEADD(DAY, -1, GETDATE())
WHERE PropertyLeaseApplicationId = YOUR_APP_ID
```

---

### **Scenario 5: Slide Navigation**
1. ? Start training
2. ? Click "Next" button (progress bar updates)
3. ? Navigate to slide 5
4. ? Click "Previous" button (can go back)
5. ? Navigate to last slide
6. ? "Next" button becomes "Complete Training"

**Expected Results:**
- Progress bar shows correct percentage
- `TenantTrainings.CurrentSlideNumber` updates in database
- All slides display correctly

---

## ?? **TROUBLESHOOTING**

### **Issue: "Application not found" error**
**Solution:** 
```sql
-- Check application status
SELECT Id, ApplicationReferenceNumber, StatusId, s.Name
FROM PropertyLeaseApplications a
INNER JOIN Status s ON a.StatusId = s.Id
WHERE Id = YOUR_APP_ID
```
Ensure `StatusId` matches "Assessment Fee Payment Approved"

---

### **Issue: Training invitation email not sent**
**Solution:**
1. Check email helper is configured in web.config
2. Check SMTP settings
3. Check EventLog table for errors:
```sql
SELECT TOP 10 * 
FROM EventLog 
WHERE LogTypeKey = 'log_type_exception'
ORDER BY CreatedDateTime DESC
```

---

### **Issue: Slides not displaying (broken images)**
**Solution:**
1. Check files exist in `/Content/Training Slides/`
2. Verify file names: `slide-01.jpg` (not `Slide1.jpg`)
3. Check IIS has read permissions on folder
4. Check `TrainingSlides.ImagePath` column has correct paths

---

### **Issue: Exam not scoring correctly**
**Solution:**
```sql
-- Verify correct answers
SELECT 
    QuestionOrder,
    QuestionText,
    CorrectAnswer,
    IsExampleQuestion
FROM ExaminationQuestions
WHERE IsActive = 1 AND IsDeleted = 0
ORDER BY QuestionOrder
```
Ensure `CorrectAnswer` values are uppercase (A, B, C, D)

---

## ?? **SUCCESS CRITERIA**

After testing, verify:

| Test | Status |
|------|--------|
| Can send training invitation | ? |
| Email received with valid link | ? |
| Can navigate all slides | ? |
| Progress bar updates correctly | ? |
| Can complete training | ? |
| Exam loads with 15 questions | ? |
| Can submit exam answers | ? |
| Scoring is accurate (15/15 = pass) | ? |
| Pass email sent | ? |
| Fail email sent (with attempts) | ? |
| Can retake exam | ? |
| Status transitions correctly | ? |
| Database records created properly | ? |

---

## ?? **READY TO DEPLOY!**

Once all tests pass:

1. ? Upload remaining slide images (12-24)
2. ? Update production database
3. ? Configure production email settings
4. ? Train Community Development Officers
5. ? Monitor usage for first week
6. ? Collect feedback from tenants

---

## ?? **SUPPORT**

**For Technical Issues:**
- Check `EventLog` table for errors
- Review IIS logs
- Check database query execution times

**For Training Content Issues:**
- Update `TrainingSlides` table
- Update `ExaminationQuestions` table
- Re-run seed scripts if needed

---

**Generated:** January 30, 2026  
**System:** Tenant Training & Examination Module  
**Version:** 1.0
