# PLM System - Unified Use Cases & Workflow Mapping

## ⚠️ EDITING INSTRUCTIONS & 1:1 MAPPING RULES

When updating this document or working on the system, the AI and Developers **MUST** adhere to the following 1:1 mapping standard. This document is the **Single Source of Truth** for workflows.

Every Use Case documented below MUST contain:
1. **Use Case ID & Name:** E.g., `UC01: Submit Application for Lease`
2. **Actors:** Primary and Secondary human or system roles involved.
3. **Controllers Touched:** The exact C# ASP.NET MVC controllers executing the logic.
4. **Views Touched:** The exact `.cshtml` files used in the flow.
5. **Instantiation & Workflow Calls:** Where the flow is routed, e.g., `EHCWorkflowEngine.EHCRoundRobin()`.
6. **Key Status Transitions:** What status triggers the flow and what it transitions to upon completion.
7. **Task Queue Integrity:** When closing active tasks via `RoundRobinMarkJobAsFinished`, ensure you pass the `Customer.Id` of the logged-in user completing the task, not a role-based ID (like the Letting Officer), to prevent queues from getting stuck on dashboards.
8. **POST Behaviour:** Every use case with a form submission MUST include a `### POST Behaviour` subsection that documents exactly what the controller's `[HttpPost]` action does for each branch (Approve, Reject, etc.) — including status changes, RoundRobin calls, emails sent, and where the user is redirected.

> [!IMPORTANT]
> **MANDATORY RULE: After modifying any controller action that is part of a documented use case, you MUST update the corresponding use case's `POST Behaviour` section in this document to reflect the change. This document is the Single Source of Truth — it must always match the live code.**

---

## 📖 TERMINOLOGY MAPPING (CEO vs. Director vs. Property Manager)

Due to legacy design and evolving business titles, there are several terms used interchangeably across the UI, the Database, and the Source Code. When modifying workflows for the final executive approval step, **ALWAYS USE THE PROPERTY MANAGER IDENTITY IN THE BACKEND**.

- **Front-End UI / Use Cases:** Often refers to the **CEO** or **Director**.
- **ASP.NET Identity Role:** The system strictly evaluates `@if (User.IsInRole("Property Manager"))` or assigns the `Property Manager` role to the CEO's user account. The `User.IsInRole("Director")` block in code is legacy/redundant and should generally be ignored in favor of Property Manager.
- **App Settings (Database):** The target queue assignment MUST route to `AppSettingKeys.PropertyManager` (`r_property_manager` in the `AppSettings` table). This maps to the `Customer.Id` of the person acting as the CEO.

---

## 📋 UNIFIED USE CASES

### UC01: Submit Application for Lease
- **Roles:** Applicant, Client Services Officer (CSO)
- **Controllers:** `PropertyLeaseApplicationController.cs`
- **Views:** `Capture.cshtml`, `Inbox.cshtml`, `Details.cshtml`
- **Workflow / Instantiation:** Initiates the primary workflow. Uses `EHCRoundRobin` to route to a CSO for assessment.

### UC02: Upload Application Fee Proof of Payment
- **Roles:** Applicant, Client Services Officer
- **Controllers:** `DocumentController.cs`
- **Views:** `ProofOfApplicationFeePayment.cshtml`, `Index.cshtml`
- **Workflow / Instantiation:** Triggers queue entry for Bookkeeper upon document upload.

### UC03: Validate Application Fee Payment
- **Roles:** Bookkeeper
- **Controllers:** `DepartmentsApprovalsController.cs`, `PropertyLeaseApplicationController.cs`
- **Views:** `ApplicationFeeValidation.cshtml` (DepartmentsApprovals & PropertyLeaseApplication)
- **Workflow / Instantiation:** 
  - On Approval: Transitions to `Awaiting Risk Assessment`.
  - On Rejection: Transitions back to `Awaiting Deposit Paid`, alerts applicant. Records via `MatchingHelper.AddHistoryLog()`.

### UC04: Capture Risk Assessment Outcome and Approval
- **Roles:** Client Services Officer, Revenue Manager, CEO
- **Controllers:** `RiskAssessmentOutcomesController.cs`
- **Views:** `ConductAssessment.cshtml`, `InboxRM.cshtml`, `InboxCEO.cshtml`
- **Workflow / Instantiation:** Uses `EHCRoundRobin` with specific boolean flags (e.g., `RiskAssessment = true`) to escalate through the multi-tier approval structure.

### UC05: Accept or Reject Unit Offer by Applicant
- **Roles:** Applicant
- **Controllers:** `PropertyLeaseApplicationController.cs`, `MatchedUnitsController.cs`
- **Views:** `Inbox.cshtml`, `AllocateOrMatchUnit.cshtml`, `MatchedUnits/UnitDetails.cshtml`
- **Workflow / Instantiation:** 
  - Acceptance: `MatchingHelper.AcceptMatchedUnit()`.
  - Rejection: Uses `MatchingHelper.RejectMatchedUnit()` via POST with a rejection reason string. 
    - 1st Reject -> Triggers `EHCRoundRobin(RiskAssessment=true)` to re-allocate via CSO. Status: `VertedApplication`.
    - 2nd Reject -> Discarded. Status: `ApplicationDiscardedNoUnitAvailable`.

### UC014: Generate Lease Agreements
- **Roles:** Client Services Officer
- **Controllers:** `PropertyLeaseApplicationController.cs` (`GenerateLeaseAgreement` action)
- **Views:** `GenerateLeaseAgreement.cshtml`
- **Workflow / Instantiation:** 
  - CSO captures financial and lease term details into `LeaseDetails` and `PropertyLeaseAgreementMaster`.
  - Generates PDF using template at `~/PDFTemplates/Revised Lease Agreement_v2.pdf`.
  - On completion, transitions status to `AwaitingLeaseAgreement` and routes to Tenant via `EHCRoundRobin(LeaseAgreementValidation=true)`.

### UC015: Deposit Payment Workflow / Sign Lease Agreements
- **Roles:** Applicant/Tenant, Client Services Officer, Revenue Manager, CEO
- **Controllers:** `FileController.cs`, `PropertyLeaseApplicationController.cs`
- **Views:** `PropertyLeaseDeposit()` view, `AssessmentFeeValidation`
- **Workflow / Instantiation:** 
  - Reuses status `s_rcs_pending_assessment_payment_validation`.
  - Differentiated by `ResponsibilityTypeId` in the `RoundRobinQueues`.

### UC018: Recommend for Lease Renewal
- **Roles:** Client Services Officer, Tenant
- **Controllers:** `DepartmentsApprovalsController.cs` (or specific renewal controller based on implementation)
- **Views:** Evaluating Application Screen.
- **Workflow / Instantiation:** Triggered automatically 3 months before lease expiration. Transitions lease to `Flagged for Lease Renewal`.

### UC17C: Payment Transgression
- **Roles:** Revenue Manager, Legal, Debt Collection
- **Controllers:** `PaymentTransgressionsController.cs`
- **Views:** `Details.cshtml`, `Index.cshtml`
- **Workflow / Instantiation:** Uses strict null checking for associated leases. Transitions through warning letters to eventual legal action or eviction based on unresolved transgressions.

### UC17D: Service Request Workflow (Maintenance)
- **Roles:** Tenant, Maintenance Officer, Inspector
- **Controllers:** `ServiceRequestsController.cs`
- **Views:** `Capture.cshtml`, `Inbox.cshtml`
- **Workflow / Instantiation:** Parallel workflow. Requires explicit `ResponsibilityTypeId` filtering when using `WorkAllocationHumanHelper.FinishAllPreviousWork()` to avoid colliding with main lease application Round Robin Queues.

### UC18: Conduct Unit Inspection & Maintenance Job Sheet
- **Roles:** Caretaker, Property & Facilities Manager, Maintenance Manager
- **Controllers:** `PropertyLeaseApplicationController.cs`, `FileController.cs`, `DocumentController.cs`
- **UI Navigation Path:** 
  - 1. System menu -> **Property Maintenance Overview** (`/PropertyLeaseApplication/PropertyMaintenanceOverview`)
  - 2. Select a unit -> **Property Facilities Manager Review** (`/PropertyLeaseApplication/PropertyFacilitiesManagerReview/{id}`)
  - 3. Select action -> **Conduct Unit Inspection** (`/PropertyLeaseApplication/ConductUnitInspection/{id}`) OR **Unit Maintenance / Job Sheet** (`/PropertyLeaseApplication/MaintenanceJobSheet/{id}`)
- **Workflow / Instantiation:** 
  - During the `ConductUnitInspection` POST action, if a unit is marked "Not Habitable", an `AllocatedUnitMaintenanceEHC` record is generated with a status of `AwaitingMaintananceJobSheet`.
  - The Job Sheet view uses `_DocumentsPartialConductUnitInspection.cshtml` to upload maintenance records, which relies on `FileController.AddDocument` and `DocumentController.RenderDocumentDetails` requiring proper `[Authorize]` roles.

### UC021: Lease Renewal Workflow (Three-Step)
- **Roles:** Tenant, Client Services Officer, Revenue Manager, CEO / Property Manager
- **Controllers:** `LeaseDetailsController.cs`
- **Views:** `Renewals.cshtml`, `RenewalLeaseAgreementValidation.cshtml`, `TenantLeaseRenewalOffer.cshtml`
- **Workflow / Instantiation:** Sequential three-step signature workflow initiated by CSO generating agreement.
  - Generates PDF using fixed template path `~/PDFTemplates/Revised Lease Agreement_v2.pdf`.
  - Routes via `RoundRobinMarkJobAsFinished()` and `WorkAllocationHumanHelper.FinishAllPreviousWork()` through specific queues.
- **Required Master Data:** 
  To ensure queues are visible in `Renewals.cshtml`, run the following SQL:
  ```sql
  INSERT INTO ResponsibilityTypes ([Key], [Name], [IsActive], [IsDeleted])
  VALUES
    ('r_renewal_rm_sign',     'Renewal RM Signature',     1, 0),
    ('r_renewal_ceo_sign',    'Renewal CEO Signature',    1, 0),
    ('r_renewal_tenant_sign', 'Renewal Tenant Signature', 1, 0)
  ```

