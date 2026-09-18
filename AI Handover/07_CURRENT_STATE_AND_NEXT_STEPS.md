# Current State & Next Steps

This document outlines where development on the Real Estate Development (RED) branch was suspended, what features are completed, what is pending, and known items requiring attention.

---

## 1. Completed Implementations (Up to RE_UC025)

The following components have been fully built, integrated, and verified to build successfully without compiler warnings or errors:

- **Database Schemas & Data Seeds:** All `RE_*` tables, indexes, and FK relationships are established. Master data categories, facilities, and units are seeded.
- **Client Capture Form (RE_UC005):** Multi-part glassmorphic wizard completed. Supports dynamic cascading dropdowns, unit grid selection stacking, estimated rental auto-aggregation, and 12-slot document upload with file resets.
- **Cascading Lookup Endpoints:** Ajax routes (`GetFacilitiesByCCC`, `GetUnitsByFacility`) implemented in `RealEstateController`.
- **Payment Verification Queue (RE_UC006):** Side-by-side proof of payment PDF viewer card and verification action modal completed.
- **Vetting & Risk Checklists (RE_UC007):** Risk assessment fields and document upload slots completed.
- **Circulation & Comments Queue (RE_UC009):** Parallel routing dashboard and comments forms completed.
- **Committee Adjudication Dashboard (RE_UC011):** Committee decision capture card and meeting minutes file upload completed.
- **HOD Final Sign-off (RE_UC012):** HTML5 signature drawing canvas and decision capture modals completed.
- **Inspection Scheduling & Logs (RE_UC013-14):** Calendars, booking slot confirmations, condition checklists, and ad-hoc maintenance works order logs completed.
- **Maintenance Works Orders (RE_UC15-17):** Prioritization dashboards, technician routing, technician job-sheet uploads, and manager closure workflows completed.
- **Permission to Occupy (RE_UC18-20):** Client request forms, officer vetting checks, HOD signature canvas controls, and 12-month letter generation completed.
- **Master Data Admin Console:** Dashboard containing KPI metrics counters, unit letting progress bars, and modal-based CRUD configurations completed.

---

## 1a. E2E Audit Findings (2026-09-02)

A full run of all 11 specs in `Tests/Playwright/RealEstate/` against a locally running instance found **the app itself works correctly** when driven manually, but **10 of 11 specs fail** — not because of app bugs, but because most specs were written against an older sidebar navigation structure. Manually walking through the flow (`RealEstateCustomer` login → sidebar → Applications → Lease Application Screen) confirms the actual capture form (RE_UC005) loads and renders correctly.

**Root cause 1 (8 of 10 failures) — stale nav selectors, not an app defect:**
Specs click `ul.nav-list a:has-text("Real Estate Applications")` then `a:has-text("Capture")`. The actual current sidebar labels are **"APPLICATIONS"** (parent) → **"Lease Application Screen"** (child) — not "Real Estate Applications" / "Capture". The AI Handover use-case doc's `Navigation:` fields (e.g. "Real Estate Applications → Submit Application for Lease") are *also* stale against this same nav restructuring and should be corrected to match the actual UI labels once confirmed with the team. Affected specs: `Application_Submission`, `Application_Submission_Slow`, `Application_Other_Uploads`, `BackOffice`, `Full_EndToEnd`, `RoleBased_E2E`, plus two text-assertion mismatches (`Login_Flow` expects "Real Estate Development Dashboard", actual heading is "Real Estate Application Inbox").
**Fix required:** update the spec files' locators/expected text to match current labels — no app code changes needed. Not yet done (out of scope for this pass — code changes weren't requested).

**Root cause 2 (1 failure) — likely stale test credentials, needs verification:**
`Admin_Walkthrough.spec.js` logs in as `BOSystemAdminstrator` / `Arsenal5@`. Manually reproducing this in-browser returns **"Error! Invalid username/email or password."** — a real login failure, not a selector issue. This contradicts `08_TEST_USERS_AND_CREDENTIALS.md`, which lists this account as valid. No seed script in this repo creates this user, so its current state can't be verified from the codebase alone — needs checking directly against the dev database, or re-seeding. Low priority since this account is explicitly flagged "do NOT use for RE workflows" anyway.

**Root cause 3 (1 failure) — needs product/business input, not a bug fix:**
`Full_Registration_Submission.spec.js` and `Negative_Flows.spec.js` expect a newly-registered user with an incomplete profile to be redirected to `/Profile/Index3`; instead they land back on `/Account/Login`. This may reflect an intentional workflow change (new users no longer routed to forced profile-completion) rather than a defect — needs a decision, not just a test fix.

**Confirmed passing:** `UC_RealEstate_NewUseCases_E2E.spec.js` (UC21–UC25: PTO drafting, HOD sign-off, PTO revocation, lease conclusion with tenant/HOD signatures, unit allocation, lease classification) passed cleanly end-to-end — the deepest, most complex part of the workflow is verified working.

