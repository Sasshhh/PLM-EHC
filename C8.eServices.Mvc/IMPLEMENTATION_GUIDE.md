# TENANT TRAINING SYSTEM - COMPLETE IMPLEMENTATION GUIDE

## ? PHASE 1: DATABASE SETUP (DO THIS FIRST)

### **RECOMMENDED: Use Entity Framework Migrations**

**Open Package Manager Console** in Visual Studio and run:

```powershell
# Option A: Explicit Migration
Add-Migration AddTenantTrainingTables -Verbose
Update-Database -Verbose

# Option B: Automatic Migration (faster)
Update-Database -Verbose
```

**This will:**
- ? Create 8 tables (4 main + 4 audit)
- ? Seed 24 training slides
- ? Seed 16 examination questions with correct answers
- ? Seed 5 status keys
- ? All in one command!

**See detailed instructions:** `MIGRATIONS_GUIDE.md`

---

### **ALTERNATIVE: Manual SQL Scripts (Not Recommended)**

If you prefer manual SQL scripts:

#### Step 1: Run Database Migration
Execute the following SQL script against your `CRMPLMDEV_2025` database:

```
C8.eServices.Mvc/SQL Scripts/Create_TrainingTables_Migration.sql
```

This will create:
- 4 Main Tables: TenantTrainings, TrainingSlides, ExaminationQuestions, TenantExamAnswers
- 4 Audit Tables
- 5 New Status Keys

#### Step 2: Insert Training Data
Execute the following SQL script:

```
C8.eServices.Mvc/SQL Scripts/Insert_TrainingData.sql
```

This will insert:
- 24 Training Slides
- 16 Examination Questions (1 example + 15 actual questions)
- ALL CORRECT ANSWERS ARE PRE-CONFIGURED

---

## ? PHASE 2: CODE IMPLEMENTATION (ALREADY DONE)

### Files Created:
1. **Models** (? Complete):
   - `C8.eServices.Mvc/Models/TenantTraining.cs`
   - `C8.eServices.Mvc/Models/TrainingSlide.cs`
   - `C8.eServices.Mvc/Models/ExaminationQuestion.cs`
   - `C8.eServices.Mvc/Models/TenantExamAnswer.cs`

2. **Audit Models** (? Complete):
   - `C8.eServices.Mvc/Models/Audits/TenantTrainingAudit.cs`
   - `C8.eServices.Mvc/Models/Audits/TrainingSlideAudit.cs`
   - `C8.eServices.Mvc/Models/Audits/ExaminationQuestionAudit.cs`
   - `C8.eServices.Mvc/Models/Audits/TenantExamAnswerAudit.cs`

3. **DbContext** (? Updated):
   - `C8.eServices.Mvc/DataAccessLayer/eServicesDbContext.cs`
   - Added DbSets for all training tables
   - Added audit handling in SaveAudit() method

4. **ViewModels** (? Complete):
   - `C8.eServices.Mvc/ViewModels/TenantTrainingViewModels.cs`

---

## ?? PHASE 3: CONTROLLER & VIEWS (NEXT STEPS)

### Controller to Create:
`C8.eServices.Mvc/Controllers/TenantTrainingController.cs`

**Required Actions:**
1. `Index()` - Admin: View all tenant training records
2. `InviteToTraining(int applicationId)` - Admin: Send training invitation
3. `StartTraining(string token)` - Tenant: Begin training with secure link
4. `ViewSlide(int slideIndex)` - Tenant: Navigate training slides
5. `CompleteTraining()` - Tenant: Mark training complete
6. `StartExamination()` - Tenant: Begin exam
7. `SubmitExamination(Dictionary<int, string> answers)` - Tenant: Submit exam answers
8. `ViewExamResults(int attemptNumber)` - Tenant: View exam results

### Views to Create:
1. **`Views/TenantTraining/Index.cshtml`**
   - Admin dashboard to manage trainings
   - Table showing all applications with training status
   - "Invite to Training" buttons

