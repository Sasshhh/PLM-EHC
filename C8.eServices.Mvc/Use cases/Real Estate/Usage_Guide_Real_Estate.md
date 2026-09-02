# Real Estate Development (RED) System - Usage & Architectural Reference Guide

This document establishes the official development guidelines, directory boundaries, database schema conventions, and testing protocols for the **Real Estate Development (RED)** system (Department ID = 2). It serves as the definitive reference to ensure complete isolation from the residential/housing (EHC/HSD) workflows.

---

## 1. Architectural Scope & Directory Boundaries

To guarantee complete separation of concerns and prevent context/data leakage, all development tasks specific to the Real Estate system must remain strictly within the following boundaries:

* **Controllers:**
  * Only modify or inspect controllers residing in `C8.eServices.Mvc/Controllers/` that manage the Real Estate workflows:
    * [RealEstateController.cs](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Controllers/RealEstateController.cs) (Client-facing actions: Capture, MyApplications, SelectInspectionSlot, RequestPto).
    * [RealEstateAdminController.cs](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Controllers/RealEstateAdminController.cs) (Back-office actions: VerifyPayment, VerifyRisk, InitiateCirculation, CaptureDepartmentalComment, ConsolidateApplication, ReviewCommitteeItem, AuthoriseApplication, ScheduleInspection, ConductInspection, AuthoriseWorkOrder, and PTO approvals).
* **Views:**
  * Client views must reside inside `C8.eServices.Mvc/Views/RealEstate/`.
  * Back-office / official views must reside inside `C8.eServices.Mvc/Views/RealEstateAdmin/`.
* **Playwright E2E Tests:**
  * All E2E test specs, mock upload directories, and presentation automation must be created inside `C8.eServices.Mvc/Tests/Playwright/RealEstate/`.
* **Use Cases & Documentation:**
  * All documentation, work logs, and use cases specific to Real Estate must be stored under `C8.eServices.Mvc/Use cases/Real Estate/`.

> [!WARNING]
> **STRICT ISOLATION POLICY:** 
> Do NOT read, modify, or reference EHC-specific or HSD-specific controllers/views (such as `PropertyLeaseApplicationController.cs`, `HumanSettlementApplicationController.cs`, or views inside `/PropertyLeaseApplication/` and `/HumanSettlementApplication/`). Real Estate must remain 100% independent.

---

## 2. Database Schema & Reference Guidelines

All database interactions, schema changes, and seed scripts must respect the following table naming and linking conventions:

### 2.1 Real Estate Tables (Prefix `RE_`)
All tables storing data exclusively for the Real Estate system are prefixed with `RE_` to isolate them from EHC/HSD data structures:
* **`RE_Applications`**: Stores applicant profiles, lease specifications, risk outcomes, inspections, works orders, and Permission to Occupy (PTO) records.
* **`RE_DepartmentalComments`**: Tracks inter-departmental circulation feedback.
* **`RE_Facilities`**: Stores physical properties and sites available for lease.
* **`RE_FacilityUnits`**: Stores units and letting spaces.
* **`RE_FacilityCategories`**: Stores tariff structures and sizes.
* **`RE_FacilityAudits` / `RE_FacilityCategoryAudits` / `RE_FacilityUnitAudits` / `RE_ApplicationsAudit`**: Audit tables prefix-matched for historical tracking.

### 2.2 Shared Audit & File Linking Tables
To leverage the shared system logs and file management without data collision, the following tables must be queried using the dedicated foreign key `RealEstateApplicationId`:
* **`dbo.Documents`**: Files uploaded during Capture, Risk Assessment, Committee Resolutions, Inspections, Job Sheets, and PTO requests link back using the `RealEstateApplicationId` column.
* **`dbo.PLMApplicationHistortyLogs`**: Chronological workflow audit trails are appended using the `RealEstateApplicationId` column.
* **`dbo.RoundRobinQueues`**: Workflow items in the shared round-robin assignment table map to the specific Real Estate application using `RealEstateApplicationId`.

> [!IMPORTANT]
> **C# Model Integrity:**
> When adding or modifying audit columns, make sure the corresponding EF audit mappings (e.g., `DocumentAudit.cs` and `RoundRobinQueueAudit.cs`) also declare the `RealEstateApplicationId` property to avoid `SaveChanges()` exceptions during audit copy transactions.

---

## 3. Playwright E2E Test Automation Protocol

To ensure consistent headed video presentation runs, all Playwright E2E tests for the Real Estate system must comply with these guidelines:

### 3.1 Execution Directory (Cwd)
Always execute Playwright commands with the current working directory (`Cwd`) set to `C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Tests\Playwright`. 
Running tests from the project root will result in `test.use()` import errors due to multiple versions of `@playwright/test` conflicting.

* **Correct Execution Command:**
  ```powershell
  $env:PATH = "C:\Users\sashe\.gemini\antigravity-ide\scratch\node\node-v20.11.0-win-x64;" + $env:PATH; npx playwright test RealEstate/UC_RealEstate_RoleBased_E2E.spec.js
  ```

### 3.2 Zoom Configuration & 4K Recording
All tests should configure the viewport to `3840 x 2160` (4K UHD) and apply a browser zoom of `1.75` for visual readability on recording playbacks.
* Use the following helper inside your tests:
  ```javascript
  async function applyZoom(page) {
    try {
      await page.waitForSelector('body', { timeout: 5000 });
      await page.evaluate(() => {
        if (document.body) {
          document.body.style.zoom = '1.75';
        }
      });
    } catch (err) {
      console.warn('Failed to zoom:', err.message);
    }
  }
  ```

---

## 4. Seeded Test Users & Role-Based Routing

The database setup scripts configure a clean set of test users mapped to specific roles. When testing or programming back-office queues, **only** use the following accounts:

| Username | Password | Role | Workflow Stage / Action |
| :--- | :--- | :--- | :--- |
| `RealEstateCustomer` | `Arsenal5@` | Lease Applicant (Client) | Capture application, confirm inspection slots, request PTO |
| `re_finance_officer` | `Arsenal5@` | Finance Administrator | Verify application fee payment (POP) |
| `re_property_officer` | `Arsenal5@` | Property Manager | Conduct Risk assessment, view comments, schedule inspections, review PTO |
| `re_committee_member` | `Arsenal5@` | Area Manager | Log Evaluation Committee resolutions and resolutions PDF |
| `re_hod` | `Arsenal5@` | Back Office System Administrator | HOD final lease contract award and HOD PTO approvals |
| `re_facilities_manager` | `Arsenal5@` | Property & Facilities Manager | Authorise Works Orders, PM planning, assign technicians, close orders |
| `re_technician` | `Arsenal5@` | Caretaker | Accept maintenance jobs, upload job sheets |

---

## 5. Developer & AI Assistant Protocol (No-Confusion Rules)

When pair programming with the user, follow these directives to avoid confusing the Real Estate system with the EHC/Residential system:

1. **Check the Folder Prefix:** Only inspect and edit code inside files containing the `RealEstate` string. Do not query or modify EHC codebase files.
2. **Disambiguate Use Cases:**
   * **Real Estate (RED)** use cases are strictly numbered `RE_UC001` through `RE_UC020`.
   * **Residential (EHC)** use cases starting from `UC021` onwards (e.g. Lease Renewal, Termination, Eviction, Disputes, Audit Trails) are part of the housing system and must not be touched or blended into Real Estate controllers.
3. **Keep the Lore Consistent:** All references to CCC selection, calculated rentals, and pre-qualification document checklists must match the specific Real Estate implementation (defined in `Unified_Use_Cases_Real_Estate.md`) rather than EHC templates.