### UC022: Lease Agreement Signing & Rejection (Three-Step Approval Chain)
- **Roles:** Tenant, Client Services Officer, Revenue Manager, CEO / Property Manager
- **Controllers:** `PropertyLeaseApplicationController.cs`
- **Views:**
  - `LeaseAgreementValidation.cshtml` — Tenant signing view (with rejection reason banner)
  - `RevenueManagerLeaseAgreementValidation.cshtml` — RM approval/rejection view
  - `PropertyManagerLeaseAgreementValidation.cshtml` — CEO/PM approval/rejection view
- **Workflow / Instantiation:**
  **Normal (Approval) Flow (Strictly Sequential):**
  1. **Tenant** signs (draws signature + witness) → Status: `AwaitingManagersSignature` → `EHCRoundRobin(LeaseAgreementValidation=true)` routes strictly to the Revenue Manager.
  2. **Revenue Manager** reviews and signs → Status transitions to `In Awaiting Lease Agreement Approval` → `EHCRoundRobin(AgreementApprovalCEO=true)` routes the application to the CEO/Director's queue.
  3. **CEO/PM** reviews and signs → Marks lease `Completed=true`, sends debit order email, and completes the lease agreement phase.

  **Rejection Flows (UC022 Extension):**
  Both RM and CEO have two rejection options via `RCSActionTypeKeys`:
  - `plm_not_supported_tenant` — **Reject to Tenant:** Tenant's documents or information are wrong.
  - `plm_not_supported_cso` — **Reject to CSO:** Lease details captured by CSO need correction.

  **Reject-to-Tenant Path:**
  1. All signatures cleared via `ClearLeaseSignatures()` (Tenant, Witness, PM, RM + dates).
  2. Rejection comment saved via `SaveLeaseRejectionComment()` and `MatchingHelper.AddCommentOnRejectAgreement()`.
  3. Status → `AwaitingLeaseAgreement`.
  4. Routes via `EHCRoundRobin(LeaseAgreementValidation=true)`.
  5. Tenant sees red `panel-danger` banner with rejection reason on `LeaseAgreementValidation.cshtml`.
  6. Tenant re-signs, then existing flow picks up → document upload → continues forward.

  **Reject-to-CSO Path:**
  1. All signatures cleared via `ClearLeaseSignatures()`.
  2. Rejection comment saved.
  3. Status → `AwaitingTenantUpdateDetails`.
  4. Routes via `EHCRoundRobin(UpdateTenantDetails=true)`.
  5. CSO sees item in `UpdateLeaseDetails` queue, corrects lease details.
  6. After CSO corrections, flow re-enters from UC014 (re-generation).

  **Signature Clearing Rule:**  
  > ⚠️ **Any rejection at any step clears ALL signatures globally** (Tenant, Witness 1, PM, RM + all date fields). This is by design — the agreement is legally invalidated once content changes, requiring a clean re-signing cycle.

  **Activity Tracker Messages:**
  - `atm_lease_agreement_rejected_to_tenant` — logged when RM or CEO rejects to Tenant.
  - `atm_lease_agreement_rejected_to_cso` — logged when RM or CEO rejects to CSO.

  **Key Helper Methods:**
  - `ClearLeaseSignatures(context, applicationId)` — Wipes all signature fields on `PropertyLeaseAgreementMaster`.
  - `SaveLeaseRejectionComment(context, applicationId, comment, rejectedBy, rejectedTarget)` — Writes structured rejection comment to `PropertyLeaseActionComments`.

  **Required Master Data (SQL):**
  ```sql
  IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_tenant')
      INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
      VALUES ('plm_not_supported_tenant', 'Not Supported - Tenant', 1, 0);

  IF NOT EXISTS (SELECT 1 FROM RCSActionTypes WHERE [Key] = 'plm_not_supported_cso')
      INSERT INTO RCSActionTypes ([Key], [Name], [IsActive], [IsDeleted])
      VALUES ('plm_not_supported_cso', 'Not Supported - CSO', 1, 0);

  IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_tenant')
      INSERT INTO ActivityTrackerMessages ([Key], [Description], [IsActive], [IsDeleted])
      VALUES ('atm_lease_agreement_rejected_to_tenant', 'Lease agreement rejected and returned to Tenant for corrections', 1, 0);

  IF NOT EXISTS (SELECT 1 FROM ActivityTrackerMessages WHERE [Key] = 'atm_lease_agreement_rejected_to_cso')
      INSERT INTO ActivityTrackerMessages ([Key], [Description], [IsActive], [IsDeleted])
      VALUES ('atm_lease_agreement_rejected_to_cso', 'Lease agreement rejected and returned to CSO for lease detail corrections', 1, 0);
  ```

  ### POST Behaviour — `PropertyManagerLeaseAgreementValidation` (CEO/PM)
  **Controller:** `PropertyLeaseApplicationController.cs` · Line ~7204
  **Redirect on all paths:** `RedirectToAction("PropertyLeaseAgreements")`

  | Branch | Status Change | RoundRobin Call | Other Actions |
  |--------|--------------|-----------------|---------------|
  | **Approved (All applicant types)** | → `ActiveLease` (`l_active_lease`) | ❌ None (future wiring point commented in code) | Sets `LeaseDetails.Completed = true`, `IsActive = true`, `StartDate` (if not already set). Sets `PropertyManagerSigned = true`, `PropertyManagerSignatureDate = DateTime.Now` on `PropertyLeaseAgreementMaster`. Sends hardcoded CCC/debit-order instruction email. Logs activity tracker. |
  | **Not Supported — Tenant** | → `AwaitingLeaseAgreement` | ✅ `EHCRoundRobin(LeaseAgreementValidation=true)` | Clears all signatures. Saves rejection comment. Logs activity tracker. |
  | **Not Supported — CSO** | → `AwaitingTenantUpdateDetails` | ✅ `EHCRoundRobin(UpdateTenantDetails=true)` | Clears all signatures. Saves rejection comment. Logs activity tracker. |
  | **Rejected (Legacy)** | → `AwaitingTenantUpdateDetails` | ✅ `EHCRoundRobin(UpdateTenantDetails=true)` | No signature clear. Uses `AssessmentFiguresPOPRejected` activity message (legacy). |

  **Lease Activation Detail (Approved path):**
  - `LeaseDetails.Completed = true` — marks lease as concluded for the renewal backdater tool.
  - `LeaseDetails.IsActive = true` — flags the lease as the live active lease for this application.
  - `LeaseDetails.StartDate` — preserved if CSO already set it; defaults to `DateTime.Now` if null.
  - `PropertyLeaseAgreementMaster.PropertyManagerSigned = true` + `PropertyManagerSignatureDate = DateTime.Now`.
  - Application status → `l_active_lease` (`StatusKeys.ActiveLease`).

  **Email sent on Approval:**
  - **Subject:** `Lease Agreement Approved – Next Steps for Billing Account & Debit Order`
  - **To:** Tenant (`rcsApps.PurEmail` / `SystemUser.EmailAddress`)
  - **Body:** Hardcoded HTML in controller (dynamic: tenant first name). Instructs tenant to visit nearest CCC to open billing account and complete debit order mandate.
  - **Sent via:** `EmailHelper.CustomerEmailNotification(...)` using `EmailContentKeys.plm_awaiting_debit_order` as the base template + hardcoded `appendedBody`.

  > [!NOTE]
  > The future deposit/debit-order routing stubs are commented directly in the controller for both Company and Natural Person paths, ready to be wired up when needed.

---

## ⚙️ CORE ENGINES & METHODS (DO NOT GUESS)

When writing logic, ALWAYS use these predefined centralized methods. **Do not write ad-hoc SQL or custom loops for these tasks.**

### 1. `EHCWorkflowEngine.EHCRoundRobin(...)`
**Location:** `Helpers/EHCWorkflowEngine.cs`
**Purpose:** The central nervous system for routing applications between human actors.
**Usage:** Call this whenever an application needs to move to the next step. Pass `true` for the specific boolean flag representing the target stage (e.g., `RiskAssessment = true`).

### 2. `WorkAllocationHumanHelper.FinishAllPreviousWork(...)`
**Location:** `Helpers/WorkAllocationHumanHelper.cs`
**Purpose:** Archives existing `RoundRobinQueues` tasks before creating a new one.
**Usage:** **CRITICAL:** You must pass the `ResponsibilityTypeId` to ensure you only archive the task for the specific workflow you are finishing. If you omit this, you will wipe out parallel workflows (like maintenance).

### 3. `MatchingHelper.RoundRobinMarkJobAsFinished(...)`
**Location:** `Helpers/MatchingHelper.cs`
**Purpose:** A safer, more granular method to finish a specific user's task.
**Usage:** Marks a job as done by looking at `ApplicationId`, `ResponsibilityTypeId`, and `ClerkId`.
**CRITICAL BUG PREVENTION:** Always pass `Customer.Id` (the ID of the logged-in user actually performing the action). Do **not** fetch a user ID based on a role (e.g., `GetBackOfficeId()` for the Letting Officer) unless that specific role is the one guaranteed to be performing the action. Passing the wrong ID leaves the active user's task stuck in an open state, causing the application to persist on their dashboard incorrectly.

### 4. `MatchingHelper.AddHistoryLog(...)`
**Location:** `Helpers/MatchingHelper.cs`
**Purpose:** Records all significant user actions, rejections, and approvals to the audit trail.
**Usage:** `MatchingHelper.AddHistoryLog(db, applicationId, clerkId, "Action description here.");`
**Target Table:** `PLMApplicationHistortyLogs`

### 5. `MatchingHelper.RejectMatchedUnit(...)`
**Location:** `Helpers/MatchingHelper.cs`
**Purpose:** Handles the "Two Strikes" unit rejection logic.
**Usage:** Called when a user rejects a unit offer. Automatically verifies if it is the first or second rejection via `VerifrySecondPropertyReject()`.

