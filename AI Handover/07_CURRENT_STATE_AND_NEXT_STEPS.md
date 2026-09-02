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
- **Playwright Test Suite:** Playwright tests are configured and verified:
  - `UC_RealEstate_Application_Submission.spec.js`
  - `UC_RealEstate_Application_Submission_Slow.spec.js` (Slow-motion capture spec)
  - `UC_RealEstate_RoleBased_E2E.spec.js`
  - `UC_RealEstate_Full_EndToEnd.spec.js`

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
