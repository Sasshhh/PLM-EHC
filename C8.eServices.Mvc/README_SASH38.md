# ? TENANT TRAINING SYSTEM - READY FOR USER: sash38

## ?? **QUICK REFERENCE**

### **Your Account:**
- **Username:** sash38
- **Test Email:** testtraining@ehc.co.za
- **Role Required:** Community Development Officer or Admin

---

## ? **START TESTING IN 3 STEPS:**

### **1?? Run SQL Script** (30 seconds)
```
File: C8.eServices.Mvc\SQL Scripts\QuickTest_Training.sql
```
Opens SSMS ? Execute script ? Note Application ID

### **2?? Access Dashboard** (1 minute)
```
Navigate to: /TenantTraining/Index
Click: "Invite to Training" button
```

### **3?? Complete Training** (5 minutes)
```
Get token from database
Open: /TenantTraining/StartTraining?token=YOUR_TOKEN
View slides ? Complete training ? Take exam ? View results
```

---

## ?? **EXAM ANSWERS (Pass with 100%)**

**Use this sequence:** `B A B C A D B A B B C B B C B`

Or individually:
```
Example: B    Q6:  D    Q11: C
Q1:  B        Q7:  B    Q12: B
Q2:  A        Q8:  A    Q13: B
Q3:  B        Q9:  B    Q14: C
Q4:  C        Q10: B    Q15: B
Q5:  A
```

---

## ?? **FILES CREATED FOR YOU:**

| File | Purpose | Quick Action |
|------|---------|--------------|
| **QuickTest_Training.sql** | Fast setup (30 sec) | Run in SSMS |
| **Create_TestTrainingRecord.sql** | Detailed setup | Run in SSMS |
| **QUICKSTART_SASH38.md** | Your quick guide | Read first! |
| **TESTING_GUIDE.md** | Full test scenarios | Reference |
| **SYSTEM_COMPLETE_SUMMARY.md** | Complete docs | Deep dive |

---

## ?? **MUST-RUN QUERY (After Sending Invitation)**

```sql
-- Get your training link:
SELECT 
    '/TenantTraining/StartTraining?token=' + InvitationToken AS TrainingLink,
    TokenExpiryDate
FROM TenantTrainings
WHERE PropertyLeaseApplicationId IN (
    SELECT Id FROM PropertyLeaseApplications 
    WHERE CustomerId = (
        SELECT Id FROM Customers 
        WHERE SystemUserId = (
            SELECT Id FROM SystemUsers WHERE UserName = 'sash38'
        )
    )
)
ORDER BY CreatedDateTime DESC
```

**Copy the TrainingLink** and paste into browser!

---

## ? **VERIFICATION CHECKLIST**

Quick checks after each step:

**After SQL Script:**
- [ ] Application ID displayed
- [ ] Status = "Assessment Fee Payment Approved"

**After Invitation:**
- [ ] Record in `TenantTrainings` table
- [ ] Email sent (check logs)
- [ ] Status = "Awaiting Online Training"

**After Training:**
- [ ] `IsTrainingCompleted = 1`
- [ ] Status = "Training Completed"

**After Exam (Passed):**
- [ ] `IsExamPassed = 1`
- [ ] `ExamScore = 100`
- [ ] Status = "Examination Passed"

---

## ?? **TEST SCENARIOS**

Choose one to test:

### **Scenario 1: Perfect Pass** ? (5 min)
1. Send invitation
2. Complete all slides
3. Answer all 15 correctly
4. See pass message
**Result:** Status = "Examination Passed"

### **Scenario 2: Fail & Retake** (8 min)
1. Complete training
2. Fail exam (answer some wrong)
3. Click "Retake Examination"
4. Pass on 2nd attempt
**Result:** `ExamAttempts = 2`, Status = "Examination Passed"

### **Scenario 3: Token Expiry** (2 min)
1. Send invitation
2. Expire token manually in DB
3. Try to access link
4. See "Token Expired" message
**Result:** Error page displayed

---

## ?? **COMMON ISSUES & FIXES**

### **Can't find application in dashboard?**
```sql
-- Check status:
SELECT Id, ApplicationReferenceNumber, StatusId 
FROM PropertyLeaseApplications
WHERE CustomerId = (SELECT Id FROM Customers WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38'))
```
**Fix:** Re-run QuickTest_Training.sql

### **Training link doesn't work?**
```sql
-- Check token:
SELECT InvitationToken, TokenExpiryDate
FROM TenantTrainings
ORDER BY CreatedDateTime DESC
```
**Fix:** Token might be expired. Reset with:
```sql
UPDATE TenantTrainings
SET TokenExpiryDate = DATEADD(DAY, 7, GETDATE())
WHERE Id = YOUR_TRAINING_ID
```

### **Want to reset and start over?**
```sql
-- Full reset (run after getting Application ID from script):
DELETE FROM TenantExamAnswers WHERE PropertyLeaseApplicationId = YOUR_APP_ID
DELETE FROM TenantTrainings WHERE PropertyLeaseApplicationId = YOUR_APP_ID

UPDATE PropertyLeaseApplications
SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_assessment_fee_payment_approved')
WHERE Id = YOUR_APP_ID
```

---

## ?? **MONITOR YOUR PROGRESS**

Real-time check:
```sql
SELECT 
    'Status Check' AS Info,
    CASE 
        WHEN InvitationSentDate IS NOT NULL THEN '? Invited'
        ELSE '? Not Invited'
    END AS Invitation,
    CASE 
        WHEN TrainingStartedDate IS NOT NULL THEN '? Started'
        ELSE '? Not Started'
    END AS Training,
    CASE 
        WHEN IsTrainingCompleted = 1 THEN '? Completed'
        ELSE '? In Progress'
    END AS TrainingStatus,
    CASE 
        WHEN IsExamPassed = 1 THEN '? PASSED'
        WHEN ExamAttempts > 0 THEN '? Failed (Attempt ' + CAST(ExamAttempts AS VARCHAR) + ')'
        ELSE '? Not Taken'
    END AS ExamStatus
FROM TenantTrainings
WHERE PropertyLeaseApplicationId IN (
    SELECT Id FROM PropertyLeaseApplications 
    WHERE CustomerId = (
        SELECT Id FROM Customers WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')
    )
)
ORDER BY CreatedDateTime DESC
```

---

## ?? **YOU'RE ALL SET!**

Everything is configured for username: **sash38**

**Next Step:** 
1. Open SSMS
2. Run `QuickTest_Training.sql`
3. Start testing!

---

**Last Updated:** January 30, 2026  
**System Version:** 1.0  
**Status:** ? READY TO TEST