### 6. `ClearLeaseSignatures(context, applicationId)` *(UC022)*
**Location:** `Controllers/PropertyLeaseApplicationController.cs` (private helper)
**Purpose:** Globally wipes ALL digital signature fields on the latest `PropertyLeaseAgreementMaster` record for a given application.
**Fields Cleared:** `TenantSignature`, `WitnessSignature1`, `PropertyManagerSignature`, `RevenueManagerSignature`, `TenantSignatureDate`, `WitnessSignatureDate1`, `PropertyManagerSignatureDate`, `RevenueManagerSignatureDate`, plus the boolean flags `PropertyManagerSigned` and `RevenueManagerSigned`.
**Usage:** Must be called on **every** rejection path (both Reject-to-Tenant and Reject-to-CSO) to ensure legal integrity of the agreement. Do not selectively clear signatures.

### 7. `SaveLeaseRejectionComment(context, applicationId, comment, rejectedBy, rejectedTarget)` *(UC022)*
**Location:** `Controllers/PropertyLeaseApplicationController.cs` (private helper)
**Purpose:** Persists a structured rejection comment to `PropertyLeaseActionComments` with metadata about who rejected and who the target is.
**Usage:** Always call alongside `MatchingHelper.AddCommentOnRejectAgreement()` — the latter writes the raw comment, this method adds the structured "Not Supported" wrapper.

### 8. `MatchingHelper.AddCommentOnRejectAgreement(context, comment, applicationId)`
**Location:** `Helpers/MatchingHelper.cs`
**Purpose:** Writes a rejection reason comment to `PropertyLeaseActionComments.RejectReason` for the given application.
**Usage:** Used in lease agreement rejection flows. The tenant-facing `LeaseAgreementValidation.cshtml` view reads the latest `RejectReason` containing "Not Supported" to display the rejection banner.

### 9. BR09 Unit Offer Expiry Test Script
**Location:** `SQL Server / Database`
**Purpose:** Creates a perfectly configured, 31-day overdue mock application assigned to a random unit, which safely copies dependencies from the latest successful application to avoid FK constraint errors.
**Usage:** Run this in a testing or Prod database to generate an application that can be instantly tested using `TestingToolsController.RunUnitOfferChecker()` (via `/TestingTools/UnitOfferExpiry`).
```sql
BEGIN TRAN;

-- 1. Fetch the exact ID for the 'Available Unit Matched' status using its system Key
DECLARE @TargetStatusId INT = (SELECT TOP 1 Id FROM Status WHERE [Key] = 's_rcs_awaited');

-- 2. Get valid foreign keys (INCLUDING DepartmentId) from the most recent application
DECLARE @SourceAppId INT = (SELECT TOP 1 Id FROM PropertyLeaseApplications WHERE IsDeleted = 0 AND CustomerId IS NOT NULL AND PreferredComplexAreaId IS NOT NULL ORDER BY Id DESC);
DECLARE @CustomerId INT, @SystemUserId INT, @Complex1 INT, @Complex2 INT, @HumanEHC INT, @Income INT, @Title INT, @IdType INT, @SecIncome INT, @DeptId INT;

SELECT 
    @CustomerId = CustomerId, @SystemUserId = SystemUserId, 
    @Complex1 = PreferredComplexAreaId, @Complex2 = PreferredComplexArea2Id,
    @HumanEHC = HumanEHCOptionsId, @Income = IncomeSourceId,
    @Title = TitleTypeId, @IdType = IdentificationTypeId, @SecIncome = SecAppIncomeSourceId,
    @DeptId = DepartmentId
FROM PropertyLeaseApplications WHERE Id = @SourceAppId;

-- 3. Create the new Application
DECLARE @AppRef NVARCHAR(50) = 'EHC_BR09_TEST_' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(10));

INSERT INTO PropertyLeaseApplications (
    ApplicationReferenceNumber, CustomerId, SystemUserId, CreatedBySystemUserId, StatusId, DepartmentId,
    PreferredComplexAreaId, PreferredComplexArea2Id, HumanEHCOptionsId, IncomeSourceId, SecAppIncomeSourceId, TitleTypeId, IdentificationTypeId,
    CreatedDateTime, IsActive, IsDeleted, FirstName, LastName
)
VALUES (
    @AppRef, ISNULL(@CustomerId, 1), ISNULL(@SystemUserId, 1), ISNULL(@SystemUserId, 1), @TargetStatusId, @DeptId,
    @Complex1, @Complex2, @HumanEHC, @Income, @SecIncome, @Title, @IdType,
    GETDATE(), 1, 0, 'BR09', 'Test User'
);

DECLARE @AppId INT = SCOPE_IDENTITY();

-- Failsafe: Only proceed if the application successfully inserted
IF @AppId IS NOT NULL AND @TargetStatusId IS NOT NULL
BEGIN
    -- 4. Grab a random available unit
    DECLARE @UnitId INT = (SELECT TOP 1 Id FROM ApplicationAllocatedProperties WHERE IsTaken = 0 AND IsDeleted = 0 ORDER BY NEWID());

    UPDATE ApplicationAllocatedProperties SET IsTaken = 1 WHERE Id = @UnitId;

    -- 5. Create the matched unit offer (Backdated 31 days)
    INSERT INTO MatchedUnits (
        PropertyLeaseApplicationId, ApplicationAllocatedPropertyId,
        IsAccepted, RejectedProperty, IsDeleted, IsActive, CreatedDateTime
    )
    VALUES (
        @AppId, @UnitId, 0, 0, 0, 1, DATEADD(day, -31, GETDATE())
    );

    -- 6. Put them in the waiting list queue
    INSERT INTO waitingListQues (
        PropertyLeaseApplicationId, IsMatched, QueueDate, IsActive, IsDeleted, IsReListed, LeaseID
    )
    VALUES (
        @AppId, 1, DATEADD(day, -60, GETDATE()), 1, 0, 0, ISNULL(@AppId, 9999)
    );

    -- Show the result
    SELECT @AppRef AS NewTestApplicationRef, @UnitId AS AllocatedUnitId;
END
ELSE
BEGIN
    SELECT 'ERROR: Application failed to insert or Status Key was missing.' AS Result;
END

COMMIT TRAN;
```

---

## Lease Termination Workflow — UC-29 through UC-35

> [!NOTE]
> **AS-IS BASELINE — Before Amendments.** This section documents the lease termination flow exactly as it exists in the codebase at the time of writing. All proposed amendments must be applied against this baseline and this section must be updated to reflect the amended flow.

---

### UC-29: Tenant Submits Serve Notice to Terminate Lease

**Primary Actor:** Tenant  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `ApplicationLeaseServeNotice(int? id, string refno)` — loads the active lease for the tenant  
**Action (POST):** `ApplicationLeaseServeNotice(vm, int? id, string ApprovalStatusddl, string ServeNoticeDate, string TerminationReason)`  
**View:** `ApplicationLeaseServeNotice.cshtml`  
**Nav entry:** *Notice → Serve Notice* (Tenant menu)

**Status Guards:** The GET blocks access if the lease is in any of:
- `AwaitingRiskAssessment`, `AwaitingRenewalDocuments`, `ApplicationUpForRenewalAtThreeMonths`, `InAwaitingPropertyManagersReview`, `InAwaitingRevenueManagersReview`, `AwaitingTenantAcceptance`

**Dropdown options shown on screen:**
- Letting Officer / Area Manager: `LeaseNotRenuewed`, `EndOfLeasePeriod60M`, `TenantNotice`
- Area Manager only: `Approved`

#### POST Behaviour — UC-29
1. **Saves termination reason** → new `PropertyLeaseActionComments` row:
   - `PropertyLeaseApplicationId`, `RejectReason = TerminationReason`, `LeaseTerminationLetter = true`, `ClerkId = Customer.Id`
