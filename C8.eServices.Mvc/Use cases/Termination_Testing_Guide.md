# Termination Flow — Testing Guide
**Location:** `Use cases/Termination_Testing_Guide.md`  
**Last Updated:** 2026-05-22

---

## ✅ Confirmed Test Reference — sash38 (Ready to Use)

> This lease has been seeded in the `CRMPLMDEV_2025` database and is ready for end-to-end termination testing.

| Field | Value |
|---|---|
| **Tenant Login** | `sash38` |
| **Tenant Name** | Sashen Moodley |
| **Application Ref** | `EHC2022072000004` |
| **Lease Ref** | `EHC2022072000004` |
| **LeaseDetails ID** | `24` |
| **Application ID** | `28` |
| **Current Status** | `Awaiting Termination Appraisal` ← start here |
| **Termination Reason** | Termination Due To End of Lease Term (60th Month) |
| **Termination Date** | 30/06/2026 |
| **Revenue Officer** | `COESolarDev09` (Dimakatso Legoabe) |

### Where Each Role Starts Testing

| Role | Login | First Step |
|---|---|---|
| Revenue Officer | `COESolarDev09` | *Termination → Terminations* → "Authorize Termination" button |
| Client Services Officer | any CSO account | After RM approves → *Terminations* → "Serve Eviction Notice" 🟡 |
| CSO (proof) | same CSO | After notice served → *Terminations* → "Capture Proof of Service" 🔵 |
| Housing Supervisor | supervisor account | After proof captured → *Terminations* → "Conduct Exit Inspection" |

### Skip-to SQL (if you want to jump to a specific step)

```sql
-- Jump to Step 5: Serve Eviction Notice
UPDATE LeaseDetails SET StatusId = 4387 WHERE Id = 24  -- s_awaiting_eviction_service

-- Jump to Step 6: Capture Proof of Service
UPDATE LeaseDetails SET StatusId = 4388 WHERE Id = 24  -- s_eviction_notice_served

-- Jump to Step 7: Exit Inspection
UPDATE LeaseDetails SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_exit_inspec') WHERE Id = 24

-- Reset to Step 3 (RM Authorization)
UPDATE LeaseDetails SET StatusId = 4375 WHERE Id = 24  -- s_awaiting_termination_appraisal
```

---

## How the System Detects Leases in Notice Period

There is **no background scheduler**. The check fires **on page load** when the CSO opens *Termination → Terminate*.

The controller (`PropertyLeaseApplicationController.Termination()` GET) runs this every time:

```csharp
// Fetches ALL active leases and checks two conditions:
bool isNoticePassed = lease.TerminationNotice != null && lease.TerminationNotice <= today;
var transgression   = activeTransgressions
    .FirstOrDefault(t => t.TenancyReferenceNumber == lease.LeaseReferenceNo);

// Lease gets FLAGGED if either is true → shown in warning panel
if (isNoticePassed || transgression != null)
    flaggedLeases.Add(new FlaggedLeaseViewModel { Lease = lease, FlaggedReason = reason });
```

**`TerminationNotice`** = `LeaseDetails.TerminationNotice` — set to `EndDate minus 3 months` when the lease is created or renewed. When today > that date, the lease surfaces.

**This does NOT change any status automatically.** It only surfaces leases as a visual flag for the CSO to act on.

---

## Two Entry Paths into the Termination Flow

```
PATH A: Tenant serves notice themselves
PATH B: CSO initiates directly (e.g. non-payment, end of term, flagged lease)
```

Both paths converge at Step 3 (Revenue Manager Authorization).

---

## Step-by-Step Testing Guide

---

### PATH A — Tenant-Initiated Termination

---

#### STEP 1 — Tenant Serves Notice (UC023-S1)

| | |
|---|---|
| **Who** | Tenant |
| **Login as** | Tenant account |
| **Menu** | *Notice → Serve Notice* |
| **URL** | `~/PropertyLeaseApplication/ServeNotice` |

**What the page shows:**  
All active leases belonging to this tenant where `NoticeDate == null` (i.e. not yet notified).

**What to do:**  
1. Enter the application reference number (e.g. `PLM/EHC-XXXX`) into the search box
2. Click Search → redirected to the `ApplicationLeaseServeNotice` form
3. Select a termination reason from the dropdown
4. Enter a termination date — **must pass two rules:**
   - **BR29:** Date must be ≥ 1 calendar month from today
   - **BR30:** Date must be the **last day** of a month (e.g. 31 May, 30 June)
5. Click Submit → SweetAlert confirm → Submit

**What happens:**
- `PropertyLeaseActionComments` row saved (reason)
- `LeaseDetails.NoticeDate` set
- `PropertyLeaseApplication.ServeNoticeDate` set
- **Status → `AwaitingCSOTerminationReview`**
- RoundRobin job created (Terminations → CSO)
- Email notification sent

