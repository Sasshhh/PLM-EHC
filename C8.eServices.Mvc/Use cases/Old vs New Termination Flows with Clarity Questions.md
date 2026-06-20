# Old vs New Termination Flows — With Clarity Questions

> **Purpose:** Single reference doc comparing the current (AS-IS) termination system against the proposed new use cases (UC023–UC026). All open questions that must be answered before coding are listed at the end.
>
> **Last updated:** 2026-05-18

---

## Table of Contents

1. [AS-IS System Baseline (Current Code)](#1-as-is-system-baseline)
2. [New Use Cases Summary (UC023–UC026)](#2-new-use-cases-summary)
3. [How Old UCs Map to New UCs](#3-mapping-old-to-new)
4. [Flow Diagrams — Old vs New](#4-flow-diagrams)
5. [Detailed Gap Analysis Per UC](#5-detailed-gap-analysis)
6. [What Needs Building — Prioritised](#6-what-needs-building)
7. [Open Questions Requiring Clarity](#7-open-questions)

---

## 1. AS-IS System Baseline

> What the code does TODAY, traced from controllers.

### UC-29: Tenant Serve Notice

- **Actor:** Tenant
- **Controller:** `PropertyLeaseApplicationController.ApplicationLeaseServeNotice`
- **View:** `ApplicationLeaseServeNotice.cshtml`
- **POST saves:** `PropertyLeaseActionComments` (reason), `LeaseDetails.NoticeDate`, `PropertyLeaseApplication.ServeNoticeDate`
- **Email:** `ServeNotice` → tenant
- **RoundRobin:** `Terminations` → assigned to Letting Officer
- **No Termination Reference Number generated**
- **No date validation** (minimum 1 month / last day of month not enforced server-side)

### UC-30: LO Captures Termination

- **Actor:** Letting Officer / CSO
- **Controllers:** `PropertyLeaseApplicationController.Termination` (search) → `LeaseDetailsController.LeaseTerminationValidation` (form)
- **POST saves:** New `LeaseTermination` row (date, reason, status=`AwaitingterminantionApproval`)
- **Status set per branch:** `TenantNotice` / `LeaseNotRenewed` / `TenantDeceased` / `EndOfLeaseTerm`
- **RoundRobin:** `TerminationValidation` → assigned to Revenue Officer
- **Does NOT pre-populate from tenant's serve notice data**

### UC-31: Revenue Officer Validates Account

- **Actor:** Revenue Officer
- **Controller:** `PropertyLeaseApplicationController.PropertyEvictionValidation`
- **View:** `PropertyEvictionValidation.cshtml`
- **POST Approved:** Status → `AwaitingCommitteEviction` ← WRONG for voluntary terminations
- **POST Rejected:** Status → `AwaitingCommitteEviction` ← BUG (same as approved)
- **No signature block, no Official Number field**
- **No Legal Services referral branch**

### UC-32: Exit Inspection

- **Actor:** Housing Supervisor
- **Controller:** `PropertyLeaseApplicationController.ConductExitInspection`
- **3 branches:** Habitable → `AwaitingVacatingConfirm`; Minor Defects → same; Not Habitable → `AwaitingMaintananceJobSheet` + creates `AllocatedUnitMaintenanceEHC`
- **Not Habitable branch is NOT in the new UCs but exists in code and should stay**

### UC-33: LO Captures Eviction Details

- **Actor:** Letting Officer
- **Controller:** `PropertyLeaseApplicationController.CaptureEvictionDetails`
- **4 reason types:** NonPayment, NonCompliance, Subletting, IllegalActivities
- **RoundRobin:** `CommitteeOutcomes` → assigned to LO
- **Email:** `EvictionCapture` → tenant

### UC-34: LO Captures Eviction Committee Outcome

- **Actor:** Letting Officer
- **Controller:** `PropertyLeaseApplicationController.EvictionCommitteeOutcome`
- **Saves:** `CommitteeOutcome` row + `PropertyLeaseActionComments`
- **Approved:** `LeaseDetails.EndDate = TerminationDate`, `LeaseTermination.StatusId = TerminationDateIssued`, status → `AwaitingVacatingConfirm`
- **Rejected:** `LeaseTermination.StatusId = TerminationRejected`, status → `TerminationRejected`

### UC-35: Housing Supervisor Confirms Vacated

- **Actor:** Housing Supervisor
- **Controller:** `PropertyLeaseApplicationController.ConfirmVacatingAppicant`
- **Vacated:** Status → `ApplicantVacated`, `ApplicationAllocatedProperty.IsTaken = false`, `EHCRoundRobin(AcknowlegeRefund: true)`
- **Not Vacated:** Status → `ApplicantNotVacated`, rental continues

---

## 2. New Use Cases Summary

### UC023 — Manage Lease Termination (replaces UC-29, UC-30, UC-31)

**Scenario 1 — Tenant Initiates:**
- Tenant submits notice with date + reason + proof of banking
- System **generates Termination Reference Number** (NEW)
- Email notification to **CSO** (not LO)
- Date validated: ≥ 1 calendar month, last day of month

**Scenario 2a — CSO Reviews Tenant Notice (NEW STEP):**
- CSO sees list of tenant-initiated termination notices
- Options: Supported → `Awaiting Termination Appraisal` / Not Supported → notify tenant with reason, STOP
- Captures: Official number, authorization action, reason

**Scenario 2b — CSO Initiates Termination Directly:**
- CSO goes to `Tenants → Lease Agreements → flagged for termination`
- Captures: Reason, Effective Date, Clause reference, Notice period, Official Number
- Uploads docs where applicable (e.g., transgression evidence)
- Status → `Awaiting Termination Appraisal`

**Scenario 3 — Revenue Manager Authorization:**
- Reviews and validates, clicks Account Validation to see bank info
- **Approved:** Official number + signature + reason → `Awaiting Exit Inspection`, notify Tenant + CSO
- **Rejected:** Reason (mandatory) → `Awaiting Termination Approval` (back to CSO), notify Tenant + CSO
- **Referral (transgression alternate flow):** Capture Legal Services details → **generates Eviction Reference Number** → notify Tenant, CSO, Legal, CEO → `Awaiting Termination Approval` → feeds into UC024

### UC024 — Capture Eviction Outcome & Generate Eviction Notice (replaces UC-33, UC-34)

**Scenario 1 — CSO Captures Eviction Outcome:**
- Selects from eviction reference list
- Captures: Outcome Type, Date, Summary, Official Number, supporting docs
- **Generates Eviction Notice from approved template** (NEW)
- Notification to CEO → `Awaiting Exit Inspection`

**Scenario 2 — CEO Authorization (replaces old Eviction Committee):**
- CEO reviews eviction, enters Official number + authorize/reject + signature
- **Approved:** Notify Tenant + CSO + Legal → `Awaiting Exit Inspection`
- **Rejected:** Reason mandatory → notify CSO → `Awaiting Termination Approval`

### UC025 — Serve Eviction Notice & Capture Proof of Service (BRAND NEW)

**Scenario 1 — Serve Notice:**
- CSO selects service method: Sheriff of Court, Hand delivery, Registered post, Email
- Records date/time stamp → `Awaiting Exit Inspection`

**Scenario 2 — Capture Proof of Service:**
- CSO captures proof: Sheriff return, Signed ack, Registered mail tracking, Email delivery confirmation, Failed attempt
- Mandatory: Date/time, comments, evidence uploads
- → `Awaiting Exit Inspection`

### UC026 — Manage Disputes (BRAND NEW MODULE — 3 sub-UCs)

**UC26A — Capture Lease Dispute:** Tenant or CSO logs dispute with category (Financial, Compliance, Maintenance, Tenure, Notices, Administrative), reference number `EHC_DISP_###_YYYY`, Status = `Open, Awaiting Review`

**UC26B — Review Lease Dispute:** Revenue Manager classifies risk (Low/Medium/High/Other). High risk → Legal Services referral. Other (insufficient) → notify tenant. Status = `Referred`

**UC26C — Resolve & Close:** Revenue Manager selects resolution type (billing correction, payment plan, maintenance order, notice withdrawal, lease variation, dismissal, litigation). CEO authorizes closure. Approved → `Closed` (lease may go to `Awaiting Exit Inspection`). Rejected → `Referred` (back to Revenue Manager).

---

## 3. Mapping Old to New

```
OLD                                    NEW
───                                    ───
UC-29  Tenant Serve Notice         →   UC023 Scenario 1 (Tenant initiates)
(nothing)                          →   UC023 Scenario 2a (CSO reviews) ← NEW
UC-30  LO Capture Termination      →   UC023 Scenario 2b (CSO initiates)
UC-31  Revenue Officer Validate    →   UC023 Scenario 3 (Revenue Manager auth)
                                         + Legal Services Referral branch ← NEW
UC-33  Capture Eviction Details    →   UC024 Scenario 1 (CSO captures outcome)
UC-34  Eviction Committee Outcome  →   UC024 Scenario 2 (CEO auth, replaces committee)
(nothing)                          →   UC025 Serve Notice + Proof of Service ← NEW
(nothing)                          →   UC026 Disputes module ← NEW
UC-32  Exit Inspection             →   stays as-is (all paths converge here)
UC-35  Confirm Vacated             →   stays as-is
```

---

## 4. Flow Diagrams

### Path A — Voluntary Termination

```
ENTRY 1: Tenant submits notice (UC023-S1)
    → Generates Termination Ref #
    → Notifies CSO
         │
         ▼
CSO REVIEWS notice (UC023-S2a) ← NEW STEP
    Supported → Awaiting Termination Appraisal
    Not Supported → Notify tenant, STOP
         │ (Supported)
         ▼
ENTRY 2: CSO initiates directly (UC023-S2b)
    → Captures details + docs
    → Awaiting Termination Appraisal
         │
         ▼
REVENUE MANAGER AUTH (UC023-S3)
    Approved → Awaiting Exit Inspection + notify Tenant + CSO
    Rejected → Awaiting Termination Approval (back to CSO)
    Referral → Legal Services → Eviction Ref # → UC024 (Path B)
         │ (Approved)
         ▼
EXIT INSPECTION (UC-32, stays as-is)
         │
         ▼
CONFIRM VACATED (UC-35, stays as-is)
    → Unit freed for re-allocation
```

### Path B — Eviction

```
Revenue Manager refers to Legal Services (UC023-S3 Alt Flow 2)
    → Eviction Ref # generated
    → Notify Tenant, CSO, Legal, CEO
         │
         ▼
CSO CAPTURES EVICTION OUTCOME (UC024-S1)
    → Outcome Type, Date, Summary
    → Generate Eviction Notice (from template)
    → Notify CEO
         │
         ▼
CEO AUTHORIZATION (UC024-S2, replaces old committee)
    Approved → Notify Tenant + CSO + Legal → Awaiting Exit Inspection
    Rejected → Notify CSO → Awaiting Termination Approval
         │ (Approved)
         ▼
SERVE EVICTION NOTICE (UC025-S1) ← NEW
    → Select service method (Sheriff/Hand/Post/Email)
         │
         ▼
CAPTURE PROOF OF SERVICE (UC025-S2) ← NEW
    → Evidence uploads, mandatory comments
         │
         ▼
EXIT INSPECTION (UC-32, stays as-is)
         │
         ▼
CONFIRM VACATED (UC-35, stays as-is)
```

### Path C — Disputes (Parallel / Independent)

```
TENANT OR CSO LOGS DISPUTE (UC26A) ← NEW
    → Ref# EHC_DISP_###_YYYY → Status = Open
         │
         ▼
REVENUE MANAGER REVIEWS (UC26B) ← NEW
    → Classifies risk (Low/Med/High/Other)
    → High → Legal referral; Other → notify tenant
    → Status = Referred
         │
         ▼
REVENUE MANAGER RESOLVES (UC26C-S1) ← NEW
    → Resolution outcome type
    → Status = Resolved (or Not Resolved → Referred)
         │
         ▼
CEO CLOSURE AUTH (UC26C-S2) ← NEW
    Approved → Status = Closed; Lease → Awaiting Exit Inspection
    Rejected → Status = Referred (back to Revenue Manager)
```

---

## 5. Detailed Gap Analysis

### UC023-S1 (Tenant Notice) vs AS-IS UC-29

| Feature | AS-IS | New | Delta |
|---|---|---|---|
| Actor | Tenant | Tenant | Same |
| Date validation (≥1 month, last day) | ❌ Not enforced | ✅ Required | **ADD** |
| Generates Termination Ref # | ❌ No | ✅ Yes | **ADD** |
| Email goes to | LO | **CSO** | **CHANGE** |
| Next step | Goes to LO termination capture | Goes to **CSO review** first | **NEW STEP** |

### UC023-S2a (CSO Reviews Notice) — BRAND NEW

| Feature | What's needed |
|---|---|
| New list view | Tenant-initiated notices for CSO review |
| Actions | Supported / Not Supported |
| Data captured | Official number, action, reason |
| New status needed? | `Awaiting Termination Appraisal` (check if exists) |

### UC023-S2b (CSO Initiates) vs AS-IS UC-30

| Feature | AS-IS | New | Delta |
|---|---|---|---|
| Entry point | Search by ref # | Tenants → Lease Agreements → flagged list | **NEW NAV** |
| Data captured | Date + reason dropdown | Reason, Date, **Clause ref, Notice period** | **EXPANDED** |
| Official Number | ❌ | ✅ | **ADD** |
| Pre-populate from tenant notice | ❌ | N/A (different path) | — |
| Status after | Multiple (per reason) | `Awaiting Termination Appraisal` | **SIMPLIFIED** |

### UC023-S3 (Revenue Manager) vs AS-IS UC-31

| Feature | AS-IS | New | Delta |
|---|---|---|---|
| Actor name | Revenue Officer | Revenue Manager | **Rename** |
| Account Validation button | Implicit | **Explicit UI step** | **ADD** |
| Digital signature | ❌ | ✅ | **ADD** |
| Official Number | ❌ | ✅ | **ADD** |
| Approved → status | `AwaitingCommitteEviction` (WRONG) | `Awaiting Exit Inspection` | **FIX** |
| Rejected → status | `AwaitingCommitteEviction` (BUG) | `Awaiting Termination Approval` | **FIX** |
| Rejected → notifications | None | Notify Tenant + CSO | **ADD** |
| **Referral to Legal** | ❌ Does not exist | ✅ Creates Eviction Ref # | **BRAND NEW** |

### UC024 (Eviction Outcome + CEO Auth) vs AS-IS UC-33 + UC-34

| Feature | AS-IS | New | Delta |
|---|---|---|---|
| Who captures eviction | LO | CSO | **Role change** |
| 4 eviction reason types | Yes (NonPayment etc.) | Generic (Outcome Type) | **SIMPLIFIED** |
| Generate Eviction Notice doc | ❌ | ✅ From template | **ADD** |
| Committee outcome → CEO auth | CommitteeOutcome table, LO captures | **CEO directly authorizes** | **MAJOR CHANGE** |
| Approved → next | `AwaitingVacatingConfirm` | `Awaiting Exit Inspection` | **CHANGED** |
| Rejected → next | `TerminationRejected` | `Awaiting Termination Approval` | **CHANGED** |
| Notifications | Activity tracker only | Tenant + CSO + Legal | **ADD** |

### UC025 (Serve + Proof of Service) — BRAND NEW

| Requirement | Details |
|---|---|
| New DB entities | `EvictionServiceRecord`, `ProofOfService` |
| New views | Serve notice form, Proof of service form |
| Service methods | Sheriff, Hand delivery, Registered post, Email |
| Proof types | Sheriff return, Signed ack, Tracking, Email confirm, Failed attempt |

### UC026 (Disputes) — BRAND NEW MODULE

| Requirement | Details |
|---|---|
| New DB entities | `LeaseDispute`, `DisputeResolution`, `DisputeClosure` |
| Ref # format | `EHC_DISP_###_YYYY` |
| 6 dispute categories | Financial, Compliance, Maintenance, Tenure, Notices, Administrative |
| Risk classification | Low, Medium, High, Other |
| 7 resolution outcome types | Billing correction, payment plan, maintenance order, notice withdrawal, lease variation, dismissal, litigation |
| New statuses | Open, Awaiting Review, Referred, Resolved, Closed |
| Legal Services referral | Shared entity with UC023/UC024 |

---

## 6. What Needs Building — Prioritised

### Phase 1: Core Termination/Eviction Fixes (UC023 + UC024)

| # | Item | Effort | Type |
|---|---|---|---|
| 1 | Fix UC-31 Approved: status → `AwaitingExitInspection` | Small | Bug fix |
| 2 | Fix UC-31 Rejected: retain status, don't progress | Small | Bug fix |
| 3 | Re-wire notifications from LO to CSO | Small | Config |
| 4 | Generate Termination Reference Number | Small | Feature |
| 5 | Add date validation (≥1 month, last day of month) | Small | Feature |
| 6 | Add CSO review step (UC023-S2a) | Medium | New step |
| 7 | Add CSO direct-initiate path (UC023-S2b) | Medium | New path |
| 8 | Add Official Number + Signature to Revenue Manager view | Medium | UI |
| 9 | Add Revenue Manager Referral → Legal Services branch | Medium | New branch |
| 10 | Replace eviction committee with CEO authorization | Medium | Refactor |

### Phase 2: New Modules (UC025 + UC026) — Defer

| # | Item | Effort |
|---|---|---|
| 11 | UC025: Serve Eviction Notice + Proof of Service | Large |
| 12 | UC026: Full Disputes module (26A + 26B + 26C) | Very Large |
| 13 | Legal Services agency registry (shared) | Medium |

---

## 7. Open Questions Requiring Clarity

> ⚠️ **These must be answered before any implementation begins.**

### Q1 — Scope

Do we implement UC023 + UC024 now (Phase 1) and defer UC025 + UC026 to a later phase? UC025/026 are full new modules requiring new DB tables, views, controllers.

**Recommendation:** Phase 1 first.

### Q2 — Termination Reference Number Format

What format? E.g., `EHC_TERM_###_YYYY`? Auto-increment from DB sequence? Or follow existing ref # patterns in the system?

### Q3 — Eviction Reference Number Format

Same question. E.g., `EHC_EVIC_###_YYYY`?

### Q4 — CSO Review Step (UC023-S2a)

When a tenant submits notice, does the CSO HAVE to review and approve it before it progresses to Revenue Manager? Is this a hard gate (blocks until CSO acts) or a soft review (auto-progresses if CSO doesn't act within X days)?

### Q5 — CEO Replaces Eviction Committee?

The old system had the LO capture a committee decision (`CommitteeOutcome` table). The new UC has the CEO directly authorize. Does the committee still exist as a real-world process where the CEO just formalises their decision in the system? Or is the committee concept gone entirely? This determines whether we keep or deprecate the `CommitteeOutcome` entity.

### Q6 — Legal Services Entity

For the Revenue Manager's referral-to-legal branch (UC023-S3 Alt Flow 2): do we build a full legal services agency registry (dropdown of agencies with name, address, contact, email), or just free-text fields for now?

**Recommendation:** Free-text fields first, registry later.

### Q7 — Digital Signatures

Is this the same signature mechanism already used in the system (e.g., lease agreements), or does it need a new implementation?

**Recommendation:** Reuse existing signature mechanism if one exists.

### Q8 — "Agreements Flagged for Termination"

In UC023-S2b, the CSO navigates to `Tenants → Lease Agreements` and sees a list "flagged for termination." What flags a lease for termination?
- 60-month limit approaching?
- Renewal not accepted / expired?
- Complaints / transgressions?
- All of the above?
- Or does the CSO just manually search by ref # (like today)?

### Q9 — Existing Code to Keep

The following features are in the current system but NOT mentioned in the new UCs. Should they stay?
- **Not Habitable branch** in exit inspection (creates maintenance job sheet)
- **Deposit refund step** after vacating (`AcknowlegeRefund` RoundRobin)
- **4 specific eviction reason types** (NonPayment, NonCompliance, Subletting, IllegalActivities)
- **TenantDeceased** as a termination reason

**Recommendation:** Keep all of these.

### Q10 — Status Name Alignment

The new UCs use status names that may not exist in the DB yet:
- `Awaiting Termination Appraisal` — new?
- `Awaiting Termination Approval` — same as existing `AwaitingterminantionApproval`?
- `Awaiting Exit Inspection` — exists as `AwaitingExitInspection` ✅

Need to confirm which statuses exist and which need to be created.

---

> **Next step:** Once the questions above are answered, an implementation plan will be created in the `Unified_Use_Cases.md` with exact code changes, SQL scripts, and verification steps.