2. **Saves notice date** → calls `MatchingHelper.ServeNoticeAppllicationDate()` which saves `ServeNoticeDate` to `PropertyLeaseApplication.ServeNoticeDate`
3. **Saves notice date to lease** → `LeaseDetails.NoticeDate = ServeNoticeDate` → `SaveChanges()`
4. **Sends email** → `EmailHelper.CustomerEmailNotification()` using key `EmailContentKeys.ServeNotice`
5. **Assigns to Letting Officer** → `EHCRoundRobin(..., Terminations: true, ...)` — creates `RoundRobinQueue` entry with `ResponsibilityType = Terminations`
6. **Notifies back office** → `EHCWorkflowEngine.BackOfficeNotification()` with the LO's name
7. **Redirects** → `ServeNotice` (Tenant's serve-notice list view)

**Session set:** `TenantServeNoticeSession`

---

### UC-30: Letting Officer — Capture Lease Termination Details

**Primary Actor:** Letting Officer / Lease Official / Client Services Officer  
**Controller (search POST):** `PropertyLeaseApplicationController.Termination(string ApprovalStatusddl)`  
**Controller (form GET):** `LeaseDetailsController.LeaseTerminationValidation(int? id)`  
**Controller (form POST):** `LeaseDetailsController.LeaseTerminationValidation(vm, int? id, string ApprovalStatusddl)`  
**View (search):** `Termination.cshtml`  
**View (form):** `LeaseTerminationValidation.cshtml`  
**Nav entry:** *Termination → Terminate* (LO / CSO menu)

**Search POST logic (`Termination POST`):**
- Accepts a reference number (PLM- prefix or PLM/EHC application ref)
- Looks up latest `LeaseDetails` where `IsNew = true`
- Guards: if status is `DeactivateLeaseNewCaptured` → error; if status is `TerminationDateIssued` → error
- Redirects to → `LeaseTerminationValidation` (GET) passing encrypted `LeaseDetails.Id`

**Documents shown on the form:** `TerminationLetter`, `ProofOfBankingDetails` (via `MatchingHelper.DocumentTerminationLetterAndProofBanking`)

**Dropdown options on form:**
- `TenantNotice`, `LeaseNotRenuewed`, `TenantDeceased`, `EndOfLeasePeriod60M`
- If `ServeNoticeDate` is not null on the lease: also shows `TenantNotice`

#### POST Behaviour — UC-30 (`LeaseTerminationValidation POST`)
Saves a new **`LeaseTermination`** row with:
- `LeaseDetailsId`, `LeaseReferenceNumber`, `PropertyLeaseApplicationId`
- `TerminationDate = vm.LeaseTermination.TerminationDate`
- `StatusId = AwaitingterminantionApproval`
- `ReasonForTermination` (set by branch below)

**Branch: TenantNotice**
- Status → `LeaseDetails.StatusId = TenantNotice`
- Marks current RoundRobin job as finished (`ResponsibilityType = Terminations`)
- Activity tracker logged

**Branch: LeaseNotRenuewed**
- Status → `LeaseDetails.StatusId = LeaseNotRenewed`
- Activity tracker logged

**Branch: TenantDeceased**
- Status → `LeaseDetails.StatusId = TenantDeceased`
- Activity tracker logged

**Branch: EndOfLeasePeriod60M**
- Status → `LeaseDetails.StatusId = EndOfLeaseTerm`
- Activity tracker logged

**All branches then:**
- `EHCRoundRobin(..., TerminationValidation: true, ...)` — assigns to **Revenue Officer** via `ResponsibilityType = TerminationValidation`
- `LeaseTerminations.Add(termination)` + `SaveChanges()`
- **Redirect:** Tenant → `Inbox`; LO/CSO → `Termination` list

**Session set:** `LeaseTerminationLOSession`

---

### UC-31: Revenue Officer — Review Lease Termination & Validate Account

**Primary Actor:** Revenue Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `PropertyLeaseApplicationTerminations()` — lists leases  
**Action (POST):** `PropertyEvictionValidation(int? id, string ApprovalStatusddl, string RejectComment)` *(Note: this action name is misleading — it serves as account validation in the termination flow)*  
**View (list):** `PropertyLeaseApplicationTerminations.cshtml`  
**View (detail):** `PropertyEvictionValidation.cshtml`  
**Nav entry:** *Termination → Terminations* (Revenue Officer menu)

**List query — Revenue Officer role:**
- Joins `RoundRobinQueues` where `ResponsibilityTypeId = TerminationValidation` AND `StatusId = Submitted`
- Filters `LeaseDetails` in those queue entries where status is one of:
  `AwaitingTenantAccountBalanceReview`, `EndOfLeaseTerm`, `LeaseNotRenewed`, `TenantNotice`

**Documents shown on form:** `PropertyEvictionValidation` document bucket (via `MatchingHelper.DocumentPropertyEvictionValidation`)

#### POST Behaviour — UC-31 (`PropertyEvictionValidation POST`)
Saves a new `PropertyLeaseActionComments` row with `PropertyEvictionValidation = true`, `RejectReason`.

**Branch: Approved**
- Status → `LeaseDetails.StatusId = AwaitingCommitteEviction`
- Activity tracker logged (`ProopertyEvictionAprove`)
- Redirects → `PropertyLeaseApplicationTerminations`

**Branch: Rejected**
- Status → `LeaseDetails.StatusId = AwaitingCommitteEviction` *(same as Approved — this appears to be a bug in the current code, both branches set the same status)*
- Activity tracker logged
- Redirects → `PropertyLeaseApplicationTerminations`

> [!WARNING]
> **Known Code Issue:** In the current `PropertyEvictionValidation POST`, the `Rejected` branch sets the lease to `AwaitingCommitteEviction` — identical to `Approved`. This means rejection currently does not retain status or prevent progression. **This is flagged for amendment.**

---

### UC-32: Housing Supervisor — Conduct Exit Unit Inspection

**Primary Actor:** Housing Supervisor  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `ConductExitInspection(int? id)` (line 1617)  
**Action (POST):** `ConductExitInspection(int? id, string ApprovalStatusddl, string UnitInspectionComment)` (line 1788)  
**View:** `ConductExitInspection.cshtml`  
**Nav entry:** *Inspections → Inspection* (Housing Supervisor)  
**Prerequisite status:** `AwaitingExitInspection`

**Documents shown:** Exit inspection documents + photos

#### POST Behaviour — UC-32 (`ConductExitInspection POST`)
Always saves a **`ConductUnitInspection`** row with `PropertyLeaseApplicationId`, `InspectionComment`.

**Branch: Habitable (no defects)**
- `EHCRoundRobin(..., VacatingConfirmation: true, ...)` — assigns to **Housing Supervisor** for vacating confirmation
- Status → `PropertyLeaseApplication = AwaitingVacatingConfirm`
- Status → `LeaseDetails = AwaitingVacatingConfirm`
- Marks inspection `RoundRobinQueue` job as finished (`ResponsibilityType = Inspections`)
- Activity tracker logged (`ConductExitInspectionApprove`)

**Branch: Habitable – Minor Defects**
- Same as Habitable: `EHCRoundRobin` assigns VacatingConfirmation
- Status → `AwaitingVacatingConfirm` (both Application and LeaseDetails)
- Marks inspection job finished
- Activity tracker logged (`ConductExitInspectionReject`)

**Branch: Not Habitable**
- `EHCRoundRobin(..., UnitMaintenance: true, ...)` — assigns to **maintenance** queue
- Sets `ApplicationAllocatedProperty.Inspection = true`
- Creates new **`AllocatedUnitMaintenanceEHC`** row: `PropertyLeaseApplicationId`, `LeaseDetailsId`, `ApplicationAllocatedPropertyId`, `RCSActionTypeId = NotHabitable`, `StatusId = Submitted`, `InspectionType = ExitUnitInspection`
- Status → `PropertyLeaseApplication = AwaitingMaintananceJobSheet`
- Activity tracker logged (`ConductExitInspectionReject`)

**All branches redirect to:** `PropertyLeaseInspections`  
**Session set:** `ConductUnitInspectionSession`

---

### UC-33: Letting Officer — Capture Eviction Details

**Primary Actor:** Letting Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `CaptureEvictionDetails(int? id)` (line 6895)  
**Action (POST):** `CaptureEvictionDetails(int? id, string ApprovalStatusddl)` (line 7026)  
**View:** `CaptureEvictionDetails.cshtml` (routed via `ApplicationEviction` search page)  
**Nav entry:** *Eviction → Capture Eviction* (LO menu)  
**Prerequisite:** Lease must be in eviction-eligible status

**Documents shown on form:** `PropertyEvictionValidation` document bucket

#### POST Behaviour — UC-33 (`CaptureEvictionDetails POST`)
Branches on the reason type selected:

| Reason | Status applied to `PropertyLeaseApplication` |
|--------|----------------------------------------------|
| NonPayment | `NonPayment` |
| NonCompliance | `NonCompliance` |
| Subletting | `Subletting` |
| IllegalActivities | `IllegalActivities` |

Then for all branches:
- `EHCRoundRobin(..., CommitteeOutcomes: true, ...)` — assigns to Letting Officer for **committee outcome capture** (position 13)
- Sends email → `EmailHelper.CustomerEmailNotification()` using key `EmailContentKeys.EvictionCapture`
- Activity tracker logged (`EvictionCapture`)
- **Redirects** → `ApplicationEviction` list

**Session set:** `EvictionDetailsSession`

---

### UC-34: Letting Officer — Capture Eviction Committee Outcome

**Primary Actor:** Letting Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `EvictionCommitteeOutcome(int? id)` (line 13054)  
**Action (POST):** `EvictionCommitteeOutcome(vm, int? id, string ApprovalStatusddl, ...)` (line 13245)  
**View:** `EvictionCommitteeOutcome.cshtml`  
**Nav entry:** *Eviction → Capture Committee Outcome*  
**Prerequisite status:** `AwaitingCommitteEviction`

**Documents shown:** `EvictionCommitteeOutcome` document bucket

#### POST Behaviour — UC-34 (`EvictionCommitteeOutcome POST`)
Always saves:
- New `PropertyLeaseActionComments` row (`PropertyEvictionValidation = true`, `RejectReason`)
- New **`CommitteeOutcome`** row: `FirstName`, `Surname`, `Reason`, `OfficialNumber`, `PropertyLeaseApplicationId`, `LeaseDetailsId`, `DateStamp`

Then looks up `LeaseTermination` for this `LeaseDetailsId`.

**Branch: Approved**
- Sets `LeaseDetails.EndDate = terminate.TerminationDate`
- Sets `LeaseDetails.RenewalNotice = TerminationDate – 3 months`
- Updates `LeaseTermination.StatusId = TerminationDateIssued`
- Status → `LeaseDetails = AwaitingVacatingConfirm`
- Activity tracker logged (`EvictionCommitteeActionsTermination`)
- Redirects → `PropertyLeaseApplicationTerminations`

**Branch: Rejected**
- Updates `LeaseTermination.StatusId = TerminationRejected`
- Status → `LeaseDetails = TerminationRejected`
- Activity tracker logged
- Redirects → `PropertyLeaseApplicationTerminations`

---

### UC-35: Housing Supervisor — Confirm Tenant Vacated

**Primary Actor:** Housing Supervisor  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `ConfirmVacatingAppicant(int? id)` (line 13328)  
**Action (POST):** `ConfirmVacatingAppicant(vm, int? id, string ApprovalStatusddl)` (line 13417)  
**View:** `ConfirmVacatingAppicant.cshtml`  
**Nav entry:** *Termination → Terminated Leases* → "Confirm Move-Out" button  
**Prerequisite status:** `AwaitingVacatingConfirm`  
**Dropdown options:** `Vacated`, `NotVacated`

**On load:** Displays `LeaseDetails`, `LeaseTermination` (termination date, reason). Loads system user's name for the committee signature fields.

#### POST Behaviour — UC-35 (`ConfirmVacatingAppicant POST`)
Both branches first:
- Mark the `VacatingConfirmation` `RoundRobinQueue` job as finished (`MatchingHelper.RoundRobinMarkJobAsFinished`)

**Branch: Vacated**
- Status → `LeaseDetails = ApplicantVacated`
- Marks RoundRobin job finished (called again — belt and braces)
- Gets `ApplicationAllocatedProperty` via `ApplicantUnits → MatchedUnits → ApplicationAllocatedProperty`
- **Sets `ApplicationAllocatedProperty.IsTaken = false`** — unit is now available for re-allocation
- Activity tracker logged (`EvictionCommitteeActionsTermination`)
- `EHCRoundRobin(..., AcknowlegeRefund: true, ...)` — triggers refund acknowledgement step
- **Redirects** → `LeaseTerminated` (LeaseDetails view)
- **Session set:** `ConfirmVacatingAppicantSession`

**Branch: NotVacated**
- Status → `LeaseDetails = ApplicantNotVacated`
- Rental charges continue (no IsTaken flip, no further workflow step)
- **Redirects** → `LeaseTerminated`
- **Session set:** `ConfirmVacatingAppicantSession`

---

## Lease Termination & Eviction Workflow — UC023 + UC024 (NEW)

> [!IMPORTANT]
> **AMENDED FLOW — UC023/UC024 replace the old UC-29 through UC-34 baseline.** The AS-IS sections above (UC-29 through UC-35) are retained for reference. The active code now follows the UC023/UC024 flows documented below. Key differences: (1) CSO review gate added, (2) Date validation enforced, (3) Revenue Manager approve/reject bug fixed, (4) CEO replaces eviction committee.

---

### 🎭 Playwright E2E Test Automation (UC023)
- **Status:** ✅ Playwright Ready & Verified (Voluntary Termination Flow)
- **Spec File:** [UC023_UC025_Termination_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC023_UC025_Termination_Eviction_Workflow.spec.js)
- **Execution Command (PowerShell):**
  ```powershell
  $env:PATH = "C:\Users\sashe\.gemini\antigravity-ide\scratch\node\node-v20.11.0-win-x64;" + $env:PATH; npx playwright test Tests/Playwright/UC023_UC025_Termination_Eviction_Workflow.spec.js --headed
  ```
- **Recorded Video Walkthrough:** [uc023_termination_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc023_termination_demo.webm)

---

### UC023-S1: Tenant Submits Serve Notice to Terminate Lease (AMENDED UC-29)

**Primary Actor:** Tenant  
**Controller:** `PropertyLeaseApplicationController`  
**Action (POST):** `ApplicationLeaseServeNotice(vm, int? id, string ApprovalStatusddl, string ServeNoticeDate, string TerminationReason)`  
**View:** `ApplicationLeaseServeNotice.cshtml`  
**Nav entry:** *Notice → Serve Notice* (Tenant menu)

#### POST Behaviour — UC023-S1

**Validation (NEW — BR29/BR30):**
1. Parses `ServeNoticeDate` — if invalid → error redirect
2. **BR29:** Termination date must be ≥ 1 calendar month from today — if not → error with earliest allowed date
3. **BR30:** Termination date must be last day of the month — if not → error suggesting correct date

**On successful validation:**
1. Saves `PropertyLeaseActionComments` (same as UC-29)
2. Saves `ServeNoticeDate` via `MatchingHelper.ServeNoticeAppllicationDate()`
3. Saves `LeaseDetails.NoticeDate`
4. **NEW:** Sets status → `AwaitingCSOTerminationReview` (`s_awaiting_cso_termination_review`)
5. Sends email via `EmailContentKeys.ServeNotice`
6. Assigns to CSO via `EHCRoundRobin(..., Terminations: true, ...)`
7. Redirects → `ServeNotice`

**Key difference from UC-29:** Status now goes to `AwaitingCSOTerminationReview` (was no specific status). Notification goes to CSO (was LO).

---

### UC023-S2a: CSO Reviews Tenant Notice (ENTIRELY NEW)

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `TerminationCSOReview()` — lists notices with status `AwaitingCSOTerminationReview`  
**Action (detail GET):** `TerminationCSOReviewDetails(int? id)`  
**Action (POST):** `TerminationCSOReviewDetails(int? id, string ApprovalStatusddl, string OfficialNumber, string RejectComment)`  
**View (list):** `TerminationCSOReview.cshtml`  
**View (detail):** `TerminationCSOReviewDetails.cshtml`  
**Nav entry:** *Termination → Termination Reviews* (CSO menu)  
**Prerequisite status:** `AwaitingCSOTerminationReview`

#### POST Behaviour — UC023-S2a

**Branch: Supported**
- Saves `PropertyLeaseActionComments` with Official Number
- Status → `AwaitingTerminationAppraisal` (`s_awaiting_termination_appraisal`)
- `EHCRoundRobin(..., TerminationValidation: true, ...)` — routes to Revenue Manager
- Activity tracker: `at_termination_cso_supported`
- Email notification to Tenant
- Redirects → `TerminationCSOReview`

**Branch: Not Supported**
- Saves `PropertyLeaseActionComments` with Official Number and Reason
- Status → `TerminationNotSupported` (`s_termination_not_supported`)
- Activity tracker: `at_termination_cso_not_supported`
- Email notification to Tenant with reason
- Redirects → `TerminationCSOReview`

> [!NOTE]
> This is a **hard gate** — the tenant's termination notice cannot progress to the Revenue Manager without CSO approval.

**Status:** 🔨 TO BE IMPLEMENTED (WP3)

---

### UC023-S2b: CSO Initiates Termination Directly (AMENDED UC-30)

**Primary Actor:** Client Services Officer  
**Controller (search POST):** `PropertyLeaseApplicationController.Termination(string ApprovalStatusddl)`  
**Controller (form GET):** `LeaseDetailsController.LeaseTerminationValidation(int? id)`  
**Controller (form POST):** `LeaseDetailsController.LeaseTerminationValidation(vm, int? id, string ApprovalStatusddl)`  
**View:** `LeaseTerminationValidation.cshtml`  
**Nav entry:** *Termination → Terminate* (CSO menu)

**Expanded fields (NEW):**
- Reason (dropdown — TenantNotice, LeaseNotRenuewed, TenantDeceased, EndOfLeasePeriod60M)
- Effective Date (with BR29/BR30 validation)
- Clause Reference (text)
- Notice Period (text)
- Official Number

#### POST Behaviour — UC023-S2b (same as UC-30 but with expanded data capture)
- Creates `LeaseTermination` row with all fields
- Status → `AwaitingTerminationAppraisal` (was `AwaitingterminantionApproval`)
- `EHCRoundRobin(..., TerminationValidation: true, ...)` — routes to Revenue Manager
- Activity tracker: `at_termination_cso_initiated`
- Redirects → `Termination` list

**Status:** 🔨 TO BE IMPLEMENTED (WP4)

---

### UC023-S3: Revenue Manager Authorization (AMENDED UC-31 — BUG FIXED & VOLUNTARY FLOW ADDED)

**Primary Actor:** Revenue Manager  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `PropertyEvictionValidation(int? id)` (GET) — Loads context. If the lease status is `AwaitingTerminationAppraisal`, the page adapts to the **Voluntary Lease Termination Appraisal** dashboard. It queries the tenant's uploaded Proof of Banking details document via `MatchingHelper.DocumentUploadBakingDetailsProof` and stores it in `ViewBag.BankingDetailsDvm`.  
**Action (POST):** `PropertyEvictionValidation(int? id, string ApprovalStatusddl, string RejectComment, string OfficialNumber, string hdnSignatureBlob)`  
**View (list):** `PropertyLeaseApplicationTerminations.cshtml`  
**View (detail):** `PropertyEvictionValidation.cshtml`  
**Nav entry:** *Termination → Terminations* (Revenue Manager menu)

**Expanded UI elements (NEW):**
- **Account Validation button:** Toggles dynamic visibility of the tenant's read-only banking proof document panel.
- **Digital Signature canvas:** Captures the RM's signature.
- **Official Number field:** Captured and persisted.
- **Outcome Options:** Approved / Rejected / Referral to Legal.

#### POST Behaviour — UC023-S3 (`PropertyEvictionValidation POST`)

Saves a structured log in `PropertyLeaseActionComments`. If the lease is in a **Voluntary Termination** status (`AwaitingTerminationAppraisal`):
* Saves `RevenueManagerSignature` (base64 image blob), `RevenueManagerOfficialNumber`, and `RevenueManagerSignDate` directly to the `LeaseTermination` entity.
* **Approved:** Status transitions to `s_awaiting_exit_inspec` (`In Awaiting Exit Inspection`, ID 255) for the housing supervisor exit inspection.
* **Rejected:** Status transitions back to `s_awaiting_termination_approval` (CSO review, ID 248) and requires a comment.
* **Referral to Legal:** Status transitions to `s_legal_referral_pending` and generates standard eviction details.

If the lease is in a **Standard Eviction Validation** status:
* **Approved:** Status transitions to `s_awaiting_eviction_service` (`Awaiting Eviction Service`, ID 253) to serve the eviction notice.
* **Rejected:** Status transitions back to `s_awaiting_termination_approval` (CSO review, ID 248).
* **Referral to Legal:** Status transitions to `s_legal_referral_pending` (ID 249).

> [!WARNING]
> The old code had a **critical bug** where both Approved and Rejected set the status to `AwaitingCommitteEviction`. This has been fixed — Approved goes to `s_awaiting_exit_inspec` (voluntary) or `s_awaiting_eviction_service` (eviction), while Rejected returns safely to the CSO at `s_awaiting_termination_approval`.

**Status:** ✅ Fully IMPLEMENTED & VERIFIED via E2E automated test browser.
* **E2E Playwright Specs:**
  - **Voluntary Path:** [UC023_UC025_Termination_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC023_UC025_Termination_Eviction_Workflow.spec.js)
  - **Eviction / Legal Referral Path:** [UC024_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC024_Eviction_Workflow.spec.js)
* **Recorded Video Demos:**
  - **Voluntary Termination (UC023):** [uc023_termination_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc023_termination_demo.webm)
  - **Legal Referral & Eviction (UC024):** [uc024_eviction_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc024_eviction_demo.webm)

---

### 🎭 Playwright E2E Test Automation (UC024)
- **Status:** ✅ Playwright Ready & Verified (Legal Referral & Eviction Flow)
- **Spec File:** [UC024_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC024_Eviction_Workflow.spec.js)
- **Execution Command (PowerShell):**
  ```powershell
  $env:PATH = "C:\Users\sashe\.gemini\antigravity-ide\scratch\node\node-v20.11.0-win-x64;" + $env:PATH; npx playwright test Tests/Playwright/UC024_Eviction_Workflow.spec.js --headed
  ```
- **Recorded Video Walkthrough:** [uc024_eviction_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc024_eviction_demo.webm)

---

### UC024-S1: CSO Captures Eviction Outcome (AMENDED UC-33)

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `CaptureEvictionDetails(int? id)` (line 6907)  
**Action (POST):** `CaptureEvictionDetails(int? id, string ApprovalStatusddl)` (line 7038)  
**View:** `CaptureEvictionDetails.cshtml`  
**Nav entry:** *Eviction → Capture Eviction* (CSO menu)

**Expanded fields (NEW):**
- Official Number
- Outcome Type (dropdown)
- Date
- Summary (textarea)
- Generate Eviction Notice button → dummy PDF from template

#### POST Behaviour — UC024-S1 (same structure as UC-33 but with expanded capture)
- Sets eviction type status (NonPayment / NonCompliance / Subletting / IllegalActivities)
- **NEW:** Generates Eviction Notice PDF from template
- `EHCRoundRobin(..., CommitteeOutcomes: true, ...)` — assigns to CEO (was LO)
- Email notification: `EvictionCapture` to tenant + notify CEO
- Activity tracker: `at_eviction_notice_generated`
- Redirects → `ApplicationEviction` list

**Status:** ✅ Fully IMPLEMENTED & VERIFIED via E2E automated test browser.
* **E2E Playwright Spec:** [UC024_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC024_Eviction_Workflow.spec.js)
* **Recorded Video Demo (Eviction UC024):** [uc024_eviction_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc024_eviction_demo.webm)

---

### UC024-S2: CEO Authorization (AMENDED UC-34 — REPLACES COMMITTEE)

**Primary Actor:** CEO / Property Manager  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `EvictionCommitteeOutcome(int? id)` (line 13091)  
**Action (POST):** `EvictionCommitteeOutcome(vm, int? id, string ApprovalStatusddl, ...)` (line 13282)  
**View:** `EvictionCommitteeOutcome.cshtml` → renamed/repurposed to `EvictionCEOAuthorization.cshtml`  
**Nav entry:** *Eviction → CEO Authorization* (CEO/PM menu)  
**Prerequisite status:** `AwaitingEvictionCEOAuth` (was `AwaitingCommitteEviction`)

**Expanded UI (NEW):**
- Removes committee FirstName/Surname fields
- Adds: Official Number, Authorize (Approve/Reject), Reason
- Adds: Digital Signature pad

#### POST Behaviour — UC024-S2 (REPLACES UC-34)

Saves `PropertyLeaseActionComments` with `PropertyEvictionValidation = true`.
Keeps `CommitteeOutcome` entity for backward compat but populated with CEO data.

**Branch: Approved**
- Sets `LeaseDetails.EndDate = terminate.TerminationDate`
- Sets `LeaseDetails.RenewalNotice = TerminationDate – 3 months`
- Updates `LeaseTermination.StatusId = TerminationDateIssued`
- Status → `AwaitingEvictionService` (`s_awaiting_eviction_service`) — routes to UC025 Serve Eviction Notice
- CEO signature + date recorded
- Notifies: Tenant + CSO + Legal
- Activity tracker: `at_eviction_ceo_approved`
- Redirects → `PropertyLeaseApplicationTerminations`

**Branch: Rejected**
- Updates `LeaseTermination.StatusId = TerminationRejected`
- Status → `AwaitingterminantionApproval` (back to CSO)
- Notifies: CSO
- Activity tracker: `at_eviction_ceo_rejected`
- Redirects → `PropertyLeaseApplicationTerminations`

**Status:** ✅ Fully IMPLEMENTED & VERIFIED via E2E automated test browser.
* **E2E Playwright Spec:** [UC024_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC024_Eviction_Workflow.spec.js)
* **Recorded Video Demo (Eviction UC024):** [uc024_eviction_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc024_eviction_demo.webm)

---

### UC025: Serve Eviction Notice & Capture Proof of Service ✅ IMPLEMENTED

> [!NOTE]
> **Implemented in Phase 3.** CSO serves the eviction notice and captures proof of service before the case proceeds to exit inspection.

### 🎭 Playwright E2E Test Automation (UC025)
- **Status:** ✅ Playwright Ready & Verified (Eviction Service & Proof Capture Flow)
- **Spec Files:**
  - **Notice Service & Proof Capture Standalone:** [UC025_Serve_Notice_Eviction.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC025_Serve_Notice_Eviction.spec.js)
  - **Full Integrated Flow:** [UC023_UC025_Termination_Eviction_Workflow.spec.js](file:///C:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Tests/Playwright/UC023_UC025_Termination_Eviction_Workflow.spec.js)
- **Execution Command (PowerShell):**
  ```powershell
  $env:PATH = "C:\Users\sashe\.gemini\antigravity-ide\scratch\node\node-v20.11.0-win-x64;" + $env:PATH; npx playwright test Tests/Playwright/UC025_Serve_Notice_Eviction.spec.js --headed
  ```
- **Recorded Video Walkthrough:** [uc025_eviction_notice_demo.webm](file:///C:/Users/sashe/.gemini/antigravity-ide/brain/acde49bc-895f-4c36-a156-be1e81ddcfc1/uc025_eviction_notice_demo.webm)

**UC025-S1 — Serve Notice:**

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `ServeEvictionNotice(int? id)` — loads lease + termination context  
**Action (POST):** `ServeEvictionNotice(int? id, string ApprovalStatusddl, string ServiceMethod, string ServiceDate, string OfficialNumber)`  
**View:** `ServeEvictionNotice.cshtml`  
**Nav entry:** *Termination → Terminations* → "Serve Eviction Notice" button (yellow)  
**Prerequisite status:** `AwaitingEvictionService` (`s_awaiting_eviction_service`)

#### POST Behaviour — UC025-S1
1. Creates `EvictionServiceRecord` row: ServiceMethod, ServiceDate, OfficialNumber, NoticeServed=true
2. Generates `EvictionReferenceNumber` via `MatchingHelper.GenerateTerminationReference()`
3. Status → `EvictionNoticeServed` (`s_eviction_notice_served`)
4. Activity tracker: `at_eviction_notice_served`
5. Email notification to Tenant
6. Redirects → `PropertyLeaseApplicationTerminations`

---

**UC025-S2 — Capture Proof of Service:**

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `CaptureProofOfService(int? id)` — loads lease + service record  
**Action (POST):** `CaptureProofOfService(int? id, string ProofOfServiceType, string ProofServiceDate, string ProofComments)`  
**View:** `CaptureProofOfService.cshtml`  
**Nav entry:** *Termination → Terminations* → "Capture Proof of Service" button (blue)  
**Prerequisite status:** `EvictionNoticeServed` (`s_eviction_notice_served`)

#### POST Behaviour — UC025-S2
1. Updates existing `EvictionServiceRecord`: ProofOfServiceType, ProofServiceDate, ProofComments, ProofCaptured=true
2. Status → `AwaitingExitInspection` (`s_awaiting_exit_inspec`)
3. Activity tracker: `at_proof_of_service_captured`
4. Redirects → `PropertyLeaseApplicationTerminations`

**Dropdown options — Service Method (UC025-S1):**
- Sheriff of the Court
- Hand Delivery
- Registered Post
- Email Service

**Dropdown options — Proof Type (UC025-S2):**
- Sheriff return of service
- Signed acknowledgement
- Registered mail tracking
- Email read receipt
- Failed attempt affidavit

---

### UC026: Manage Disputes ✅ IMPLEMENTED

> [!NOTE]
> **Implemented in Phase 3.** Full dispute lifecycle management with dedicated `LeaseDispute` entity, reference numbers (`EHC_DISP_###_YYYY`), and a 4-step workflow (Register → Review → Resolve → Close).

---

**UC26A — Register Dispute:**

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET):** `RegisterLeaseDispute(int? id)` — loads lease context  
**Action (POST):** `RegisterLeaseDispute(int? id, string Category, string SubCategory, string Description, string ReportedByName, string ContactNumber, string EmailAddress)`  
**View:** `RegisterLeaseDispute.cshtml`  
**Status set:** `DisputeOpenAwaitingReview` (`s_dispute_open_awaiting_review`)

**Dropdown options — Category:**
- Financial & Billing | Compliance | Maintenance | Tenure | Notices | Administrative

---

**UC26B — Review Dispute:**

**Primary Actor:** Client Services Officer  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET/POST):** `ReviewLeaseDispute(int? id, ...)`  
**View:** `ReviewLeaseDispute.cshtml`  
**Prerequisite status:** `DisputeOpenAwaitingReview`  
**Status set:** `DisputeReferred` (`s_dispute_referred`)

**Fields captured:** Risk Classification (Low/Medium/High), Official Number, Review Comment

---

**UC26C-S1 — Revenue Manager Resolution:**

**Primary Actor:** Revenue Manager  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET/POST):** `ResolveLeaseDispute(int? id, ...)`  
**View:** `ResolveLeaseDispute.cshtml`  
**Prerequisite status:** `DisputeReferred`

