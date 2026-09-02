# File Map — Use Cases → Source Files

This document maps every Real Estate use case (RE_UC001–RE_UC025) to the exact source code files that implement it.

---

## Legend

| Symbol | Meaning |
|:---|:---|
| 🎮 | Controller action |
| 👁️ | View (`.cshtml`) |
| 📦 | Model (`.cs`) |
| 🔧 | Helper (`.cs`) |
| 🗄️ | Database table |
| 📝 | SQL script |

---

## Core Real Estate Source Files

### Controllers
| File | Path | Purpose |
|:---|:---|:---|
| `RealEstateController.cs` | [Controllers/RealEstateController.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Controllers/RealEstateController.cs) | Client-facing actions (Capture, MyApplications, LeaseAgreements, Inspection, PTO) |
| `RealEstateAdminController.cs` | [Controllers/RealEstateAdminController.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Controllers/RealEstateAdminController.cs) | Back-office actions (Payment, Risk, Departmental, Committee, HOD, Inspection, WorkOrders, PTO) |

### Models (All new — `RE_` prefix)
| File | Path | DB Table |
|:---|:---|:---|
| `RE_Application.cs` | [Models/RE_Application.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/RE_Application.cs) | `RE_Applications` |
| `RE_Facility.cs` | [Models/RE_Facility.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/RE_Facility.cs) | `RE_Facilities` |
| `RE_FacilityCategory.cs` | [Models/RE_FacilityCategory.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/RE_FacilityCategory.cs) | `RE_FacilityCategories` |
| `RE_FacilityUnit.cs` | [Models/RE_FacilityUnit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/RE_FacilityUnit.cs) | `RE_FacilityUnits` |
| `RE_DepartmentalComment.cs` | [Models/RE_DepartmentalComment.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/RE_DepartmentalComment.cs) | `RE_DepartmentalComments` |

### Audit Models
| File | Path | DB Table |
|:---|:---|:---|
| `RE_ApplicationAudit.cs` | [Models/Audits/RE_ApplicationAudit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/Audits/RE_ApplicationAudit.cs) | `RE_ApplicationsAudit` |
| `RE_FacilityAudit.cs` | [Models/Audits/RE_FacilityAudit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/Audits/RE_FacilityAudit.cs) | `RE_FacilityAudits` |
| `RE_FacilityCategoryAudit.cs` | [Models/Audits/RE_FacilityCategoryAudit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/Audits/RE_FacilityCategoryAudit.cs) | `RE_FacilityCategoryAudits` |
| `RE_FacilityUnitAudit.cs` | [Models/Audits/RE_FacilityUnitAudit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/Audits/RE_FacilityUnitAudit.cs) | `RE_FacilityUnitAudits` |
| `RE_DepartmentalCommentAudit.cs` | [Models/Audits/RE_DepartmentalCommentAudit.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Models/Audits/RE_DepartmentalCommentAudit.cs) | `RE_DepartmentalCommentAudits` |

### Helpers
| File | Path | Purpose |
|:---|:---|:---|
| `RealEstateUserAgreementHelper.cs` | [Helpers/RealEstateUserAgreementHelper.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Helpers/RealEstateUserAgreementHelper.cs) | Dynamic PDF generation for PTO/Lease agreements |
| `RealEstateWorkAllocationHelper.cs` | [Helpers/RealEstateWorkAllocationHelper.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Helpers/RealEstateWorkAllocationHelper.cs) | RE-specific workflow queue routing |

### Key Configuration Files
| File | Path | Purpose |
|:---|:---|:---|
| `StatusKeys.cs` | [Keys/StatusKeys.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Keys/StatusKeys.cs) | C# constants for status keys |
| `ResponsibilityTypeKeys.cs` | [Keys/ResponsibilityTypeKeys.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Keys/ResponsibilityTypeKeys.cs) | C# constants for queue routing types |
| `eServicesDbContext.cs` | [DataAccessLayer/eServicesDbContext.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/DataAccessLayer/eServicesDbContext.cs) | EF DbContext — includes RE_ DbSets |

---

## Use Case → File Mapping

### RE_UC001–UC004: User Registration & Login
| Component | File |
|:---|:---|
| 🎮 Controller | `AccountController.cs` |
| 👁️ Views | `Views/Account/Login.cshtml`, `Register.cshtml` |
| 🗄️ Tables | `SystemUsers`, `AspNetUsers`, `Customers`, `Agents` |

