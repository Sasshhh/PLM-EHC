# Database Schema Reference — Real Estate Branch

This document defines the complete database schema for the Real Estate branch, detailing core inventory tables, the main application table, departmental comment logs, evaluation tables, and audit trail tables.

---

## 1. Inventory & Pricing Tables

### `RE_FacilityCategories`
Stores the high-level lease categories and their gazetted tariff rates per square meter.
* **Id** (`INT`, Primary Key, Identity)
* **Key** (`NVARCHAR(100)`, Unique, Not Null) — System key (e.g., `township_industrial_parks`)
* **Name** (`NVARCHAR(250)`, Not Null) — Display name (e.g., `Township Industrial Parks`)
* **TariffPerSqm** (`DECIMAL(18,2)`, Not Null) — Gazetted rate per m²
* **DepartmentId** (`INT`, Null) — Maps to CoE department
* **IsActive** (`BIT`, Default 1, Not Null)
* **IsDeleted** (`BIT`, Default 0, Not Null)
* **IsLocked** (`BIT`, Default 0, Null)
* **CreatedBySystemUserId** (`INT`, Null)
* **CreatedDateTime** (`DATETIME`, Null)
* **ModifiedBySystemUserId** (`INT`, Null)
* **ModifiedDateTime** (`DATETIME`, Null)

### `RE_Facilities`
Houses the physical facility site locations and regions.
* **Id** (`INT`, Primary Key, Identity)
* **Name** (`NVARCHAR(250)`, Not Null) — e.g., `Tsakane Business Park`
* **CCCId** (`INT`, Not Null) — Foreign Key to `dbo.CCCs(Id)` (Customer Care Centre)
* **Address** (`NVARCHAR(500)`, Null)
* **DepartmentId** (`INT`, Null) — Maps to CoE department
* **IsActive** (`BIT`, Default 1, Not Null)
* **IsDeleted** (`BIT`, Default 0, Not Null)
* **IsLocked** (`BIT`, Default 0, Null)
* **CreatedBySystemUserId** (`INT`, Null)
* **CreatedDateTime** (`DATETIME`, Null)
* **ModifiedBySystemUserId** (`INT`, Null)
* **ModifiedDateTime** (`DATETIME`, Null)

### `RE_FacilityUnits`
Represents the individual letting units/spaces available inside a facility.
* **Id** (`INT`, Primary Key, Identity)
* **FacilityId** (`INT`, Not Null) — FK to `RE_Facilities(Id)`
* **FacilityCategoryId** (`INT`, Not Null) — FK to `RE_FacilityCategories(Id)`
* **UnitType** (`NVARCHAR(150)`, Not Null) — e.g., `Office`, `Workshop`, `Kiosk`
* **UnitSize** (`DECIMAL(18,2)`, Not Null) — Dimensions in m²
* **MaxUnits** (`INT`, Default 1, Not Null) — Total available quantity of this unit type
* **DepartmentId** (`INT`, Null) — Maps to CoE department
* **IsActive** (`BIT`, Default 1, Not Null)
* **IsDeleted** (`BIT`, Default 0, Not Null)
* **IsLocked** (`BIT`, Default 0, Null)
* **CreatedBySystemUserId** (`INT`, Null)
* **CreatedDateTime** (`DATETIME`, Null)
* **ModifiedBySystemUserId** (`INT`, Null)
* **ModifiedDateTime** (`DATETIME`, Null)

---

## 2. Main Lease Applications Table

### `RE_Applications`
Main transactional table housing the entire workflow state, captures details, risk reviews, committee resolutions, inspections, works orders, and early occupation approvals.
* **Id** (`INT`, Primary Key, Identity)
* **ApplicationReferenceNumber** (`NVARCHAR(100)`, Not Null) — Format: `DPRE-YYYYMMDD-XXXX`
* **SystemUserId** (`INT`, Not Null) — FK to `SystemUsers(Id)` (Applicant user profile)
* **CustomerId** (`INT`, Not Null) — FK to `Customers(Id)` (Applicant billing customer)
* **ApplicantType** (`NVARCHAR(100)`, Not Null) — e.g., `Individual`, `Company`, `NPO`
* **StatusId** (`INT`, Not Null) — FK to `Status(Id)` (Workflow state machine)
* **DepartmentId** (`INT`, Null) — Always `3` (Real Estate Development)