**Branch: Resolved**
- Resolution outcome type (7 options: Mutual settlement, Lease amendment, Billing correction, Compliance directive, Maintenance resolution, Formal mediation, Withdrawal by complainant)
- Status → `DisputeResolved` (`s_dispute_resolved`)
- Activity tracker: `at_dispute_resolved`

**Branch: Not Resolved**
- Reason captured
- Status → `DisputeReferred` (returned to review)
- Activity tracker: `at_dispute_not_resolved`

---

**UC26C-S2 — CEO Closure:**

**Primary Actor:** CEO / Property Manager  
**Controller:** `PropertyLeaseApplicationController`  
**Action (GET/POST):** `CloseLeaseDispute(int? id, ...)`  
**View:** `CloseLeaseDispute.cshtml`  
**Prerequisite status:** `DisputeResolved`

**Branch: Close Dispute**
- Closure outcome (5 options: Upheld in favour of tenant, Upheld in favour of EHC, Mutual agreement, Withdrawn by tenant, External legal referral)
- CEO signature + date recorded
- Status → `DisputeClosed` (`s_dispute_closed`)
- Activity tracker: `at_dispute_closed_ceo`

**Branch: Reject / Return**
- Rejection type (3 options: Return for rework, Insufficient evidence, Requires legal opinion)
- Status → `DisputeReferred` (returned to Revenue Manager)
- Activity tracker: `at_dispute_rejected_ceo`

