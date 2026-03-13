# PLM EHC — Test Seed Scripts

Scripts in this folder insert complete, validated test records directly into the database so you can test the full workflow without manually capturing through the UI each time.

---

## How to run a script

```
sqlcmd -S localhost -d CRMPLMDEV_2025 -E -i "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts\<script_name>.sql"
```

Or open the file in **SSMS** and execute against `CRMPLMDEV_2025`.

Each script:
- Runs inside a transaction — if anything fails it rolls back cleanly
- Prints the new Application ID and Reference Number on success
- Generates a **unique reference number** per run so you can run it multiple times

---

## Where the code lives

| Area | Location |
|------|----------|
| Main controller | `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs` |
| Round-robin assignment logic | `PropertyLeaseApplicationController.EHCRoundRobin(...)` |
| Area / officer mapping | `AreaManagerController.cs` ? `PreferredComplexAreas` table |
| Layout / nav | `C8.eServices.Mvc\Views\Shared\RCS_Layout.cshtml` |
| Risk assessment views | `C8.eServices.Mvc\Views\PropertyLeaseApplication\` |
| Use case documentation | `C8.eServices.Mvc\Use cases\Usecase summary.txt` |

### Key database tables

| Table | Purpose |
|-------|---------|
| `PropertyLeaseApplications` | Main application record |
| `RoundRobinQueues` | Work queue — assigns tasks to officers |
| `ResponsibilityTypes` | Defines each process step (Risk Assessment, Inspections, etc.) |
| `Status` | All application statuses |
| `PreferredComplexAreas` | Complex areas — holds `LettingOfficerId` for routing |
| `AppSettings` | Fallback officer IDs (keys: `u_letting_officer`, `r_revenue_officer`, etc.) |
| `MonthlyIncomes` | Applicant income — required for risk assessment |
| `MonthlyExpenses` | Applicant expenses |
| `Documents` + `Files` | Uploaded supporting documents |
| `PLMApplicationHistortyLogs` | Full audit trail per application |

---

## Test Cases

### ? TC-01 — Risk Assessment
**Script:** `01_risk_assessment_test_run.sql`
**Status inserted:** `Awaiting Risk Assessment Outcome` (Status Id 137)
**Assigned to:** AshKay — Customer 190 (Client Services Officer)
**Area:** Airport Park (PreferredComplexAreaId 24)
**Login as:** AshKay
**Start at:** `http://localhost:3450/PropertyLeaseApplication/ApplicantRiskAssessment`

**What it inserts:**
- 1× `PropertyLeaseApplications` — Female applicant, Airport Park, R20,000 income
- 1× `MonthlyIncomes` — R20,000 gross, R15,000 net
- 1× `MonthlyExpenses` — all zeros
- 8× `Files` + `Documents` — all required document check list items (IDs 100,98,99,102,103,104,101,118)
- 5× `PLMApplicationHistortyLogs` — capture through to fee validation
- 1× `RoundRobinQueues` — Risk Assessment task assigned to AshKay

---

### ? TC-02 — Tenant Training: Schedule Training Slot
**Script:** `02_tenant_training_schedule_test_run.sql`
**Status inserted:** `Payment Validated and Approved` (Status Id 122 — `s_rcs_assessment_payment_approved`)
**Customer:** Sashen Moodley — Customer 45 (login: `Sash38`)
**Area:** Airport Park (PreferredComplexAreaId 24)
**Login as:** Sash38 (customer portal)
**Start at:** `http://localhost:3450/TenantTraining/MyScheduledTraining`

**What it inserts:**
- 1× `PropertyLeaseApplications` — Sashen Moodley, Airport Park, status 122 (AssessmentFeePaymentApproved)
- 1× `MonthlyIncomes` — R18,000 gross / R13,500 net (`GrossIncome`, `NetIncome`)
- 1× `MonthlyExpenses` — zeroed (applicant qualifies)
- 5× `PLMApplicationHistortyLogs` — capture ? fee upload ? fee validated ? approved

