# Real Estate Development (DPRE/RED) System Work Log

*This log serves as a running record of all work and updates applied to the Real Estate module. Mark items off chronologically as they are completed.*

---

## [2026-07-12] - Database Schema Extensions

### Status: [x] Completed

**Changes:**
1. **Complete Database Setup Script:** Modified [RE_PropertyLeaseManagementRealEstate_CompleteSetup.sql](file:///C:/REPO/PLM%20V1/PLM-EHC/DatabaseScripts/RE_PropertyLeaseManagementRealEstate_CompleteSetup.sql) to add `SelectedUnitsJson NVARCHAR(MAX) NULL` column definition.
2. **Applications & Audits Schema Migration:** Implemented DDL schema migration checks using conditional `ALTER TABLE` blocks with separated `GO` batch runs to dynamically provision the `SelectedUnitsJson` column on both `dbo.RE_Applications` and `dbo.RE_ApplicationsAudit` tables.
3. **Execution & Validation:** Successfully executed the setup script against the local database instance and validated that `SelectedUnitsJson` was cleanly added.

---

## [2026-07-12] - UC 05 Submit Application Form Upgrades (CIPC Validations, Sorting, Multiple Unit Selection)

### Status: [x] Completed

**Completed Tasks:**
* [x] Update `RE_Application.cs` and `RE_ApplicationAudit.cs` models with the `SelectedUnitsJson` C# property.
* [x] Update `RealEstateController.cs`:
  * [x] Change sequence reference prefix from `"RED-"` to `"DPRE-"`.
  * [x] Sort CCC list alphabetically.
  * [x] Add backend ModelState validations for CIPC registration, SARS tax PIN, VAT number, telephones, mobile/cell, fax, and postal codes.
  * [x] Parse `SelectedUnitsJson` on submission to compute the sum of rental, setting `SelectedFacilityUnitId` to the first selected unit for backward compatibility.
* [x] Update `Capture.cshtml` UI:
  * [x] Add HTML5 validation attributes (regex patterns, max lengths) for input fields.
  * [x] Build dynamic, interactive multi-select table for unit selection with real-time tariff aggregation.
  * [x] Implement "Remove/Delete" links for uploaded pre-qualification files.
* [x] Update the Unified Use Cases document (`Unified_Use_Cases_Real_Estate.md`) to reflect these new constraints.

---

## [2026-07-12] - Database Deployment Fixes & Sidebar Menu Standardization

### Status: [x] Completed

**Changes:**
1. **Deployment DDL Migration:** Created a dedicated SQL script [dpre_plm_deployment_fixes.sql](file:///C:/REPO/PLM%20V1/PLM-EHC/DatabaseScripts/dpre_plm_deployment_fixes.sql) to add the missing `RealEstateApplicationId` column to `dbo.Documents`, `dbo.PLMApplicationHistortyLogs`, `dbo.PLMApplicationHistortyLogAudits`, `dbo.RoundRobinQueues`, `dbo.RoundRobinQueueAudits`, and `dbo.DocumentAudits` tables. Verified the script runs cleanly on the database.
2. **C# Audit Class Mapping:** Updated C# classes `DocumentAudit.cs` and `RoundRobinQueueAudit.cs` with the `RealEstateApplicationId` property to match the new database columns and prevent EF SaveChanges audit copy runtime crashes.
3. **Standardized Side Navigation:** Edited [RCS_Layout.cshtml](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Views/Shared/RCS_Layout.cshtml) to rename all sidebar links under the `Real Estate Queue` and `Facilities Queue` sections to exactly match the functional Use Case titles (e.g., `Validate Proof of Payment`, `Capture Risk Assessment Outcome`, `Initiate Departmental Review`, etc.).
4. **Walkthrough & Verification:** Compiled the solution successfully with no errors using MSBuild, updated navigation instructions inside [Unified_Use_Cases_Real_Estate.md](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/Real%20Estate/Unified_Use_Cases_Real_Estate.md), and confirmed local page rendering.

---

## [2026-07-13] - Use Cases and User Guide Text Standardization

### Status: [x] Completed

**Changes:**
1. **Repository Use Cases Updated:** Aligned all system navigation references (from old "Financials", "Assessment", "Inspections", and "Maintenance" main/sub-menus) to match the actual application menus exactly in:
   * [DPRE_PLM_uc_0.1_updated.md](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/Real%20Estate/DPRE_PLM_uc_0.1_updated.md)
   * [DPRE_PLM_Dev_Use_Cases.md](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/Real%20Estate/DPRE_PLM_Dev_Use_Cases.md)
2. **C Drive Use Cases Updated:** Replaced all old navigation labels in the C Drive directory files:
   * [DPRE_PLM_Dev_Use_Cases.md](file:///C:/Real%20estate%20use%20cases/DPRE_PLM_Dev_Use_Cases.md)
   * [Use_Cases_v0.2.md](file:///C:/Real%20estate%20use%20cases/Use_Cases_v0.2.md)
3. **User Guide Real Estate Updated:** Aligned the back-office procedures to use the standardized sidebar menu labels (e.g. `Validate Proof of Payment`, `Capture Risk Assessment Outcome`, `Capture Departmental Reviews and Comments`, etc.) in [user_guide_real_estate.md](file:///C:/Real%20estate%20use%20cases/monday%20demo%204k/user_guide_real_estate.md).