**Dispute Dashboard:** `LeaseDisputes.cshtml` — lists all disputes with DataTable, status badges (colour-coded), and action buttons driven by status.

---

### Termination Flow — AMENDED Status Transition Map

```
TENANT SERVES NOTICE (UC023-S1 — AMENDED)
    Tenant → ApplicationLeaseServeNotice POST
        *** BR29: Date must be ≥1 month from today
        *** BR30: Date must be last day of month
        LeaseDetails.NoticeDate saved
        Status → AwaitingCSOTerminationReview  ← NEW
        RoundRobin: Terminations → assigned to CSO (was LO)
        Email: ServeNotice sent to tenant
        ↓
CSO REVIEWS NOTICE (UC023-S2a — NEW)
    CSO → TerminationCSOReviewDetails POST
        Supported:
            Status → AwaitingTerminationAppraisal  ← NEW
            RoundRobin: TerminationValidation → Revenue Manager
        Not Supported:
            Status → TerminationNotSupported  ← NEW
            (flow ends)
        ↓
CSO INITIATES DIRECTLY (UC023-S2b — AMENDED UC-30)
    CSO → LeaseTerminationValidation POST
        LeaseTermination row created (expanded fields)
        Status → AwaitingTerminationAppraisal  ← NEW
        RoundRobin: TerminationValidation → Revenue Manager
        ↓
REVENUE MANAGER AUTHORIZATION (UC023-S3 — FIXED)
    RevMgr → PropertyEvictionValidation POST
        Approved:
            Status → AwaitingEvictionService  ← FIXED (routes to UC025)
            → UC025 Serve Notice
        Rejected:
            Status → AwaitingterminantionApproval  ← FIXED (was AwaitingCommitteEviction)
            → back to CSO
        Referral:  ← IMPLEMENTED
            Generates Eviction Ref#
            Status → LegalReferralPending
            → UC024 (Eviction path)
        ↓
[EVICTION PATH]
CSO CAPTURES EVICTION OUTCOME (UC024-S1 — AMENDED UC-33)
    CSO → CaptureEvictionDetails POST
        Generates Eviction Notice PDF  ← NEW
        Notifies CEO  ← NEW (was LO)
        ↓
CEO AUTHORIZATION (UC024-S2 — REPLACES UC-34 COMMITTEE)
    CEO → EvictionCEOAuthorization POST (repurposed)
        Approved + Signature:
            Status → AwaitingEvictionService  ← routes to UC025
            Notifies Tenant + CSO + Legal
        Rejected:
            Status → AwaitingterminantionApproval (back to CSO)
        ↓
SERVE EVICTION NOTICE (UC025-S1 — NEW)
    CSO → ServeEvictionNotice POST
        Select method (Sheriff / Hand / Post / Email)
        Creates EvictionServiceRecord
        Status → EvictionNoticeServed
        ↓
CAPTURE PROOF OF SERVICE (UC025-S2 — NEW)
    CSO → CaptureProofOfService POST
        Record proof type + date + comments
        Status → AwaitingExitInspection
        ↓
EXIT INSPECTION (UC-32 — unchanged)
    → Habitable: AwaitingVacatingConfirm
    → Not Habitable: AwaitingMaintananceJobSheet
        ↓
CONFIRM VACATED (UC-35 — unchanged)
    → Vacated: ApplicantVacated + unit freed
    → Not Vacated: ApplicantNotVacated
```

