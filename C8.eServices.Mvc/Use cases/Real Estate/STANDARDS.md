# Real Estate Development (RED) System Standards

This document establishes the architecture, boundaries, and conventions for the **Real Estate Development** system (Department ID = 2). 

These standards ensure that any agentic AI or developer working on the `RealEstate-Development` branch stays strictly focused on the Real Estate components, avoiding unnecessary context from other departments (EHC or HSD).

---

## 1. Architectural Scope & Directory Boundaries

To prevent context leakage, developers and AI agents must restrict their focus to files and structures marked specifically for Real Estate:

* **Controllers:**
  * Only modify or inspect controllers that reside in `C8.eServices.Mvc/Controllers/` and start with the prefixes `RealEstate` or `RE_` (e.g., `RealEstateController.cs`, `RE_LeaseController.cs`).
* **Views:**
  * All views, forms, and pages must reside strictly inside `C8.eServices.Mvc/Views/RealEstate/`.
* **Playwright E2E Tests:**
  * All test specs and mock states must be created inside `C8.eServices.Mvc/Tests/Playwright/RealEstate/`.
* **Database Schema:**
  * Tables, columns, or triggers specific to Real Estate must be prefixed with `RE_` or `RealEstate_`.

> [!IMPORTANT]
> Do NOT read, modify, or reference EHC-specific or HSD-specific controllers/views (such as `PropertyLeaseApplicationController.cs` or `HumanSettlementApplicationController.cs`) unless modifying shared global layout files (e.g., `RCS_Layout.cshtml`, `ProfileController.cs`) for cross-cutting features like main dashboard routing or navigation bars.

---

## 2. Shared File Modifications (Cross-Cutting Concerns)

When modifying files shared by all departments:

1. **`RCS_Layout.cshtml` (Side Navigation):**
   * Real Estate menus must be restricted using:
     ```csharp
     if (SecurityHelper.UserDepartment().Equals(ApplicationEntityKeys.RealEstateDevelopment))
     {
         // Render ONLY Real Estate specific menus
     }
     ```
2. **`ProfileController.cs` (Redirection Logic):**
   * Real Estate customers landing on the entry profile action (`Index3`) must be directed to `/RealEstate/Inbox`:
     ```csharp
     if (userDepartment.Equals(ApplicationEntityKeys.RealEstateDevelopment))
     {
         return RedirectToAction("Inbox", "RealEstate");
     }
     ```

---

## 3. Workflow & Data Isolation

* **Department ID Mapping:**
  * Always verify that the current user belongs to the Real Estate department (`DepartmentId = 2`) before displaying data or processing forms.
* **Master Data Isolation:**
  * Do not query or modify `ApplicationEntityKeys` belonging to `EkurhuleniHousingCompany` or `HumanSettlmentDevelopment`. Keep all operations strictly aligned to the `RealEstateDevelopment` key.

---

## 4. Single Source of Truth for Use Cases (RED)

* **Reference Document:** [Unified_Use_Cases_Real_Estate.md](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/Real%20Estate/Unified_Use_Cases_Real_Estate.md) is the authoritative Single Source of Truth for all Real Estate workflows (`RE_UC001` through `RE_UC020`).
* **Recent Changes (Capture - RE_UC005):**
  * **Banking Details Removed:** No banking details are captured in the client-facing UI or forms. They are initialized to empty/default strings and saved directly in the database.
  * **Updated Pre-Qualification List:** The pre-qualification document checklist contains 14 documents in total. Refer to `Unified_Use_Cases_Real_Estate.md` Section 5 for the exact checklist.
* **Testing Guidelines:** Do not run Playwright E2E tests from the project root; they must be run from `C8.eServices.Mvc/Tests/Playwright/` to avoid import conflicts. Set viewport to 4K (`3840x2160`) and zoom to `1.75` for visual clarity.