**Redirect:** Back to `ServeNotice` list with session message confirmation.

---

#### STEP 2 — CSO Reviews the Notice (UC023-S2a)

| | |
|---|---|
| **Who** | Client Services Officer |
| **Login as** | CSO account |
| **Menu** | *Termination → Termination Reviews* |
| **URL** | `~/PropertyLeaseApplication/TerminationCSOReview` |

**What the page shows:**  
All leases in status `AwaitingCSOTerminationReview`.

**What to do:**  
1. Click the lease row → goes to `TerminationCSOReviewDetails`
2. Review the tenant details and notice date
3. Select decision from dropdown:
   - **Supported** → enter Official Number → Submit
   - **Not Supported** → enter reason → Submit

**Outcomes:**

| Decision | Status Set | Next Step |
|----------|-----------|-----------|
| Supported | `AwaitingTerminationAppraisal` | RM Authorization (Step 3) |
| Not Supported | `TerminationNotSupported` | **Flow ends** — tenant notified |

---

### PATH B — CSO-Initiated Termination (Flagged Lease / Non-Payment / End of Term)

---

#### STEP 1 — CSO Searches for Lease (UC023-S2b)

| | |
|---|---|
| **Who** | Client Services Officer |
| **Login as** | CSO account |
| **Menu** | *Termination → Terminate* |
| **URL** | `~/PropertyLeaseApplication/Termination` |

**What the page shows on load:**  
- A **Flagged Leases panel** — active leases where either:
  - `TerminationNotice` date has passed, OR
  - An active Payment Transgression is linked to the tenant
- A search box to look up any lease

**What to do:**  
1. Enter a lease ref (`PLM-XXXXX`) or application ref (`PLM/EHC-XXXX`) into search → Submit
2. Validates: Not inactive, not already issued a termination date
3. Redirected to `LeaseTerminationValidation` form (in `LeaseDetailsController`)

**On the LeaseTerminationValidation form:**
1. Select termination reason:
   - `TenantNotice` / `LeaseNotRenuewed` / `TenantDeceased` / `EndOfLeasePeriod60M`
2. Set effective date
3. Submit → saves `LeaseTermination` record

**Outcome:**
- **Status → `AwaitingTerminationAppraisal`**
- RoundRobin → Revenue Manager

---

### SHARED PATH — Steps 3 Onwards (Both Paths Converge Here)

---

#### STEP 3 — Revenue Manager Authorization (UC023-S3)

| | |
|---|---|
| **Who** | Revenue Officer |
| **Login as** | Revenue Officer account |
| **Menu** | *Termination → Terminations* |
| **URL** | `~/PropertyLeaseApplication/PropertyLeaseApplicationTerminations` |

**What the page shows:**  
Leases assigned to this RM via RoundRobin where status is `AwaitingTerminationAppraisal`.

**Action button shown:** `"Authorize Termination"` (green) or `"Property Eviction"` (blue)

**What to do:**
1. Click the action button → opens `PropertyEvictionValidation` form
2. Review the lease and termination details
3. Select from dropdown: **Approve / Reject / Refer to Legal**

**Outcomes:**

| Decision | Status Set | Next |
|----------|-----------|------|
| **Approve** | `AwaitingEvictionService` | Step 5 (Serve Notice) |
| **Reject** | `AwaitingterminantionApproval` | Back to CSO |
| **Refer to Legal** | `LegalReferralPending` | Step 4 (CEO) |

---

#### STEP 4 — CEO Authorization (UC024-S2) — Legal Referral Path Only

| | |
|---|---|
| **Who** | Director / CEO |
| **Login as** | Director account |
| **Menu** | *Termination → Terminations* |
| **URL** | `~/PropertyLeaseApplication/PropertyLeaseApplicationTerminations` |

**Action button shown:** 🔴 `"CEO Authorization"` — appears when status = `LegalReferralPending` or `AwaitingEvictionCEOAuth`

**What to do:**
1. Click `"CEO Authorization"` → opens `EvictionCEOAuthorization` form
2. Review eviction reference and termination details
3. Select: **Approve / Reject**

**Outcomes:**

| Decision | Status Set | Next |
|----------|-----------|------|
| **Approve** | `AwaitingEvictionService` | Step 5 (Serve Notice) |
| **Reject** | `AwaitingterminantionApproval` | Back to CSO |

---

#### STEP 5 — Serve Eviction Notice (UC025-S1) ← NEW

| | |
|---|---|
| **Who** | Client Services Officer |
| **Login as** | CSO account |
| **Menu** | *Termination → Terminations* |
| **URL** | `~/PropertyLeaseApplication/PropertyLeaseApplicationTerminations` |

**Action button shown:** 🟡 `"Serve Eviction Notice"` — appears when status = `AwaitingEvictionService`