#### Premises Selection & Pricing
* **SelectedFacilityId** (`INT`, Null) — FK to `RE_Facilities(Id)`
* **SelectedFacilityUnitId** (`INT`, Null) — FK to `RE_FacilityUnits(Id)`
* **SelectedUnitCount** (`INT`, Null) — Number of units requested
* **CalculatedMonthlyRental** (`DECIMAL(18,2)`, Null) — Dynamically aggregated rental

#### Entity Details
* **EntityName** (`NVARCHAR(250)`, Null)
* **CompanyRegistrationNumber** (`NVARCHAR(100)`, Null) — Format: `YYYY/NNNNNN/NN`
* **VatRegistrationNumber** (`NVARCHAR(100)`, Null) — 10 digits starting with `4`
* **TaxReferenceNumber** (`NVARCHAR(100)`, Null) — 10 digits
* **EntityRegisteredAddress** (`NVARCHAR(500)`, Null)
* **EntityRegisteredPostalCode** (`NVARCHAR(20)`, Null) — 4 digits
* **AuthorizedRepresentativeName** (`NVARCHAR(250)`, Null)
* **AuthorizedRepresentativeCapacity** (`NVARCHAR(100)`, Null)
* **EntityTelephone** / **EntityMobile** / **EntityFax** (`NVARCHAR(50)`, Null) — 10 digits starting with `0`
* **EntityEmail** (`NVARCHAR(150)`, Null)

#### Banking Details (Retained as Nullable, Not captured in front-end)
* **BankName** (`NVARCHAR(100)`, Null)
* **BankAccountType** (`NVARCHAR(50)`, Null)
* **BankAccountName** (`NVARCHAR(150)`, Null)
* **BankAccountNumber** (`NVARCHAR(100)`, Null)
* **BankBranchCode** (`NVARCHAR(50)`, Null)

#### Premises Specification
* **PurposeOfLease** (`NVARCHAR(100)`, Not Null)
* **CCCId** (`INT`, Not Null) — FK to `CCCs(Id)`
* **ErfFarmNumber** (`NVARCHAR(100)`, Not Null)
* **PropertyAddress** (`NVARCHAR(500)`, Not Null)
* **TownshipSuburbFarmName** (`NVARCHAR(200)`, Not Null)
* **PropertyPostalCode** (`NVARCHAR(20)`, Not Null)

#### Checked Facility Sub-types (Historical checkboxes)
* **FacilityOutdoorAdvertising** / **FacilityTelecommunications** / **FacilityInformalTrading** / **FacilityTaxiRankTrading** / **FacilityVocationalSkills** / **FacilityComputerTraining** / **FacilityIndustrialPark** / **FacilityBusinessHub** / **FacilityAutomotiveHub** / **FacilityAgriPark** / **FacilityIncubationFarm** (`BIT`, Default 0, Not Null)

#### Vetting & Risk Assessment (UC07)
* **CreditBureauResult** (`NVARCHAR(MAX)`, Null)
* **HomeAffairsResult** (`NVARCHAR(MAX)`, Null)
* **DeedsResult** (`NVARCHAR(MAX)`, Null)
* **SassaResult** (`NVARCHAR(MAX)`, Null)
* **CipcResult** (`NVARCHAR(MAX)`, Null)
* **RiskAssessmentRecommendation** (`NVARCHAR(MAX)`, Null)
* **RiskAssessmentReason** (`NVARCHAR(MAX)`, Null)
* **RiskAssessmentEvidenceFileId** (`INT`, Null) — FK to `Files(Id)`
* **PaymentValidationComment** (`NVARCHAR(MAX)`, Null)

#### Committee Decision (UC11)
* **CommitteeDecision** (`NVARCHAR(MAX)`, Null) — e.g., `Recommended`
* **CommitteeComments** (`NVARCHAR(MAX)`, Null)
* **CommitteeResolutionFileId** (`INT`, Null) — FK to `Files(Id)`

#### HOD Lease Final Award (UC12)
* **FinalOutcome** (`NVARCHAR(MAX)`, Null) — `Approved` / `Rejected`
* **FinalComments** (`NVARCHAR(MAX)`, Null)
* **FinalSignature** (`NVARCHAR(MAX)`, Null) — Base64 HOD signature canvas blob

#### Pre-Occupation Inspection (UC14)
* **InspectionType** (`NVARCHAR(MAX)`, Null) — e.g., `Pre-Occupation`
* **InspectionDate** (`DATETIME`, Null)
* **InspectionTime** (`NVARCHAR(MAX)`, Null)
* **InspectionStatus** (`NVARCHAR(MAX)`, Null)
* **InspectionComments** (`NVARCHAR(MAX)`, Null)
* **InspectionPlumbing** / **InspectionElectrical** / **InspectionFixtures** / **InspectionSanitation** / **InspectionHazards** / **InspectionWearTear** (`NVARCHAR(MAX)`, Null)
* **InspectionFormFileId** (`INT`, Null) — FK to `Files(Id)` (Signed checklist PDF)