2. **`Views/TenantTraining/TrainingProgramme.cshtml`**
   - Slide carousel (24 slides)
   - Navigation buttons (Previous/Next)
   - Progress bar
   - Display slide images from `/Content/Training Slides/`
   - "Complete Training" button on last slide

3. **`Views/TenantTraining/Examination.cshtml`**
   - Display all 15 questions + 1 example
   - Multiple choice options (A, B, C, D)
   - "Submit Examination" button
   - Timer (optional)

4. **`Views/TenantTraining/ExamResults.cshtml`**
   - Display score (must be 100% to pass)
   - Show correct/incorrect answers
   - "Retake Exam" button (max 3 attempts)
   - Success message if passed

---

## ?? BUSINESS RULES

### Training Rules:
1. Tenant must view ALL 24 slides in order
2. Cannot skip slides
3. Must click "Complete Training" on last slide
4. Training link expires after 7 days

### Examination Rules:
1. Must score **100%** to pass (15 out of 15 correct)
2. Maximum **3 attempts** allowed
3. Questions randomized on each attempt
4. Example question pre-circled (not counted in score)
5. Must pass exam before lease agreement generation

### Status Workflow:
```
Assessment Fee Approved 
    ?
[Invite to Training] 
    ?
s_awaiting_online_training 
    ?
[Tenant Clicks Link]
    ?
s_training_in_progress 
    ?
[Complete All 24 Slides]
    ?
s_training_completed 
    ?
[Pass Examination 100%]
    ?
s_examination_passed 
    ?
Generate Lease Agreement
```

---

## ?? EMAIL NOTIFICATIONS

### 1. Training Invitation Email
**Trigger:** Admin clicks "Invite to Client Training"
**Recipients:** Tenant email
**Content:**
```
Subject: Your Pre-Tenancy Training Invitation - EHC Social Housing

Dear [Tenant Name],

Congratulations! You have been invited to complete the mandatory Pre-Tenancy Training Programme.

This training is required before we can proceed with your lease agreement.

Click here to start your training:
[SECURE LINK WITH TOKEN]

The training consists of:
- 24 informative slides (approx. 15-20 minutes)
- 15 question examination (must score 100%)
- Maximum 3 exam attempts

This link expires in 7 days.

Best regards,
Ekurhuleni Housing Company
```

### 2. Exam Pass Email
**Trigger:** Tenant passes exam (100%)
**Content:**
```
Subject: Congratulations! You Passed the Pre-Tenancy Examination

Dear [Tenant Name],

Well done! You have successfully passed the Pre-Tenancy Examination with 100%.

Your Score: 15/15 (100%)

Your application will now proceed to lease agreement generation.

Best regards,
Ekurhuleni Housing Company
```

### 3. Exam Fail Email
**Trigger:** Tenant fails exam (<100%)
**Content:**
```
Subject: Pre-Tenancy Examination Results

Dear [Tenant Name],

Your Score: [X]/15 ([X]%)
Required Score: 100%

Attempts Remaining: [X] of 3

Please review the training material and try again.

Best regards,
Ekurhuleni Housing Company
```

---

## ?? SECURITY CONSIDERATIONS

1. **Training Link Token:**
   - Generate unique GUID for each invitation
   - Store in `TenantTrainings.TrainingLinkToken`
   - Set expiry date (7 days)
   - Validate token before allowing access

2. **Exam Integrity:**
   - Questions stored in database (NOT hardcoded)
   - Correct answers hidden from frontend
   - Server-side validation only
   - Track attempt numbers

3. **Authorization:**
   - Only assigned tenant can access their training
   - Admin role required to invite tenants
   - No public access to training content

---

## ?? TESTING CHECKLIST

### Admin Functions:
- [ ] View tenant training dashboard
- [ ] Send training invitation
- [ ] Email received with valid link
- [ ] Link expires after 7 days
- [ ] Can re-send invitation if needed