**What to do:**
1. Click the yellow button → opens `ServeEvictionNotice` form
2. Select service method:
   - Sheriff of the Court
   - Hand Delivery
   - Registered Post
   - Email Service
3. Enter service date
4. Enter official/case number (optional)
5. Click `"Serve Notice"` → SweetAlert confirm → Submit

**Outcome:**
- `EvictionServiceRecord` created
- **Status → `EvictionNoticeServed`**
- Activity tracker logged

---

#### STEP 6 — Capture Proof of Service (UC025-S2) ← NEW

| | |
|---|---|
| **Who** | Client Services Officer |
| **Login as** | CSO account |
| **Menu** | *Termination → Terminations* |

**Action button shown:** 🔵 `"Capture Proof of Service"` — appears when status = `EvictionNoticeServed`

**What to do:**
1. Click the blue button → opens `CaptureProofOfService` form
2. Select proof type:
   - Sheriff return of service
   - Signed acknowledgement
   - Registered mail tracking
   - Email read receipt
   - Failed attempt affidavit
3. Enter proof date
4. Enter comments
5. Submit

**Outcome:**
- `EvictionServiceRecord` updated with proof details
- **Status → `AwaitingExitInspection`**

---

#### STEP 7 — Conduct Exit Inspection (UC-32)

| | |
|---|---|
| **Who** | Housing Supervisor |
| **Login as** | Housing Supervisor account |
| **Menu** | *Termination → Terminations* |

**Action button shown:** `"Conduct Exit Inspection"` — appears when status = `AwaitingExitInspection`

**Outcomes:**

| Outcome | Status Set | Next |
|---------|-----------|------|
| Habitable | `AwaitingVacatingConfirm` | Step 8 |
| Habitable – Minor Defects | `AwaitingVacatingConfirm` | Step 8 |
| Not Habitable | `AwaitingMaintananceJobSheet` | Maintenance queue |

---

#### STEP 8 — Confirm Tenant Vacated (UC-35)

| | |
|---|---|
| **Who** | Housing Supervisor |
| **Login as** | Housing Supervisor account |
| **Menu** | *Termination → Terminated Leases* |

**Action button shown:** `"Confirm Move-Out"`

**Outcomes:**

| Outcome | Status Set | Effect |
|---------|-----------|--------|
| **Vacated** | `ApplicantVacated` | Unit freed (`IsTaken = false`), refund step triggered |
| **Not Vacated** | `ApplicantNotVacated` | Rental continues, no unit release |

---

## Status Transition Summary

```
[PATH A] Tenant Submits Notice
    → AwaitingCSOTerminationReview

[PATH B] CSO Initiates
    → AwaitingTerminationAppraisal (skips CSO review)

CSO Supports (PATH A only)
    → AwaitingTerminationAppraisal

Revenue Manager — Approve
    → AwaitingEvictionService          ← UC025 entry
Revenue Manager — Reject
    → AwaitingterminantionApproval     ← back to CSO
Revenue Manager — Refer
    → LegalReferralPending

CEO Approve (legal path)
    → AwaitingEvictionService          ← UC025 entry
CEO Reject
    → AwaitingterminantionApproval

Serve Eviction Notice
    → EvictionNoticeServed

Capture Proof of Service
    → AwaitingExitInspection

Exit Inspection — Habitable
    → AwaitingVacatingConfirm
Exit Inspection — Not Habitable
    → AwaitingMaintananceJobSheet

Confirm Vacated
    → ApplicantVacated (unit freed)
    → ApplicantNotVacated
```

---

## Quick SQL — Seed a Test Case

```sql
-- Option 1: Set a specific lease to AwaitingTerminationAppraisal
-- (RM can pick it up immediately)
DECLARE @LeaseId INT = ???
UPDATE LeaseDetails
SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_termination_appraisal')
WHERE Id = @LeaseId

-- Option 2: Skip straight to UC025 (Serve Notice)
UPDATE LeaseDetails
SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_awaiting_eviction_service')
WHERE Id = @LeaseId

-- Option 3: Skip to Proof of Service
UPDATE LeaseDetails
SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_eviction_notice_served')
WHERE Id = @LeaseId
```

---

## Checklist — No-Break Verification

- [ ] Each status shows exactly one action button on the dashboard
- [ ] No button leads to a blank page or exception
- [ ] Every POST redirects back to `PropertyLeaseApplicationTerminations`
- [ ] Status changes after each step (verify in SQL or check the row)
- [ ] Session message modal fires on redirect back to dashboard
- [ ] SweetAlert confirmation fires before every submit
- [ ] "View" links open application/lease details in a new tab
- [ ] `EvictionServiceRecord` row created after Step 5
- [ ] `EvictionServiceRecord` updated (ProofCaptured=true) after Step 6
