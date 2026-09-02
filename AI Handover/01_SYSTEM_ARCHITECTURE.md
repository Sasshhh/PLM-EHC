# System Architecture — Multi-Department PLM Platform

## 1. Platform Overview

The PLM (Property Lease Management) system is a **multi-departmental platform** designed to serve multiple City of Ekurhuleni (CoE) business units from a **single shared codebase**. Each department operates as an isolated branch within the same ASP.NET MVC application.

```
┌──────────────────────────────────────────────────────────┐
│                    PLM V1 Platform                       │
│                (Single ASP.NET MVC App)                  │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │    EHC      │  │ Real Estate │  │  Future Branch  │  │
│  │  (Housing)  │  │   (DPRE)    │  │  (e.g. Roads)   │  │
│  │ DeptID: 1   │  │ DeptID: 3   │  │  DeptID: TBD    │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
│                                                          │
│            Shared: Auth, Layout, Status tables,          │
│            CCCs, SystemUsers, Customers, Documents       │
└──────────────────────────────────────────────────────────┘
```

## 2. Department Isolation via DepartmentId

Each branch is separated by `DepartmentId` in the database:

| DepartmentId | Branch Name | Table Prefix | Status |
|:---:|:---|:---|:---|
| 1 | EHC (Ekurhuleni Housing Company) | *(none — first branch, no prefix)* | Production |
| 3 | Real Estate / DPRE (Development Planning & Real Estate) | `RE_` | In Development |
| TBD | Future departments | To be determined (e.g., `RD_` for Roads) | Planned |

> **KEY RULE:** When working in the Real Estate branch, ONLY create tables with the `RE_` prefix. EHC has no prefix because it was the first branch built. All NEW tables added to any branch going forward MUST have a prefix to avoid naming collisions.

## 3. Table Naming Convention

### EHC Branch (No Prefix — Legacy First Branch)
```
PropertyLeaseApplications     ← Main application table
LeaseDetails                   ← Lease records
MatchedUnits                   ← Unit matching
ApplicationAllocatedProperties ← Available units
PropertyLeaseAgreementMaster   ← Signed agreements
ConductUnitInspection          ← Inspection records
AllocatedUnitMaintenanceEHC    ← Maintenance records
waitingListQues                ← Waiting list queue
LeaseTerminations              ← Termination records
RoundRobinQueues               ← Workflow task queue (shared)
```

### Real Estate Branch (RE_ Prefix)
```
RE_Applications                ← Main application table
RE_Facilities                  ← Facility inventory
RE_FacilityCategories          ← Tariff matrix & categories
RE_FacilityUnits               ← Unit let-spaces
RE_DepartmentalComments        ← Departmental review feedback
RE_ApplicationsAudit           ← Application audit trail
RE_FacilityAudits              ← Facility change audit
RE_FacilityCategoryAudits      ← Category change audit
RE_FacilityUnitAudits          ← Unit change audit
RE_DepartmentalCommentAudits   ← Comment change audit
RE_EvaluationCriteria_PreQualDoc       ← Pre-qualification doc checklist
RE_EvaluationCriteria_PreQualEval      ← Evaluation scoring matrix
RE_EvaluationCriteria_CommitteeChecklist ← Committee compliance checklist
```

### Shared Tables (Used by ALL Branches)
```
SystemUsers, AspNetUsers, AspNetRoles, AspNetUserRoles  ← Authentication
Customers, Agents                                        ← Customer profiles
Status, StatusTypes                                      ← State machine
ResponsibilityTypes                                      ← Queue routing
RCSActionTypes                                           ← Decision outcomes
ActivityTrackerMessages                                  ← Audit messages
EmailContentTypes                                        ← Email templates
DocumentTypes                                            ← Document classifications
Documents                                                ← File storage
PLMApplicationHistortyLogs                               ← Universal audit trail
PLMApplicationHistortyLogAudits                          ← Audit trail audits
CCCs                                                     ← Customer Care Centres
DepartmentsCoEs                                          ← City departments
RoundRobinQueues                                         ← Workflow routing (shared)
ReferenceTypes                                           ← Reference number types
```