**Net assessment:** the previous "Playwright tests are configured and verified" claim above was not accurate as of this audit — the suite runs but most specs need their selectors updated to match the current nav structure before they're a reliable regression signal again.

---

## 1b. Full E2E Video Effort — Stopped Here (2026-09-18)

Goal: one continuous video covering RE_UC05–UC20 via `Tests/Playwright/RealEstate/UC_RealEstate_Full_EndToEnd.spec.js`. Fixes already applied to that spec (test-only, no app code changed):
- Sidebar locators now use `ul.nav-list > li > a` filtered by "APPLICATIONS" (top-level only) then "Lease Application Screen".
- Back-office login changed from `BOSystemAdminstrator` (invalid credentials in dev DB) to `re_hod` (redirects to `/AreaManager/AdminInbox`, has access to all RE admin pages).
- Viewport/video reduced from 4K to 1080p — 4K encoding starved the CPU and pushed the slow login past the 90s action timeout.
- Facility changed to "Tokoza Traders Market" (the Tokoza CCC's other facility, "Fannie Malape", has 0 active units left after earlier runs).
- Added the missing `#btnAddUnit` click; without it submission fails with "Selection Required".

**Result of last run:** passed UC05 through UC11 (submit, payment, risk, circulation, comments, consolidation, committee). **Failed at STEP 15 (UC12, HOD final authorisation):** after clicking `#btn-submit-auth`, the `#confirmModal` "Yes, Finalize" button never became visible. Was mid-diagnosis: suspected the synthetic signature drawing on `#signatureCanvas` isn't registering, so the page silently refuses to open the modal. Test application id 71 (`DPRE-20260917-00001`) is left waiting at HOD authorisation in the dev DB.

**Findings to know about (not fixed, app code untouched):**
- `AccountController.Login` (legacy EHC code) is slow: ~8–25s per login. It runs an unused `context.SystemUsers.ToList()` (6,147 rows) on every login, and doubles a per-IP delay (1s, 2s, 4s...) in in-memory Application state after each failed login. Both hurt automated testing.
- Every test run consumes letting units (sets `IsActive=0`). Re-runs need fresh units or a data reset.
- Running the app locally: IIS Express with `.vs/C8.eServices/config/applicationhost.config`, site `C8.eServices.Mvc`, port 3450 (see `.claude/launch.json`). Start it via PowerShell, not Git Bash (path mangling).

**Next steps:** finish diagnosing the STEP 15 signature/modal issue, then keep running until UC12–UC20 pass; UC21–25 already pass via `UC_RealEstate_NewUseCases_E2E.spec.js`. The other 9 specs still need the same nav-label fix.

---

## 2. Uncommitted Development Changes

The files listed in `git status` represent the final active development state. They contain:
1. All custom `RE_` C# Models and DB audit schema mapping setups.
2. The custom `RealEstateController.cs` and `RealEstateAdminController.cs` containing all lifecycle action endpoints.
3. The scoped Onyx Obsidian styled UI views inside `Views/RealEstate` and `Views/RealEstateAdmin`.
4. Playwright spec suites under `Tests/Playwright/RealEstate`.

> **IMPORTANT:** DO NOT discard or reset these uncommitted changes, as they represent the complete implementation of the Real Estate branch.

---

## 3. Stopped Position & Next Steps

Work stopped during the validation of **Video 3: Customer Dynamic Lease Application Submission (4K UHD)**. 

To resume work and finish the module, follow these steps:

### Step 1: Complete E2E Video 3 Capture
1. Open PowerShell and run the Playwright test:
   ```powershell
   cd "c:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Tests\Playwright"
   npx playwright test RealEstate/UC_RealEstate_Application_Submission_Slow.spec.js --headed
   ```
2. Copy the resulting `video.webm` from `test-results/` to:
   `C:\Real estate use cases\monday demo 4k\Video3_Dynamic_Application_Process_and_Calculations_4K.webm`

### Step 2: Implement Agreement Document Template Mapping
- Standardize the PDF assembly inside `RealEstateUserAgreementHelper.cs` to map fields from `RE_Applications` into:
  - `PDFTemplates/RealEstate/Updated User Agreement.doc` (12-Month PTO Certificate)
  - `PDFTemplates/RealEstate/URC User Agreement_Latest.doc` (36-Month Lease Agreement)
- Wire up the download actions in `RealEstateAdminController`:
  - `DownloadPtoCertificatePdf`
  - `DownloadLeaseAgreementPdf`

### Step 3: Eviction & Termination Integration
- Map the voluntary lease termination date validations (Tenant served notices) to ensure dates are ≥ 1 calendar month in the future and fall on the last day of the month (BR29/BR30).
- Integrate the HOD signature authorization screens to bypass the legacy eviction committee (UC24) and handle former tenant deposits.
- Run the Playwright test for terminations to verify correctness:
  ```powershell
  npx playwright test Tests/Playwright/UC023_UC025_Termination_Eviction_Workflow.spec.js --headed
  ```