**No `TenantTrainings` record is pre-inserted** — the system creates it when the customer
clicks **Schedule Training**, fills in date/time/venue and confirms the SweetAlert prompt.

**Expected outcome after scheduling via UI:**
1. A `TenantTrainings` row is inserted with `InvitationToken`, `InvitationSentDate`, `TokenExpiryDate`
2. Application status updates to **3323** (`s_awaiting_online_training`)
3. Email/SMS notification sent to tenant

**Key table columns written on schedule (verify in DB):**

| Column | Expected value |
|--------|---------------|
| `PropertyLeaseApplicationId` | `@AppId` |
| `InvitationToken` | New GUID string |
| `InvitationSentDate` | `GETDATE()` |
| `TokenExpiryDate` | `GETDATE() + 7 days` |
| `CurrentSlideNumber` | `0` |
| `IsTrainingCompleted` | `0` |
| `IsExamPassed` | `0` |
| `ExamAttempts` | `0` |
| `TrainingStartedDate` | `NULL` |
| `IsActive` | `1` |
| `IsDeleted` | `0` |

**Verify after scheduling:**
```sql
SELECT tt.*, pla.ApplicationReferenceNumber, s.Name AS AppStatus
FROM TenantTrainings tt
JOIN PropertyLeaseApplications pla ON pla.Id = tt.PropertyLeaseApplicationId
JOIN Status s ON s.Id = pla.StatusId
WHERE tt.PropertyLeaseApplicationId = <AppId from seed output>
```

### ?? TC-03 — Schedule Inspection Slots
*(Coming soon — starts from vetted status, assigns to Letting Officer)*

### ?? TC-04 — Unit Inspection
*(Coming soon — starts from inspection slot confirmed)*

### ?? TC-05 — Generate Lease Agreement
*(Coming soon — starts from inspection passed)*

### ?? TC-06 — Lease Agreement Validation
*(Coming soon — Revenue Manager + Property Manager sign-off)*

### ?? TC-07 — Debit Order Registration & Validation
*(Coming soon)*

### ?? TC-08 — Termination Flow
*(Coming soon — serves notice through to vacating confirmation)*

### ?? TC-09 — Deposit Refund Flow
*(Coming soon)*

---

## Key IDs (live DB — CRMPLMDEV_2025)

| Name | Table | Id |
|------|-------|----|
| AshKay (Ashraf Kader) | `Customers` | 190 |
| Sashen Moodley | `Customers` | 45 |
| Airport Park | `PreferredComplexAreas` | 24 |
| Airport Park - Staff Unit | `PreferredComplexAreas` | 34 |
| Risk Assessment | `ResponsibilityTypes` | 11 |
| Invite To Client Training | `ResponsibilityTypes` | 12 |
| Awaiting Risk Assessment | `Status` | 137 |
| Payment Validated and Approved (`s_rcs_assessment_payment_approved`) | `Status` | 122 |
| Awaiting Online Training (`s_awaiting_online_training`) | `Status` | 3323 |
| Training In Progress (`s_training_in_progress`) | `Status` | 3324 |
| Training Completed (`s_training_completed`) | `Status` | 3325 |
| Examination Passed (`s_examination_passed`) | `Status` | 3326 |
| Examination Failed (`s_examination_failed`) | `Status` | 3327 |
| Submitted (RRQ status) | `Status` | 99 |
| Document Pending | `Status` | 46 |
| Individual Application | `PurchaserTypes` | 1 |

## AppSettings keys (officer fallbacks)

| Key | Role |
|-----|------|
| `u_letting_officer` | Letting Officer fallback Customer ID |
| `r_revenue_officer` | Revenue Officer Customer ID |
| `r_revenue_manager` | Revenue Manager Customer ID |
| `r_property_manager` | Property Manager Customer ID |
| `r_community_development_officer` | Community Development Officer Customer ID |