#### Works Order Maintenance (UC15/UC17)
* **WorkOrderNumber** (`NVARCHAR(MAX)`, Null)
* **WorkOrderStatus** (`NVARCHAR(MAX)`, Null) — e.g., `Logged`, `Assigned`, `Closed`
* **WorkOrderTasks** (`NVARCHAR(MAX)`, Null)
* **WorkOrderIssueDescription** (`NVARCHAR(MAX)`, Null)
* **WorkOrderPriority** (`NVARCHAR(MAX)`, Null) — `Low`, `Medium`, `High`
* **WorkOrderDueDate** (`DATETIME`, Null)
* **WorkOrderMaterials** (`NVARCHAR(MAX)`, Null)
* **WorkOrderSafetyInstructions** (`NVARCHAR(MAX)`, Null)
* **WorkOrderAssignmentType** (`NVARCHAR(MAX)`, Null) — `Internal`, `External`
* **WorkOrderTechnicianName** (`NVARCHAR(MAX)`, Null)
* **WorkOrderRejectionReason** (`NVARCHAR(MAX)`, Null)
* **WorkOrderJobSheetFileId** (`INT`, Null) — FK to `Files(Id)` (Technician job sheet)
* **WorkOrderManagerComments** (`NVARCHAR(MAX)`, Null)
* **WorkOrderManagerSignature** (`NVARCHAR(MAX)`, Null) — Base64 Facilities Manager signature

#### Permission to Occupy (PTO) Early Access (UC18-21)
* **PtoReferenceNumber** (`NVARCHAR(MAX)`, Null)
* **PtoPurposeOfOccupation** (`NVARCHAR(MAX)`, Null)
* **PtoStartDate** (`DATETIME`, Null)
* **PtoEndDate** (`DATETIME`, Null)
* **PtoAcceptedIndemnity** (`BIT`, Null)
* **PtoStatus** (`NVARCHAR(MAX)`, Null)
* **PtoReviewRecommendation** (`NVARCHAR(MAX)`, Null)
* **PtoReviewReason** (`NVARCHAR(MAX)`, Null)
* **PtoDecision** (`NVARCHAR(MAX)`, Null)
* **PtoDecisionReason** (`NVARCHAR(MAX)`, Null)
* **PtoSignature** (`NVARCHAR(MAX)`, Null) — Base64 HOD signature canvas blob
* **PtoRevocationReason** (`NVARCHAR(MAX)`, Null)
* **PtoRevocationDate** (`DATETIME`, Null)

#### Lease Agreement Signature (UC23)
* **LeaseAgreementFileId** (`INT`, Null) — FK to `Files(Id)` (Draft Lease PDF)
* **LeaseAgreementSignedFileId** (`INT`, Null) — FK to `Files(Id)` (Uploaded Signed Lease PDF)
* **LeaseAgreementTenantSignatureDate** (`DATETIME`, Null)
* **LeaseAgreementHodSignature** (`NVARCHAR(MAX)`, Null) — Base64 HOD signature canvas blob
* **LeaseAgreementHodSignatureDate** (`DATETIME`, Null)

#### Lease Activation Details (UC25)
* **LeaseCategory** (`NVARCHAR(100)`, Null) — `Temporary Occupation`, `Lease/User Agreement Term`, `Month-To-Month`, `Long-Term`
* **UniqueTenancyLeaseNumber** (`NVARCHAR(100)`, Null) — Format: `UTLN-YYYY-XXXXX`
* **LeaseStartDate** (`DATETIME`, Null)
* **LeaseEndDate** (`DATETIME`, Null)
* **LeaseDateOfOccupation** (`DATETIME`, Null)
* **LeaseEscalationTerms** (`NVARCHAR(MAX)`, Null)
* **LeasePaymentFrequency** (`NVARCHAR(100)`, Null)
* **LeaseDepositAmount** (`DECIMAL(18,2)`, Null)
* **LeaseStatus** (`NVARCHAR(100)`, Null) — e.g., `Active`

#### BaseModel Fields
* **IsActive** (`BIT`, Default 1, Not Null)
* **IsDeleted** (`BIT`, Default 0, Not Null)
* **IsLocked** (`BIT`, Default 0, Null)
* **CreatedBySystemUserId** (`INT`, Null)
* **CreatedDateTime** (`DATETIME`, Null)
* **ModifiedBySystemUserId** (`INT`, Null)
* **ModifiedDateTime** (`DATETIME`, Null)