---

### Key Tables Written During Termination Flow (AMENDED)

| Step | Table | Notes |
|------|-------|-------|
| UC023-S1 Serve Notice | `PropertyLeaseActionComments` | LeaseTerminationLetter=true, reason text |
| UC023-S1 Serve Notice | `PropertyLeaseApplication` | ServeNoticeDate |
| UC023-S1 Serve Notice | `LeaseDetails` | NoticeDate, StatusId → `AwaitingCSOTerminationReview` |
| UC023-S1 Serve Notice | `RoundRobinQueues` | Terminations responsibility → CSO (was LO) |
| UC023-S2a CSO Review | `PropertyLeaseActionComments` | OfficialNumber, Support/NotSupported |
| UC023-S2a CSO Review | `LeaseDetails` | StatusId → `AwaitingTerminationAppraisal` or `TerminationNotSupported` |
| UC023-S2b CSO Initiate | `LeaseTerminations` | TerminationDate, Reason, Status, ClauseRef, NoticePeriod |
| UC023-S2b CSO Initiate | `LeaseDetails` | StatusId → `AwaitingTerminationAppraisal` |
| UC023-S3 RM Auth | `PropertyLeaseActionComments` | PropertyEvictionValidation=true |
| UC023-S3 RM Auth | `LeaseDetails` | StatusId → `AwaitingEvictionService` (approved) or `AwaitingterminantionApproval` (rejected) |
| UC025-S1 Serve Notice | `EvictionServiceRecords` | ServiceMethod, ServiceDate, OfficialNumber, NoticeServed=true |
| UC025-S1 Serve Notice | `LeaseDetails` | StatusId → `EvictionNoticeServed` |
| UC025-S2 Proof of Service | `EvictionServiceRecords` | ProofOfServiceType, ProofServiceDate, ProofComments, ProofCaptured=true |
| UC025-S2 Proof of Service | `LeaseDetails` | StatusId → `AwaitingExitInspection` |
| UC-32 Exit Inspection | `ConductUnitInspections` | Comment |
| UC-32 (Not Habitable) | `AllocatedUnitMaintenanceEHC` | Maintenance job row |
| UC024-S1 Eviction Capture | `PropertyLeaseApplication` | StatusId (eviction reason) |
| UC024-S1 Eviction Capture | `RoundRobinQueues` | CommitteeOutcomes → CEO (was LO) |
| UC024-S2 CEO Auth | `CommitteeOutcomes` | CEO outcome data (repurposed from committee) |
| UC024-S2 CEO Auth | `LeaseTerminations` | StatusId updated |
| UC024-S2 CEO Auth | `LeaseDetails` | EndDate, RenewalNotice, StatusId |
| UC-35 Confirm Vacated | `LeaseDetails` | StatusId → ApplicantVacated/NotVacated |
| UC-35 (Vacated) | `ApplicationAllocatedProperty` | IsTaken = false |
| UC-35 (Vacated) | `RoundRobinQueues` | AcknowlegeRefund step triggered |

---



> [!IMPORTANT]
> All SQL scripts that must be executed on the **production database before go-live** are recorded here and backed up in `Scripts/`. Run these in order on the target DB. Every script is idempotent (safe to re-run). **Keep this list up to date — it is the single source of truth for production DB changes.**

---

### UC027: System Audit Trail (Real Estate & EHC)
- **Roles:** Super Administrators, Back Office System Administrators, Area Managers (filtered by Department)
- **Controllers:** `AuditTrailController.cs`, `AccountController.cs` (hooks), `ApplicationUserRoleController.cs` (hooks)
- **Views:**
  - `ActiveUsers.cshtml` — active users and login history
  - `ActivityLog.cshtml` — all user activities / transactions
  - `AdminActivityLog.cshtml` — admin-specific action log (with Controller/Action details)
  - `PasswordResets.cshtml` — password reset tracking history
  - `RoleModifications.cshtml` — user access role changes (before/after states)
- **Workflow / Instantiation:**
  - Integrated via global MVC action filter `AuditTrailActionFilter` and static helper hooks in `AuditTrailHelper.cs`.
  - Enforces department-level data isolation: EHC managers only see EHC data; Real Estate managers only see Real Estate data; Super Admins have a system-wide view.
  - Avoids transaction recursion by using `SaveChangesWithoutAudit()` when persisting log entries.
- **POST / Export Behaviour:**
  - Each report supports CSV data export via dedicated action endpoints (`ExportActiveUsers`, `ExportActivityLog`, etc.).
  - Clicking the export button downloads the file dynamically and triggers an audit activity log entry indicating who exported what report.

---

### PLM-001 · Renewal Document Types & Checklists
**Script file:** `Scripts/add_renewal_document_types.sql`  
**Date added:** 2026-05-17  
**Why:** The original PLM application upload flow (`DocumentCaptureApplication`) and the renewal upload flow (`DocumentCaptureTenantLease`) shared the same six `DocumentType`/`DocumentCheckList` rows (`dt_identity_document`, `dt_bank_statement`, etc.). This caused original application documents to appear on the tenant renewal upload page and be deletable by tenants during renewal.  
**Fix:** Six new rows created with `_renewal_` keys. `DocumentCaptureTenantLease` now points exclusively to these renewal rows so original and renewal documents are stored in completely separate buckets.  
**Code changed:** `Keys/DocumentTypeKeys.cs` (6 new constants), `Helpers/MatchingHelper.cs` (`DocumentCaptureTenantLease` method).