## 4. Controller & View Isolation

### EHC Branch
| Controller | Views Folder |
|:---|:---|
| `PropertyLeaseApplicationController.cs` | `Views/PropertyLeaseApplication/` |
| `DepartmentsApprovalsController.cs` | `Views/DepartmentsApprovals/` |
| `RiskAssessmentOutcomesController.cs` | `Views/RiskAssessmentOutcomes/` |
| `LeaseDetailsController.cs` | `Views/LeaseDetails/` |
| `MatchedUnitsController.cs` | `Views/MatchedUnits/` |
| `DocumentController.cs` | `Views/Document/` |
| `ServiceRequestsController.cs` | `Views/ServiceRequests/` |
| `PaymentTransgressionsController.cs` | `Views/PaymentTransgressions/` |

### Real Estate Branch
| Controller | Views Folder | Purpose |
|:---|:---|:---|
| `RealEstateController.cs` | `Views/RealEstate/` | Client-facing (applicant) |
| `RealEstateAdminController.cs` | `Views/RealEstateAdmin/` | Back-office (officials) |

### Shared
| Controller | Purpose |
|:---|:---|
| `AccountController.cs` | Login, Registration, Password Reset |
| `FileController.cs` | File upload/download |
| `ApplicationUserRoleController.cs` | Role-based sidebar navigation |

## 5. Workflow Engine Architecture

### EHC Branch
Uses the centralized `EHCWorkflowEngine.EHCRoundRobin(...)` method (in `Helpers/EHCWorkflowEngine.cs`) with boolean flags to route between actors:
- `RiskAssessment = true` → Routes to Risk Assessment queue
- `LeaseAgreementValidation = true` → Routes to Lease Signing queue
- `Terminations = true` → Routes to Termination queue
- etc.

### Real Estate Branch
Uses **direct status-based routing** without the EHC RoundRobin engine. Applications move through statuses and queues are filtered by `StatusId` in the controller's LINQ queries.

Helper: `RealEstateWorkAllocationHelper.cs` — manages RE-specific queue routing.

## 6. Visual Theme Isolation

### EHC Branch
Uses the **default Bootstrap theme** from `RCS_Layout.cshtml` and shared CSS.

### Real Estate Branch
Uses the **Onyx Obsidian gold/amber theme** applied via `<style>` blocks scoped locally inside each RE view. Key design tokens:

| Token | Hex | Usage |
|:---|:---|:---|
| `--re-gold` | `#d2930b` | Primary accent |
| `--re-gold-hover` | `#b57c04` | Hover states |
| `--re-gold-light` | `#fefcf3` | Card backgrounds |
| `--re-text-dark` | `#1e293b` | Headings |
| `--re-banner-grad` | `linear-gradient(135deg, #1e293b, #0f172a)` | Premium headers |

> **CRITICAL RULE:** The shared layout file `Views/Shared/RCS_Layout.cshtml` and global sidebar/navigation **MUST NOT be modified** by the Real Estate branch. All custom branding is declared locally via `<style>` blocks or inline styles inside `Views/RealEstate/*` and `Views/RealEstateAdmin/*`.

## 7. Database Connection

The Real Estate branch uses a **separate SQL Server database** from EHC:

| Branch | Database Name | Connection String Key |
|:---|:---|:---|
| EHC | `eServices_DB` | Default connection |
| Real Estate | `PropertyLeaseManagementRealEstate` | Same server, different database |

Both databases share the same schema structure for shared tables (Status, SystemUsers, etc.), but each has its own instance of the data.

## 8. Future Branch Guidelines

When adding a new department branch (e.g., Roads & Stormwater):
1. Choose a 2-3 letter prefix (e.g., `RD_` for Roads)
2. Create all new tables with that prefix
3. Create dedicated Controllers (`RoadsController.cs`, `RoadsAdminController.cs`)
4. Create dedicated View folders (`Views/Roads/`, `Views/RoadsAdmin/`)
5. Assign a unique `DepartmentId` 
6. Style locally — never touch `RCS_Layout.cshtml`
7. Later, branches can be merged by unifying shared workflow queues via `DepartmentId` filtering