---

## 3. Department Review Comments

### `RE_DepartmentalComments`
Captures reviews, recommendations, and uploaded supporting files from inter-departmental reviewers.
* **Id** (`INT`, Primary Key, Identity)
* **RE_ApplicationId** (`INT`, Not Null) — FK to `RE_Applications(Id)`
* **DepartmentName** (`NVARCHAR(250)`, Not Null) — Name of reviewing department
* **DepartmentId** (`INT`, Null) — Maps to CoE department lookup table
* **RepresentativeName** (`NVARCHAR(100)`, Null) — Representative reviewer's name
* **Outcome** (`NVARCHAR(100)`, Null) — `Supported`, `Supported with Conditions`, `Not Supported`, `Info Request`
* **Comments** (`NVARCHAR(MAX)`, Null)
* **SupportingDocumentFileId** (`INT`, Null) — FK to `Files(Id)`
* **DateStamp** (`DATETIME`, Null)
* **IsActive** / **IsDeleted** / **IsLocked** (`BIT`, Default values)
* **CreatedBySystemUserId** / **CreatedDateTime** / **ModifiedBySystemUserId** / **ModifiedDateTime** (`BaseModel fields`)

---

## 4. Evaluation Criteria Reference Tables

### `RE_EvaluationCriteria_PreQualDoc`
Master checklist for the 12 mandatory document uploads.
* **Id** (`INT`, Primary Key, Identity)
* **DocumentName** (`NVARCHAR(250)`, Not Null) — e.g., `SARS Tax Clearance Pin`
* **IsMandatory** (`BIT`, Default 1, Not Null)
* **Description** (`NVARCHAR(500)`, Null)
* **IsActive** (`BIT`, Default 1, Not Null)
* **CreatedDateTime** (`DATETIME`, Default GETDATE(), Not Null)

### `RE_EvaluationCriteria_PreQualEval`
Interactive scorecard categories, weighting weights, and threshold limits.
* **Id** (`INT`, Primary Key, Identity)
* **CriteriaCategory** (`NVARCHAR(200)`, Not Null) — e.g., `Financial Capability`
* **CriteriaDescription** (`NVARCHAR(500)`, Not Null)
* **WeightScore** (`DECIMAL(5,2)`, Default 0.00, Not Null) — Max points allocated
* **PassingThreshold** (`DECIMAL(5,2)`, Default 0.00, Not Null) — Required points to pass
* **IsActive** (`BIT`, Default 1, Not Null)
* **CreatedDateTime** (`DATETIME`, Default GETDATE(), Not Null)

### `RE_EvaluationCriteria_CommitteeChecklist`
Compliance checklists for the Working & Evaluation Committees.
* **Id** (`INT`, Primary Key, Identity)
* **ChecklistItem** (`NVARCHAR(250)`, Not Null)
* **ChecklistCategory** (`NVARCHAR(100)`, Not Null) — e.g., `Land Use`, `Public Safety`
* **RequiredStatus** (`NVARCHAR(50)`, Default 'Compliant', Not Null)
* **IsMandatory** (`BIT`, Default 1, Not Null)
* **IsActive** (`BIT`, Default 1, Not Null)
* **CreatedDateTime** (`DATETIME`, Default GETDATE(), Not Null)

---

## 5. Audit Tables

Each major inventory and transactional table is accompanied by a corresponding audit table. Triggers or service managers save matching history rows on changes.

* **`RE_ApplicationsAudit`** — Audit trail for applications
* **`RE_FacilityAudits`** — Audit trail for facility sites
* **`RE_FacilityCategoryAudits`** — Audit trail for facility category tariffs
* **`RE_FacilityUnitAudits`** — Audit trail for let-spaces
* **`RE_DepartmentalCommentAudits`** — Audit trail for departmental reviews

---

## 6. Shared Schema Extensions (In Shared Database)

To link Real Estate applications into shared logging and file storage structures, the following columns were added to shared tables:

* **`dbo.Documents.RealEstateApplicationId`** (`INT`, Null, FK to `RE_Applications(Id)`)
* **`dbo.PLMApplicationHistortyLogs.RealEstateApplicationId`** (`INT`, Null, FK to `RE_Applications(Id)`)
* **`dbo.PLMApplicationHistortyLogAudits.RealEstateApplicationId`** (`INT`, Null)