```sql
-- PLM-001: ADD RENEWAL-SPECIFIC DOCUMENT TYPES & CHECKLISTS
-- Change 'PropertyLeaseManagement' to the correct production DB name.
USE [PropertyLeaseManagement]
GO
SET NOCOUNT ON
GO

DECLARE @ApplicationId INT, @ReferenceTypeId INT
SELECT @ApplicationId   = Id FROM Applications  WHERE [Key] = 'a_rates_clearance_system'
SELECT @ReferenceTypeId = Id FROM ReferenceTypes WHERE [Key] = 'rt_upload_rcs'
IF @ApplicationId IS NULL OR @ReferenceTypeId IS NULL BEGIN RAISERROR('ERROR: missing Application or ReferenceType',16,1) RETURN END

-- 1. ID Document (Renewal)
DECLARE @IdDocRenewalId INT
SELECT @IdDocRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_identity_document'
IF @IdDocRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_identity_document','ID Document (Renewal)','Copy of ID - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @IdDocRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@IdDocRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@IdDocRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

-- 2. Proof of Income (Renewal)
DECLARE @IncomeRenewalId INT
SELECT @IncomeRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_income'
IF @IncomeRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_income','Proof of Income (Renewal)','Payslip/Pension/Grant - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @IncomeRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@IncomeRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@IncomeRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

-- 3. Bank Statement (Renewal)
DECLARE @BankRenewalId INT
SELECT @BankRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_bank_statement'
IF @BankRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_bank_statement','Bank Statement (Renewal)','Bank statement - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @BankRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@BankRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@BankRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

-- 4. Proof of Employment (Renewal)
DECLARE @EmploymentRenewalId INT
SELECT @EmploymentRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_employment'
IF @EmploymentRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_employment','Proof of Employment (Renewal)','Proof of employment - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @EmploymentRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@EmploymentRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@EmploymentRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

-- 5. Affidavit (Renewal)
DECLARE @AffidavitRenewalId INT
SELECT @AffidavitRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_affidavit'
IF @AffidavitRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_affidavit','Affidavit (Renewal)','Affidavit - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @AffidavitRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@AffidavitRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@AffidavitRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

-- 6. Proof of Address (Renewal)
DECLARE @AddressRenewalId INT
SELECT @AddressRenewalId = Id FROM DocumentTypes WHERE [Key] = 'dt_renewal_proof_of_address'
IF @AddressRenewalId IS NULL BEGIN
    INSERT INTO DocumentTypes ([Key],[Name],[Description],IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime,CreatedBySystemUserId)
    VALUES ('dt_renewal_proof_of_address','Proof of Address (Renewal)','Proof of address - lease renewal',1,0,GETDATE(),GETDATE(),1)
    SET @AddressRenewalId = SCOPE_IDENTITY() END
IF NOT EXISTS (SELECT 1 FROM DocumentCheckLists WHERE DocumentTypeId=@AddressRenewalId AND ReferenceTypeId=@ReferenceTypeId AND ApplicationId=@ApplicationId)
    INSERT INTO DocumentCheckLists (DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime)
    VALUES (@AddressRenewalId,@ApplicationId,@ReferenceTypeId,1,0,0,1,GETDATE(),1,GETDATE())

PRINT 'PLM-001 complete.'
SET NOCOUNT OFF
GO
```

---

## 🏛️ REAL ESTATE DEVELOPMENT (RED / DPRE) UNIFIED WORKFLOWS & SPECIFICATIONS

### 📌 Architectural & Domain Isolation Standard
1. **Database Schema**: All entities operate exclusively on `RE_*` tables (`RE_Applications`, `RE_Facilities`, `RE_FacilityCategories`, `RE_FacilityUnits`).
2. **Controllers & Views**: Customer interactions are handled in `RealEstateController.cs` (`Views/RealEstate/`), and back-office / admin operations in `RealEstateAdminController.cs` (`Views/RealEstateAdmin/`).
3. **Visual Design**: Uses local Onyx Obsidian gold theme CSS tokens (`var(--re-gold)`, `#c59b27`, `#d2930b`) without altering shared global layout files.

---

### RE_UC001 — RE_UC005: Property Onboarding & Dynamic Lease Application Capture
- **Actors:** Applicant, Real Estate Officer
- **Controllers:** `RealEstateController.cs`
- **Views:** `Capture.cshtml`, `Inbox.cshtml`, `MyApplications.cshtml`
- **Key Features:**
  - Cascading dropdowns: Customer Care Centre (CCC Area) $\rightarrow$ Facility Site $\rightarrow$ Unit Let-Space.
  - Multi-unit selection grid (`#tblSelectedUnits`) allowing applicants to stack multiple unit types per application.
  - Dynamic estimated rental total calculation: $\text{Monthly Rental} = \text{Size (m}^2\text{)} \times \text{Tariff (R/m}^2\text{)} \times \text{Quantity}$.
  - Pre-qualification document inline uploads (11 mandatory + optional docs).

---

### RE_UC021 — RE_UC025: Lease Administration & User Agreement Execution
- **Actors:** Property Officer, HOD (Development Planning & Real Estate), Tenant
- **Controllers:** `RealEstateAdminController.cs`, `RealEstateController.cs`
- **Views:** `PtoApprovals.cshtml`, `PtoSignatureQueue.cshtml`, `ActivePto.cshtml`, `GenerateLeaseAgreement.cshtml`, `SignLeaseAgreement.cshtml`, `LeaseAgreementApprovals.cshtml`, `Allocations.cshtml`, `ActiveOccupancy.cshtml`
- **Workflow Steps:**
  - **RE_UC021**: Draft Permission to Occupy (PTO) Certificate & HOD Signature Canvas.
  - **RE_UC022**: Revoke / Terminate Active PTO with reason & evidence upload.
  - **RE_UC023**: Generate Standard Lease, Live Parameter Review (Rental, Deposit, Duration, Escalation, Special Clauses), `#paperDraft` preview sheet, Tenant PDF Upload, & HOD Sign-off with confirmation modals (`#modalConfirmSubmit`, `#modalTenantConfirm`, `#modalHodConfirm`).
  - **RE_UC024**: Unit Let-Space Allocation.
  - **RE_UC025**: Capture Detailed Lease Classification & Unique Tenancy Reference (`LSE-*`).

---

### RE_UC026 (NEW): Evaluation Criteria Navigation & Working Committee Screening Tool
- **Actors:** Client Services Officer, Working Committee, Evaluation Committee
- **Controllers:** `RealEstateAdminController.cs` (`EvaluationCriteria` action)
- **Views:** `Views/RealEstateAdmin/EvaluationCriteria.cshtml`, `Views/Shared/RCS_Layout.cshtml`
- **Navigation Location:** New top-level navigation panel menu item: **"Evaluation Criteria"** containing 3 sub-tabs:
  1. **Tab 1 — Pre-Qualification Documents**: Guidelines for 12 mandatory applicant uploads (Business Plan, Company Profile, Tax Clearance/BEE, MBD 4 Declaration, CSD Registration, 3 Years Audited Financials, Proof of Location, Certified ID, Facilities Management Experience, Funding Letter, Strategic Partnerships, Ownership/Job Creation structure).
  2. **Tab 2 — Pre-Qualification Evaluation (Scoring Matrix)**:
     - **Track Record / Experience** (15 pts max): 11+ yrs = 15 pts, 6-10 yrs = 13 pts, 3-5 yrs = 5 pts, No submission = 0 pts. Requires 3 yrs financials & 3 testimonial letters.
     - **Financial Stability** (20 pts max): R1.5M+ = 20 pts, R1M+ = 15 pts, R750k+ = 12 pts, R500k+ = 10 pts, No funding = 0 pts. Requires bank statement/intent letter/guarantee.
     - **Business Case** (25 pts max): Strategic Plan = 10 pts, Sector Analysis = 5 pts, Financial Model = 10 pts (Funding Mix 5 pts, Projected Revenue 5 pts).
     - **Marketing Plan** (20 pts max): Marketing Methods = 10 pts, Strategic Partnerships = 10 pts.
     - **Operations Plan** (10 pts max): Enterprise Development = 5 pts, Facility Management = 5 pts.
  3. **Tab 3 — Checklist for Committee Evaluations (Screening & Adjudication)**:
     - **Screening Committee**: Interactive compliance checklist (Attached / Not attached) for 11 compliance documents, screening officer name, date, signature, and Declined (Yes/No) toggle.
     - **Evaluation Committee**: Job Creation scoring scale (15+ jobs = 50 pts, 10-14 jobs = 40 pts, 5-13 jobs = 30 pts, 2-12 jobs = 20 pts).

---

### RE_UC027 (NEW): PDF User Agreement Generation for Municipal Property Leases
- **Actors:** Property Officer, System Automated PDF Engine
- **Controllers:** `RealEstateAdminController.cs` (`DownloadUserAgreement` action), `Helpers/RealEstateUserAgreementHelper.cs`
- **Templates Touched:**
  - `PDFTemplates/RealEstate/URC User Agreement_Latest.doc` (Economic Development / Standard Municipal Template)
  - `PDFTemplates/RealEstate/Updated User Agreement.doc` (Development Planning & Real Estate / CoE Updated Template)
- **PDF Data Population Mapping:**
  - **Lessor / Municipality Representation**: Head of Department: Development Planning & Real Estate.
  * **Tenant / User Data**: Full Name, ID Number, Residential Address, Entity Name, Registration Number, VAT Number, Tax Number, Representative Name & Capacity, Resolution Date.
  * **Premises & Lease Specifications**: Property Name, CCC Area, Address, Erf/Farm Number, Unit Type (Kiosk, Stall, Workshop, Office), Unit Size ($m^2$), Gazetted Tariff Rate ($R/m^2$), Calculated Monthly Rental, Deposit Amount, Lease Duration (months), Commencement Date, Escalation Rate, and Special Clauses.