### Tenant Functions:
- [ ] Click training link from email
- [ ] View all 24 slides
- [ ] Cannot skip slides
- [ ] Progress saved if interrupted
- [ ] Complete training
- [ ] Access examination
- [ ] Submit examination
- [ ] View results
- [ ] Retake exam (max 3 attempts)
- [ ] Pass with 100%
- [ ] Application status updates correctly

### Database:
- [ ] TenantTrainings record created
- [ ] Slide progress tracked
- [ ] Exam answers stored
- [ ] Audit trails created
- [ ] Correct answers validated server-side

---

## ?? CORRECT ANSWERS REFERENCE

| Question | Answer | Description |
|----------|--------|-------------|
| Example  | B      | The morning |
| Q1       | B      | EHC |
| Q2       | A      | Rental forever |
| Q3       | B      | Call Complex Supervisor |
| Q4       | C      | Both of the above |
| Q5       | A      | Before you sign the lease |
| Q6       | D      | All of the above |
| Q7       | B      | The tenant |
| Q8       | A      | EHC outside, tenant inside |
| Q9       | B      | 1st of July |
| Q10      | B      | 1st of each month |
| Q11      | C      | By preferred debit order |
| Q12      | B      | 1 month notice |
| Q13      | B      | Deposit refunded |
| Q14      | C      | Ask EHC permission first |
| Q15      | B      | Apply to EHC to sublet |

---

## ?? DEPLOYMENT STEPS

### 1. Database Setup:
```sql
-- Execute on CRMPLMDEV_2025 database
1. Run: Create_TrainingTables_Migration.sql
2. Run: Insert_TrainingData.sql
3. Verify: SELECT COUNT(*) FROM TrainingSlides (should be 24)
4. Verify: SELECT COUNT(*) FROM ExaminationQuestions (should be 16)
```

### 2. Image Setup:
```
1. Screenshot all 24 PowerPoint slides
2. Save as: slide-01.jpg through slide-24.jpg
3. Place in: C8.eServices.Mvc/Content/Training Slides/
4. Verify paths match SQL script
```

### 3. Code Deployment:
```
1. Build solution (already successful ?)
2. Commit changes to Git
3. Push to main branch
4. Deploy to test environment
5. Run integration tests
```

### 4. Testing:
```
1. Create test application
2. Approve assessment fee payment
3. Click "Invite to Client Training"
4. Check email received
5. Complete training flow
6. Verify exam scoring
7. Confirm status updates
```

---

## ?? TROUBLESHOOTING

### Issue: Training link not working
**Solution:** Check token expiry date, regenerate invitation

### Issue: Exam not scoring correctly
**Solution:** Verify CorrectAnswer values in ExaminationQuestions table

### Issue: Images not displaying
**Solution:** Check file paths in TrainingSlides.ImagePath column

### Issue: Cannot retake exam
**Solution:** Check ExamAttempts < 3 in TenantTrainings table

---

## ?? SUPPORT

For implementation questions:
- Check database migration logs
- Verify all 8 tables created
- Confirm 24 slides + 16 questions inserted
- Review build output for errors

---

## ? COMPLETION CHECKLIST

- [x] Database migration script created
- [x] Training data SQL script created
- [x] Models created (4 main + 4 audit)
- [x] DbContext updated
- [x] ViewModels created
- [x] Build successful (no errors)
- [ ] Controller implementation (TODO)
- [ ] Views implementation (TODO)
- [ ] Email templates configured (TODO)
- [ ] Testing completed (TODO)
- [ ] Deployed to production (TODO)

---

**?? CONGRATULATIONS! Phase 1 & 2 Complete!**

**Next:** Implement TenantTrainingController and Views following the patterns in your existing PropertyLeaseApplicationController.

**Estimated Time:** 4-6 hours for controller + views + testing

---

Generated: 2025-01-15
System: Tenant Training & Examination Module
Version: 1.0