### RE_UC005: Submit Application for Lease
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateController.cs` → `Capture (GET/POST)` |
| 👁️ Views | `Views/RealEstate/Capture.cshtml` |
| 📦 Model | `RE_Application.cs` |
| 🗄️ Tables | `RE_Applications`, `RE_Facilities`, `RE_FacilityUnits`, `RE_FacilityCategories`, `Documents` |

### RE_UC006: Validate Proof of Payment
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `ApplicationFeePayments`, `VerifyPayment` |
| 👁️ Views | `Views/RealEstateAdmin/ApplicationFeePayments.cshtml`, `VerifyPayment.cshtml` |
| 🗄️ Tables | `RE_Applications`, `Documents` |

### RE_UC007: Capture Risk Assessment Outcome
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `RiskAssessments`, `VerifyRisk` |
| 👁️ Views | `Views/RealEstateAdmin/RiskAssessments.cshtml`, `ConductAssessment.cshtml` |
| 🗄️ Tables | `RE_Applications` |

### RE_UC008: Initiate Departmental Review
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `InitiateCirculation` |
| 🗄️ Tables | `RE_Applications`, `DepartmentsCoEs` |

### RE_UC009: Capture Departmental Reviews & Comments
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `DepartmentalQueue`, `CaptureDepartmentalComment` |
| 👁️ Views | `Views/RealEstateAdmin/DepartmentalQueue.cshtml`, `CaptureDepartmentalComment.cshtml` |
| 📦 Model | `RE_DepartmentalComment.cs` |
| 🗄️ Tables | `RE_DepartmentalComments`, `RE_Applications` |

### RE_UC010: Consolidate Departmental Feedback
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `ConsolidateFeedback`, `ConsolidateApplication` |
| 👁️ Views | `Views/RealEstateAdmin/ConsolidateFeedback.cshtml`, `ConsolidateApplication.cshtml` |
| 🗄️ Tables | `RE_DepartmentalComments`, `RE_Applications` |

### RE_UC011: Committee Review and Decision
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `CommitteeReviews`, `ReviewCommitteeItem` |
| 👁️ Views | `Views/RealEstateAdmin/CommitteeReviews.cshtml`, `ReviewCommitteeItem.cshtml` |
| 🗄️ Tables | `RE_Applications`, `Documents` |

### RE_UC012: Application Final Authorisation (HOD)
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `FinalAuthorisation`, `AuthoriseApplication` |
| 👁️ Views | `Views/RealEstateAdmin/FinalAuthorisation.cshtml`, `AuthoriseApplication.cshtml` |
| 🗄️ Tables | `RE_Applications` |

### RE_UC013: Schedule Unit Inspection
| Component | File |
|:---|:---|
| 🎮 Controller (Officer) | `RealEstateAdminController.cs` → `InspectionSchedules`, `ScheduleInspection` |
| 🎮 Controller (Client) | `RealEstateController.cs` → `SelectInspectionSlot` |
| 👁️ Views | `Views/RealEstateAdmin/InspectionSchedules.cshtml`, `ScheduleInspection.cshtml`, `Views/RealEstate/SelectInspectionSlot.cshtml` |

### RE_UC014: Conduct Unit Inspection
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `ConductInspections`, `ConductInspection` |
| 👁️ Views | `Views/RealEstateAdmin/ConductInspections.cshtml`, `ConductInspection.cshtml` |

### RE_UC015: Authorise Works Order
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `WorkOrders`, `AuthoriseWorkOrder` |
| 👁️ Views | `Views/RealEstateAdmin/WorkOrders.cshtml`, `AuthoriseWorkOrder.cshtml` |

### RE_UC018–UC020: PTO Request, Review, Approval
| Component | File |
|:---|:---|
| 🎮 Controller (Client) | `RealEstateController.cs` → `RequestPto` |
| 🎮 Controller (Officer) | `RealEstateAdminController.cs` → `PtoReviews`, `ReviewPto` |
| 🎮 Controller (HOD) | `RealEstateAdminController.cs` → `PtoAuthorisations`, `AuthorisePto` |
| 👁️ Views | `Views/RealEstate/RequestPto.cshtml`, `Views/RealEstateAdmin/PtoReviews.cshtml`, `ReviewPto.cshtml`, `PtoAuthorisations.cshtml`, `AuthorisePto.cshtml` |

### RE_UC021: Generate PTO Certificate
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `DownloadPtoCertificatePdf` |
| 🔧 Helper | `RealEstateUserAgreementHelper.cs` |
| 📄 Template | `PDFTemplates/RealEstate/Updated User Agreement.doc` |

### RE_UC023: Generate & Sign Lease Agreement
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `DownloadLeaseAgreementPdf` |
| 🔧 Helper | `RealEstateUserAgreementHelper.cs` |
| 📄 Template | `PDFTemplates/RealEstate/URC User Agreement_Latest.doc` |

### Evaluation Criteria (UC020/UC021/UC025 Supporting UI)
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `EvaluationCriteria` |
| 👁️ View | `Views/RealEstateAdmin/EvaluationCriteria.cshtml` |
| 🗄️ Tables | `RE_EvaluationCriteria_PreQualDoc`, `RE_EvaluationCriteria_PreQualEval`, `RE_EvaluationCriteria_CommitteeChecklist` |

### Admin Dashboard (Master Data CRUD)
| Component | File |
|:---|:---|
| 🎮 Controller | `RealEstateAdminController.cs` → `Index` (+ CRUD actions for Facilities, Categories, Units) |
| 👁️ View | `Views/RealEstateAdmin/Index.cshtml` |
| 🗄️ Tables | `RE_Facilities`, `RE_FacilityCategories`, `RE_FacilityUnits` |

### Consolidated Report PDF
| Component | File |
|:---|:---|
| 👁️ View | `Views/RealEstateAdmin/ConsolidatedReportPdf.cshtml` |

---

## Database Scripts Location Map

| Script | Path | Purpose |
|:---|:---|:---|
| **Unified Setup** | [db/unified/setup.sql](file:///c:/REPO/PLM%20V1/db/unified/setup.sql) | Complete RE schema + seed data (tables, CCCs, categories, facilities, units, users, statuses) |
| **Banking Migration** | [db/migrations/20260630_make_banking_columns_nullable.sql](file:///c:/REPO/PLM%20V1/db/migrations/20260630_make_banking_columns_nullable.sql) | Make banking columns nullable |
| **Workflow Setup** | [DatabaseScripts/RE_WorkflowSetup_Data.sql](file:///c:/REPO/PLM%20V1/DatabaseScripts/RE_WorkflowSetup_Data.sql) | Roles, users, department mappings |
| **UC21-25 Setup** | [DatabaseScripts/RE_UseCases_21_25_Setup.sql](file:///c:/REPO/PLM%20V1/DatabaseScripts/RE_UseCases_21_25_Setup.sql) | Evaluation criteria tables + PTO/Lease statuses |
| **Prod Complete** | [DatabaseScriptsBackup/RE_PropertyLeaseManagementRealEstate_CompleteSetup_PROD.sql](file:///c:/REPO/PLM%20V1/DatabaseScriptsBackup/RE_PropertyLeaseManagementRealEstate_CompleteSetup_PROD.sql) | Full production backup script |
| **Master Migrations** | [SQL_MIGRATIONS_MASTER_ALL_USE_CASES.sql](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/SQL_MIGRATIONS_MASTER_ALL_USE_CASES.sql) | Consolidated UC01-UC25 migrations |
| **EHC Metadata** | [Use cases/Unified_PLM_Metadata_Migration.sql](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/Unified_PLM_Metadata_Migration.sql) | EHC branch metadata (statuses, responsibilities, etc.) |
| **All Statuses** | [Use cases/All_Status_Metadata.sql](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Use%20cases/All_Status_Metadata.sql) | Complete status table seed |

---

## Test Files

| File | Path | Purpose |
|:---|:---|:---|
| E2E Submission | `Tests/Playwright/RealEstate/UC_RealEstate_Application_Submission.spec.js` | Fast application submission test |
| E2E Slow Demo | `Tests/Playwright/RealEstate/UC_RealEstate_Application_Submission_Slow.spec.js` | 4K video capture submission |
| E2E Full Flow | `Tests/Playwright/RealEstate/UC_RealEstate_Full_EndToEnd.spec.js` | Complete workflow test |
| E2E Role-Based | `Tests/Playwright/RealEstate/UC_RealEstate_RoleBased_E2E.spec.js` | Multi-user role-based test |
