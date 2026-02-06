# ?? TENANT TRAINING - QUICK START GUIDE
## For User: **sash38**

---

## ? **30-SECOND SETUP**

### **Step 1: Run SQL Script**
Open **SQL Server Management Studio** and execute:

```sql
-- File Location: C8.eServices.Mvc\SQL Scripts\QuickTest_Training.sql
```

**What it does:**
- ? Finds your customer account (username: `sash38`)
- ? Creates or updates a test application
- ? Sets status to "Assessment Fee Payment Approved"
- ? Cleans up old training records
- ? Displays the Application ID

---

## ?? **TESTING WORKFLOW**

### **1. Access Admin Dashboard** (Logged in as: sash38)
```
Navigate to: http://localhost:PORT/TenantTraining/Index
```

You should see a table with applications ready for training invitation.

---

### **2. Send Training Invitation**

1. Find the test application in the table:
   - **Applicant:** John TestTraining
   - **Email:** testtraining@ehc.co.za
   - **Status:** Assessment Fee Payment Approved

2. Click the **"Invite to Training"** button

3. System will:
   - ? Generate unique token
   - ? Create TenantTraining record
   - ? Send email notification
   - ? Update status to "Awaiting Online Training"

---

### **3. Get Training Link**

Run this query in SSMS to get the token:

```sql
SELECT 
    'Training Link:' AS Info,
    '/TenantTraining/StartTraining?token=' + InvitationToken AS Link,
    InvitationToken AS Token,
    TokenExpiryDate
FROM TenantTrainings
WHERE PropertyLeaseApplicationId IN (
    SELECT TOP 1 Id 
    FROM PropertyLeaseApplications 
    WHERE CustomerId = (
        SELECT Id FROM Customers 
        WHERE SystemUserId = (
            SELECT Id FROM SystemUsers WHERE UserName = 'sash38'
        )
    )
    ORDER BY CreatedDateTime DESC
)
ORDER BY CreatedDateTime DESC
```

**Copy the token** and navigate to:
```
http://localhost:PORT/TenantTraining/StartTraining?token=YOUR_TOKEN
```

---

### **4. Complete Training**

1. **View Slides:**
   - Click "Next" to advance through slides
   - Click "Previous" to go back
   - Progress bar shows your position

2. **Complete Training:**
   - On the last slide (Slide 24), click **"Complete Training"**
   - System automatically redirects to examination

---

### **5. Take Examination**

**Important:** You must answer ALL 15 questions correctly (100%) to pass!

#### **?? Answer Key (Use these to pass):**

| Question # | Correct Answer |
|------------|----------------|
| Example    | B              |
| Q1         | B              |
| Q2         | A              |
| Q3         | B              |
| Q4         | C              |
| Q5         | A              |
| Q6         | D              |
| Q7         | B              |
| Q8         | A              |
| Q9         | B              |
| Q10        | B              |
| Q11        | C              |
| Q12        | B              |
| Q13        | B              |
| Q14        | C              |
| Q15        | B              |

**Quick Cheat:** `B A B C A D B A B B C B B C B`

---

### **6. View Results**

After submitting:
- ? **If 15/15:** See "CONGRATULATIONS! YOU PASSED!" message
- ? **If < 15/15:** See "Unfortunately, you did not pass" message
  - You can retake up to 3 times
  - Review correct answers shown on results page

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
WHERE a.CustomerId = (
    SELECT Id FROM Customers 
    WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')
)
ORDER BY t.CreatedDateTime DESC
```

### **Check Exam Answers:**
```sql
SELECT 
    q.QuestionOrder AS [Q#],
    LEFT(q.QuestionText, 50) AS Question,
    ea.SelectedAnswer AS [Your Answer],
    q.CorrectAnswer AS [Correct],
    CASE WHEN ea.IsCorrect = 1 THEN '?' ELSE '?' END AS [Result],
    ea.AttemptNumber
FROM TenantExamAnswers ea
INNER JOIN ExaminationQuestions q ON ea.ExaminationQuestionId = q.Id
INNER JOIN PropertyLeaseApplications a ON ea.PropertyLeaseApplicationId = a.Id
WHERE a.CustomerId = (
    SELECT Id FROM Customers 
    WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')
)
ORDER BY ea.AttemptNumber DESC, q.QuestionOrder
```

---

## ? **EXPECTED STATUS FLOW**

```
Assessment Fee Payment Approved
    ?
[Click: Invite to Training]
    ?
Awaiting Online Training
    ?
[Click email link]
    ?
Training In Progress
    ?
[Complete all slides]
    ?
Training Completed
    ?
[Take exam]
    ?
?? Pass (15/15) ? Examination Passed ?
?? Fail (<15/15) ? Examination Failed ? (can retake)
```

---

## ?? **TROUBLESHOOTING**

### **Issue: "Application not found" in dashboard**
**Solution:**
```sql
-- Check application status
SELECT 
    Id,
    ApplicationReferenceNumber,
    s.Name AS Status,
    CustomerId
FROM PropertyLeaseApplications a
INNER JOIN Status s ON a.StatusId = s.Id
WHERE CustomerId = (
    SELECT Id FROM Customers 
    WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')
)
ORDER BY CreatedDateTime DESC
```

### **Issue: Token expired**
**Solution:**
```sql
-- Reset token expiry
UPDATE TenantTrainings
SET TokenExpiryDate = DATEADD(DAY, 7, GETDATE())
WHERE PropertyLeaseApplicationId = YOUR_APP_ID
```

### **Issue: Can't retake exam**
**Solution:**
```sql
-- Reset exam attempts
UPDATE TenantTrainings
SET ExamAttempts = 0,
    IsExamPassed = 0,
    ExamScore = NULL
WHERE PropertyLeaseApplicationId = YOUR_APP_ID

-- Clear previous answers
DELETE FROM TenantExamAnswers
WHERE PropertyLeaseApplicationId = YOUR_APP_ID
```

---

## ?? **EMAIL NOTIFICATIONS**

### **Training Invitation Email**
- **Sent to:** testtraining@ehc.co.za
- **Contains:** Training link with token
- **Valid for:** 7 days

### **Exam Pass Email**
- **Sent to:** testtraining@ehc.co.za
- **Contains:** Congratulations, score, next steps

### **Exam Fail Email**
- **Sent to:** testtraining@ehc.co.za
- **Contains:** Score, attempts remaining, retry instructions

---

## ?? **SUCCESS CHECKLIST**

After testing, verify:

- [ ] Can see application in `/TenantTraining/Index`
- [ ] Can send training invitation
- [ ] Training record created in database
- [ ] Can access training link with token
- [ ] Can navigate through slides
- [ ] Progress bar updates correctly
- [ ] Can complete training
- [ ] Exam loads with 15 questions
- [ ] Can submit exam
- [ ] Results page displays correctly
- [ ] Status changes to "Examination Passed"
- [ ] Email notifications received

---

## ?? **NEED HELP?**

Check these files for detailed documentation:
- `TESTING_GUIDE.md` - Complete testing scenarios
- `SYSTEM_COMPLETE_SUMMARY.md` - Full system documentation
- `SQL Scripts/Create_TestTrainingRecord.sql` - Detailed setup script

---

**Generated for:** sash38  
**Date:** January 30, 2026  
**System:** Tenant Training & Examination Module v1.0
