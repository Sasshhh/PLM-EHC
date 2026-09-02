# Complaints/Transgressions Module (UC17B) - Implementation Guide

## 📋 Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Features Implemented](#features-implemented)
4. [Business Rules](#business-rules)
5. [Database Setup](#database-setup)
6. [Role Configuration](#role-configuration)
7. [Testing Guide](#testing-guide)
8. [API Reference](#api-reference)
9. [File Upload Configuration](#file-upload-configuration)
10. [Email/SMS Configuration](#emailsms-configuration)
11. [Known Limitations](#known-limitations)
12. [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

The Complaints/Transgressions Module enables tenants to lodge formal complaints against other tenants, with a complete workflow from submission through investigation to resolution. The system enforces a **7-day SLA** for complaint resolution and automates warning letter generation and lease termination processes.

### Key Capabilities
- ✅ Multi-channel complaint submission (online form)
- ✅ Automated assignment to Client Services Officers (round-robin)
- ✅ Investigation appointment scheduling with alternative date proposals
- ✅ Warning letter tracking (3 warnings trigger escalation)
- ✅ Automatic lease termination for sub-letting violations
- ✅ External referral tracking to agencies (RSD, SAPS, etc.)
- ✅ Comprehensive audit trail (BR27 compliance)
- ✅ SLA monitoring (7 working days - BR22, BR24)
- ✅ Email and SMS notifications at all stages

---

## 🏗️ Architecture

### Design Pattern: Engine-Based Architecture

The module follows a **separation of concerns** approach with reusable generic engines:

```
┌─────────────────────────────────────────────────────────────┐
│                    ComplaintsController                      │
│  (Handles HTTP requests, authorization, view rendering)      │
└─────────────────────────┬───────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Notification │  │   Workflow   │  │   SLA        │
│   Engine     │  │   Engine     │  │   Engine     │
└──────────────┘  └──────────────┘  └──────────────┘
│ Email/SMS    │  │ Assignment   │  │ 7-day track  │
│ All stages   │  │ Status Mgmt  │  │ Overdue      │
│              │  │ Warning Ltrs │  │ Statistics   │
│              │  │ Lease Term.  │  │              │
│              │  │ Audit Log    │  │              │
└──────────────┘  └──────────────┘  └──────────────┘
```

### Components

#### 1. **ComplaintsController.cs**
Main controller handling all complaint operations:
- `Index()` - List complaints with SLA dashboard
- `Create()` - Submit new complaint
- `Details()` - View complaint details
- `ScheduleAppointment()` - CSO schedules investigation
- `ConfirmAppointment()` - Respondent confirms/proposes alternative
- `ApproveAlternativeDate()` - CSO approves alternative appointment
- `CaptureOutcome()` - CSO records investigation result
- `GetComplaintTypes()` - AJAX endpoint for cascading dropdowns

#### 2. **NotificationEngine.cs** (Reusable)
Handles all email and SMS notifications:
- Complaint acknowledgement to complainant
- Appointment notifications to respondent
- Confirmation notifications to CSO
- Assignment notifications to CSO
- Outcome notifications to complainant
- Warning letters to respondent (1st, 2nd, FINAL)

#### 3. **ComplaintWorkflowEngine.cs** (Reusable)
Manages business logic and workflow:
- **Round-robin CSO assignment** (balances workload)
- Status transitions
- Warning letter counting per unit
- Sub-letting detection (BR26)
- Lease termination triggering
- Audit trail logging (BR27)

#### 4. **ComplaintSLAEngine.cs** (Reusable)
Monitors service level agreements:
- 7-day SLA tracking
- Overdue complaint detection
- Days remaining calculation
- SLA status ("On Track", "At Risk", "Overdue")
- Dashboard statistics

---

## ✨ Features Implemented

### 1. Complaint Submission (UC17B Steps 1-21)
- **Categories**: Administration, Nuisance & Behavioural, Parking & Vehicle, Pets & Animals, Property Usage, Safety & Security, Other
- **21 Complaint Types** grouped by category
- Complainant auto-populated from logged-in tenant
- Respondent selected by Complex → Block → Unit
- Evidence file uploads (photos, videos, documents)
- Unique case reference number generation (COMP-YYYY-MM-XXXXXX)
- Instant email/SMS acknowledgement
- Automatic CSO assignment (round-robin based on current workload)

### 2. Investigation Appointment Scheduling (Steps 22-34)
- CSO schedules appointment with date, time, location
- System validates future dates only
- Email/SMS notification sent to respondent
- Respondent can confirm or propose alternative

### 3. Alternative Appointment Proposal (Steps 35-38 Alternate Flow)
- Respondent proposes alternative date/time with reason
- CSO receives notification
- CSO approves or rejects alternative
- System updates appointment if approved
- Re-notification sent to respondent

### 4. Outcome Recording (Steps 39-47)
Three possible outcomes:
- **Resolved**: Complaint resolved, may trigger warning letter
- **Referred**: Escalated to external agency (RSD, SAPS, Municipal Health, etc.)
- **Unresolved**: Could not be resolved, triggers lease termination process

Supporting document uploads for outcome evidence

### 5. Warning Letter System (BR25)
- Automatic counting of warnings per respondent unit
- **3-warning threshold**: After 3rd warning, escalation occurs
- Warning letter includes:
  - Warning number (1st, 2nd, FINAL WARNING)
  - Complaint details
  - Date and evidence
  - Consequences of further violations
- Email and SMS delivery

### 6. Sub-letting Policy Enforcement (BR26)
- **Automatic detection** of sub-letting complaints
- When sub-letting complaint is **Resolved**:
  - Lease termination is **automatically triggered**
  - Lease status changed to "Lease Termination Pending"
  - No warning letters issued
  - Direct eviction process initiated

### 7. Unresolved Complaint Handling
- When complaint marked as **Unresolved**:
  - Lease termination process triggered
  - Lease status changed to "Lease Termination Pending"
  - Email/SMS notification sent to complainant and respondent

### 8. Audit Trail (BR27 Compliance)
Every action is logged with:
- Action type (e.g., "Complaint Submitted", "Outcome Captured")
- Details of action
- User who performed action
- Timestamp
- Visible in complaint details view

### 9. Access Control & Privacy
- **Client Services Officers**: See all complaints
- **Tenants**: See only:
  - Complaints they submitted (as complainant)
  - Complaints where they are the respondent
- Unauthorized access returns 403 Forbidden
- Case reference numbers visible to authorized users only

### 10. SLA Monitoring (BR22, BR24)
- **7 working days** to resolve complaints
- Dashboard shows:
  - Total complaints
  - On Track (< 5 days)
  - At Risk (5-7 days)
  - Overdue (> 7 days)
- Color-coded status badges
- Days remaining/overdue calculation

---

## 📜 Business Rules

### BR22: Service Level Agreement
- All complaints must be resolved within **7 working days**
- SLA tracking starts from `DateSubmitted`
- Weekend days are excluded from working day calculation

### BR24: Compliance Consequences
- Exceeding 7-day SLA requires escalation to senior management
- System flags overdue complaints for visibility

### BR25: Warning Letter Management
- **3 warnings** before lease termination consideration
- Warning letters must document:
  - Specific transgression
  - Date and evidence
  - Consequences of repeat violations
- Each unit tracks its own warning count

### BR26: Sub-letting Policy
- Sub-letting is a **zero-tolerance** violation
- **Immediate lease termination** upon confirmation
- No warning letters issued for sub-letting
- Automatic eviction process triggered

### BR27: Audit Trail Requirements
- **All actions** must be logged
- Logs include: Who, What, When, Details
- Audit trail is **immutable** (no deletions)
- Visible to authorized users

---

## 🗄️ Database Setup

### Step 1: Run Migration Script

**File**: `C8.eServices.Mvc\MasterData\ComplaintsDatabaseMigration.sql`

This script adds new columns and tables:

```sql
-- Run this FIRST
USE [eServicesDb]
GO

-- Execute the migration script
-- This will:
-- 1. Add 4 columns to TenantComplaints table
-- 2. Add 4 columns to ComplaintInvestigations table
-- 3. Create ComplaintAuditLogs table
-- 4. Create performance indexes
```

**What it does**:
- **TenantComplaints**: Adds `WarningLetterCount`, `LastWarningDate`, `LeaseTerminationTriggered`, `LeaseTerminationDate`
- **ComplaintInvestigations**: Adds `ProposedAlternativeDate`, `ProposedAlternativeTime`, `AlternativeDateReason`, `AlternativeApproved`
- **ComplaintAuditLogs**: Creates audit trail table with foreign keys
- **Indexes**: Creates performance indexes for SLA queries and respondent lookups

**Verification**:
The script includes verification queries to confirm all changes were applied.

### Step 2: Run Master Data Script

**File**: `C8.eServices.Mvc\MasterData\ComplaintsMasterData.sql`

This script inserts all required master data:

```sql
-- Run this SECOND (after migration)
USE [eServicesDb]
GO

-- Execute the master data script
-- This will:
-- 1. Insert 7 complaint statuses
-- 2. Insert 7 complaint categories
-- 3. Insert 21 complaint types
```

**What it inserts**:

#### Statuses (7)
1. `complaint_status_submitted` - Initial state
2. `complaint_status_awaiting_appointment` - CSO assigned, needs appointment
3. `complaint_status_awaiting_investigation` - Appointment scheduled
4. `complaint_status_resolved` - Complaint resolved
5. `complaint_status_referred` - Referred to external agency
6. `complaint_status_unresolved` - Could not be resolved
7. `lease_termination_pending` - Lease termination triggered

#### Categories (7)
1. Administration (sub-letting, rule violations)
2. Nuisance and Behavioural (noise, odors, aggression, children)
3. Parking and Vehicle (obstructive, unauthorized, car wash)
4. Pet and Animals (nuisance pets, unapproved pets)
5. Property Usage (untidy, neglected, alterations, misuse)
6. Safety & Security (security neglect, other safety issues)
7. Other (catch-all category)

#### Types (21)
Each type is linked to its parent category with display order.

**Important**: The script is **idempotent** - safe to run multiple times. It uses `IF NOT EXISTS` checks.

---

## 👥 Role Configuration

### Required Role

The module requires the **"Client Services Officer"** role to exist in the `AspNetRoles` table.

#### Check if role exists:
```sql
SELECT * FROM AspNetRoles WHERE Name = 'Client Services Officer'
```

#### Create role if missing:
```sql
INSERT INTO AspNetRoles (Id, Name)
VALUES (NEWID(), 'Client Services Officer')
```

### Assign Users to CSO Role

At least **one user** must be assigned to the CSO role for round-robin assignment to work.

#### Assign user to role:
```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (
    (SELECT Id FROM AspNetUsers WHERE UserName = 'cso.user@example.com'),
    (SELECT Id FROM AspNetRoles WHERE Name = 'Client Services Officer')
)
```

#### Verify CSO assignments:
```sql
SELECT u.UserName, u.Email, r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Client Services Officer'
```

---

## 🧪 Testing Guide

### Prerequisites
1. ✅ Database migration script executed
2. ✅ Master data script executed
3. ✅ At least one CSO user assigned
4. ✅ Email/SMS configuration completed
5. ✅ At least two tenant accounts for testing (complainant + respondent)

### Test Scenario 1: Basic Complaint Flow

**Actors**: Tenant A (complainant), Tenant B (respondent), CSO

1. **Submit Complaint** (Tenant A)
   - Navigate to `/Complaints/Create`
   - Select category: "Nuisance and Behavioural"
   - Select type: "Noise Disturbance"
   - Select respondent: Tenant B's unit
   - Enter description
   - Upload evidence file (optional)
   - Click Submit
   - ✅ Verify case reference generated (COMP-YYYY-MM-XXXXXX)
   - ✅ Verify acknowledgement email/SMS sent
   - ✅ Verify CSO automatically assigned

2. **View Assignment** (CSO)
   - Navigate to `/Complaints`
   - ✅ Verify complaint appears in list
   - ✅ Verify SLA status shows "On Track"
   - Click on complaint to view details

3. **Schedule Appointment** (CSO)
   - In complaint details, click "Schedule Appointment"
   - Enter appointment date (future date)
   - Enter time (e.g., 14:00)
   - Enter location
   - Click Submit
   - ✅ Verify appointment notification sent to Tenant B
   - ✅ Verify status changed to "Awaiting Investigation"

4. **Confirm Appointment** (Tenant B as Respondent)
   - Login as Tenant B
   - Navigate to `/Complaints` (see respondent complaints)
   - Click on complaint
   - Click "Confirm Appointment"
   - Select "Yes, I can attend"
   - Click Submit
   - ✅ Verify confirmation sent to CSO

5. **Capture Outcome** (CSO)
   - Navigate to complaint details
   - Click "Capture Outcome"
   - Select outcome: "Resolved"
   - Select "Issue Warning Letter": Yes
   - Enter findings
   - Upload supporting document (optional)
   - Click Submit
   - ✅ Verify warning letter sent to Tenant B
   - ✅ Verify outcome notification sent to Tenant A
   - ✅ Verify status changed to "Resolved"
   - ✅ Verify warning count incremented

### Test Scenario 2: Alternative Appointment Date

1. **Schedule Appointment** (CSO) - same as above

2. **Propose Alternative** (Tenant B)
   - Login as Tenant B
   - View complaint details
   - Click "Confirm Appointment"
   - Select "No, I would like to propose an alternative date"
   - Enter alternative date/time
   - Enter reason (e.g., "I have a work commitment")
   - Click Submit
   - ✅ Verify notification sent to CSO

3. **Approve Alternative** (CSO)
   - View complaint details
   - See "Alternative Date Proposed" section
   - Click "Approve Alternative Date"
   - ✅ Verify appointment date updated
   - ✅ Verify notification sent to Tenant B
   - ✅ Continue with investigation

### Test Scenario 3: Sub-letting Auto-Eviction (BR26)

1. **Submit Sub-letting Complaint** (Tenant A)
   - Select category: "Administration"
   - Select type: "Sub-letting"
   - Provide evidence
   - Submit

2. **Investigate and Resolve** (CSO)
   - Schedule appointment
   - Respondent confirms
   - Capture outcome: "Resolved"
   - **Do NOT** check "Issue Warning Letter" (sub-letting is auto-eviction)
   - Enter findings confirming sub-letting occurred
   - Click Submit
   - ✅ Verify lease termination automatically triggered
   - ✅ Verify `LeaseTerminationTriggered = true`
   - ✅ Verify lease status changed to "Lease Termination Pending"
   - ✅ Verify NO warning letter sent (direct eviction)

### Test Scenario 4: Warning Letter Escalation (BR25)

1. **Submit 3 Separate Complaints** against same respondent unit
   - Complaint 1: Noise disturbance → Resolved → Warning #1
   - Complaint 2: Parking violation → Resolved → Warning #2
   - Complaint 3: Property neglect → Resolved → Warning #3 (FINAL WARNING)

2. **Verify Warning Count**
   ```sql
   SELECT WarningLetterCount, LastWarningDate 
   FROM TenantComplaints 
   WHERE RespondentComplexId = X 
     AND RespondentBlockNumber = Y 
     AND RespondentUnitNumber = Z
   ORDER BY DateSubmitted DESC
   ```
   - ✅ Verify count increments with each resolution
   - ✅ Verify 3rd warning letter contains "FINAL WARNING"
   - ✅ After 3 warnings, escalation procedures apply

### Test Scenario 5: Unresolved → Lease Termination

1. **Submit Complaint** (any type except sub-letting)

2. **Capture Outcome: Unresolved** (CSO)
   - Select outcome: "Unresolved"
   - Enter reason (e.g., "Respondent non-cooperative, issue persists")
   - Upload documentation
   - Click Submit
   - ✅ Verify lease termination triggered
   - ✅ Verify status changed to "Unresolved"
   - ✅ Verify notifications sent to both parties

### Test Scenario 6: External Referral

1. **Capture Outcome: Referred** (CSO)
   - Select outcome: "Referred"
   - Select agency (e.g., "RSD - Rental Support Desk")
   - Enter referral reason
   - Upload documents
   - Click Submit
   - ✅ Verify status changed to "Referred"
   - ✅ Verify external referral record created
   - ✅ Verify notification sent to complainant

### Test Scenario 7: SLA Monitoring

1. **Create Test Data** with backdated submissions:
   ```sql
   UPDATE TenantComplaints 
   SET DateSubmitted = DATEADD(DAY, -10, GETDATE())
   WHERE Id = [complaint_id]
   ```

2. **Check SLA Dashboard**
   - Navigate to `/Complaints`
   - ✅ Verify statistics show:
     - "On Track" (< 5 days old)
     - "At Risk" (5-7 days old)
     - "Overdue" (> 7 days old)
   - ✅ Verify color coding: Green → Yellow → Red

### Test Scenario 8: Access Control

1. **Tenant A** (Complainant)
   - ✅ Can view complaints they submitted
   - ✅ Cannot view complaints where they are respondent
   - ✅ Cannot view other complaints

2. **Tenant B** (Respondent)
   - ✅ Can view complaints where they are respondent
   - ✅ Cannot view complaints they submitted (separate test)
   - ✅ Cannot view other complaints

3. **CSO**
   - ✅ Can view all complaints
   - ✅ Can perform all management actions

4. **Unauthorized User**
   - ✅ Attempting to access `/Complaints/Details/[id]` for unauthorized complaint returns 403 Forbidden

### Test Scenario 9: Audit Trail (BR27)

1. **Perform Multiple Actions** on a complaint:
   - Submit complaint
   - Assign to CSO
   - Schedule appointment
   - Respondent confirms
   - Capture outcome

2. **View Audit Trail**
   - Navigate to complaint details
   - Scroll to "Audit Trail" section
   - ✅ Verify all actions logged with:
     - Action name
     - Details
     - User who performed action
     - Timestamp
   - ✅ Verify chronological order

---

## 📡 API Reference

### Public Endpoints

#### GET /Complaints
**Purpose**: List complaints for current user
**Authorization**: [Authorize] - any logged-in user
**Returns**: View with complaint list and SLA statistics

**Access Logic**:
- CSOs: See all complaints
- Tenants: See complaints they submitted + complaints where they are respondent

---

#### GET /Complaints/Create
**Purpose**: Display complaint submission form
**Authorization**: [Authorize] - any logged-in user
**Returns**: View with form

---

#### POST /Complaints/Create
**Purpose**: Submit new complaint
**Authorization**: [Authorize] - any logged-in user
**Parameters**:
- `TenantComplaint model` - complaint details
- `HttpPostedFileBase[] evidenceFiles` - evidence uploads (optional)

**Validations**:
- Complainant must be current user
- Respondent must exist
- Evidence files: max 10MB each, allowed extensions: .jpg, .jpeg, .png, .pdf, .doc, .docx, .mp4, .avi, .mov

**Process**:
1. Generate unique case reference (COMP-YYYY-MM-XXXXXX)
2. Save complaint with status "Submitted"
3. Save evidence files
4. Send acknowledgement email/SMS to complainant
5. Assign to CSO (round-robin)
6. Send assignment notification to CSO
7. Log audit trail
8. Redirect to complaint list

---

#### GET /Complaints/Details/{id}
**Purpose**: View complaint details
**Authorization**: [Authorize] - authorized users only
**Parameters**:
- `id` (int, encrypted) - complaint ID

**Access Control**:
- CSOs: Can view any complaint
- Tenants: Can view only if they are complainant or respondent
- Others: 403 Forbidden

**Returns**: View with full complaint details, SLA status, audit trail

---

#### GET /Complaints/ScheduleAppointment/{complaintId}
**Purpose**: Display appointment scheduling form
**Authorization**: [Authorize(Roles = "Client Services Officer")]
**Parameters**:
- `complaintId` (int, encrypted) - complaint ID

**Returns**: View with scheduling form

---

#### POST /Complaints/ScheduleAppointment
**Purpose**: Schedule investigation appointment
**Authorization**: [Authorize(Roles = "Client Services Officer")]
**Parameters**:
- `ComplaintInvestigation model` - appointment details

**Validations**:
- Appointment date must be in the future
- Time must be valid

**Process**:
1. Save investigation record
2. Update complaint status to "Awaiting Investigation"
3. Send appointment notification to respondent (email/SMS)
4. Log audit trail
5. Redirect to complaint details

---

#### GET /Complaints/ConfirmAppointment/{investigationId}
**Purpose**: Respondent confirms or proposes alternative appointment
**Authorization**: [Authorize] - respondent only
**Parameters**:
- `investigationId` (int, encrypted) - investigation ID

**Access Control**:
- Only the respondent can access this page
- Others: 403 Forbidden

**Returns**: View with confirmation form

---

#### POST /Complaints/ConfirmAppointment
**Purpose**: Submit appointment confirmation or alternative proposal
**Authorization**: [Authorize] - respondent only
**Parameters**:
- `int investigationId`
- `bool respondentConfirmed` - true if accepting appointment
- `DateTime? proposedAlternativeDate` - if proposing alternative
- `TimeSpan? proposedAlternativeTime` - if proposing alternative
- `string alternativeDateReason` - reason for alternative

**Process**:
1. If confirmed: Update `RespondentConfirmed = true`, send confirmation to CSO
2. If alternative: Save alternative details, send notification to CSO for approval
3. Log audit trail
4. Redirect to complaint details

---

#### POST /Complaints/ApproveAlternativeDate/{investigationId}
**Purpose**: CSO approves or rejects alternative appointment
**Authorization**: [Authorize(Roles = "Client Services Officer")]
**Parameters**:
- `investigationId` (int, encrypted) - investigation ID

**Process**:
1. Update `AlternativeApproved = true`
2. Update `AppointmentDate` and `AppointmentTime` to proposed values
3. Send updated appointment notification to respondent
4. Log audit trail
5. Redirect to complaint details

---

#### GET /Complaints/CaptureOutcome/{investigationId}
**Purpose**: Display outcome capture form
**Authorization**: [Authorize(Roles = "Client Services Officer")]
**Parameters**:
- `investigationId` (int, encrypted) - investigation ID

**Returns**: View with outcome form

---

#### POST /Complaints/CaptureOutcome
**Purpose**: Record investigation outcome
**Authorization**: [Authorize(Roles = "Client Services Officer")]
**Parameters**:
- `int investigationId`
- `string outcome` - "Resolved", "Referred", "Unresolved"
- `string findings` - investigation findings
- `bool issueWarningLetter` - whether to issue warning (Resolved only)
- `int? externalAgencyId` - agency ID (Referred only)
- `string referralReason` - reason (Referred only)
- `HttpPostedFileBase[] supportingDocuments` - evidence uploads

**Validations**:
- Outcome is required
- Findings are required
- Supporting documents: max 10MB each

**Process**:
1. Update investigation with outcome and findings
2. Save supporting documents
3. **If Resolved**:
   - Update status to "Resolved"
   - If `issueWarningLetter = true`:
     - Count existing warnings for respondent unit
     - Send warning letter (1st, 2nd, or FINAL WARNING)
     - Update `WarningLetterCount` and `LastWarningDate`
   - **If sub-letting** (BR26):
     - Trigger lease termination automatically
     - Set `LeaseTerminationTriggered = true`
     - Update lease status to "Lease Termination Pending"
4. **If Referred**:
   - Update status to "Referred"
   - Create external referral record
5. **If Unresolved**:
   - Update status to "Unresolved"
   - Trigger lease termination
   - Set `LeaseTerminationTriggered = true`
   - Update lease status to "Lease Termination Pending"
6. Send outcome notification to complainant
7. Log audit trail
8. Redirect to complaint details

---

#### GET /Complaints/GetComplaintTypes
**Purpose**: AJAX endpoint for cascading dropdowns
**Authorization**: [Authorize] - any logged-in user
**Parameters**:
- `categoryId` (int) - complaint category ID

**Returns**: JSON array of complaint types for the category

**Example Response**:
```json
[
  { "Id": 1, "Name": "Sub-letting" },
  { "Id": 2, "Name": "Violation of Complex Rules" }
]
```

---

## 📁 File Upload Configuration

### Allowed File Extensions
- Images: `.jpg`, `.jpeg`, `.png`
- Documents: `.pdf`, `.doc`, `.docx`
- Videos: `.mp4`, `.avi`, `.mov`

### File Size Limit
- **Maximum 10MB per file**

### Validation Logic
Located in `SaveEvidenceFiles()` and `SaveInvestigationDocuments()` methods:

```csharp
var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".mp4", ".avi", ".mov" };
var maxFileSize = 10 * 1024 * 1024; // 10MB
```

### Storage Location
Files are saved to:
```
~/Uploads/Complaints/Evidence/
~/Uploads/Complaints/InvestigationDocuments/
```

**Note**: Ensure these directories exist and have write permissions.

---

## 📧 Email/SMS Configuration

### Email Configuration
The module uses `EmailHelper` for sending emails. Ensure SMTP settings are configured in `Web.config`:

```xml
<system.net>
  <mailSettings>
    <smtp from="noreply@yourorganization.com">
      <network host="smtp.yourorganization.com" 
               port="587" 
               userName="smtp_user" 
               password="smtp_password" 
               enableSsl="true" />
    </smtp>
  </mailSettings>
</system.net>
```

### SMS Configuration
The module uses `SmsHelper` for sending SMS. Ensure SMS gateway configuration is set up according to your SMS provider.

### Notification Types

| Notification Type | Recipient | Trigger | Method |
|------------------|-----------|---------|--------|
| Acknowledgement | Complainant | Complaint submitted | `SendComplaintAcknowledgement()` |
| Assignment | CSO | Complaint assigned | `SendAssignmentNotification()` |
| Appointment | Respondent | Appointment scheduled | `SendAppointmentNotification()` |
| Confirmation | CSO | Respondent confirms/proposes | `SendAppointmentConfirmationToCSO()` |
| Outcome | Complainant | Investigation completed | `SendOutcomeNotification()` |
| Warning Letter | Respondent | Warning issued | `SendWarningLetter()` |

---

## ⚠️ Known Limitations

### 1. Working Days Calculation
- SLA engine counts calendar days, not working days
- Weekend exclusion not implemented
- Public holidays not considered
- **Recommendation**: Implement `WorkingDaysCalculator` utility class

### 2. File Upload Security
- File content validation not implemented (relies on extension checking)
- Antivirus scanning not integrated
- **Recommendation**: Integrate antivirus scanning service (e.g., ClamAV)

### 3. Appointment Conflict Detection
- No check for CSO appointment conflicts
- No calendar view for CSOs
- **Recommendation**: Implement appointment calendar with conflict detection

### 4. Warning Letter Escalation
- After 3 warnings, no automatic action taken
- Requires manual management intervention
- **Recommendation**: Implement escalation workflow to senior management

### 5. External Referral Tracking
- No integration with external agency systems
- No status updates from agencies
- **Recommendation**: Implement webhook/API integration for status updates

### 6. Bulk Operations
- No bulk assignment to CSOs
- No bulk status updates
- **Recommendation**: Implement bulk action features for administrators

### 7. Reporting
- No built-in reports (e.g., CSO performance, complaint trends)
- **Recommendation**: Implement reporting dashboard with charts

### 8. Mobile Responsiveness
- Views not optimized for mobile devices
- **Recommendation**: Implement responsive CSS or mobile app

---

## 🔧 Troubleshooting

### Issue: Complaint list is empty for CSO

**Cause**: User not properly assigned to "Client Services Officer" role

**Solution**:
```sql
-- Check role assignment
SELECT u.UserName, r.Name 
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'your.cso@example.com'

-- Assign role if missing
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (
    (SELECT Id FROM AspNetUsers WHERE UserName = 'your.cso@example.com'),
    (SELECT Id FROM AspNetRoles WHERE Name = 'Client Services Officer')
)
```

---

### Issue: Respondent cannot see complaints lodged against them

**Cause**: Incorrect property mapping in `ApplicationAllocatedProperty`

**Solution**:
- Verify `ApplicationAllocatedProperty` records exist for respondent unit
- Check `OfferedComplexId`, `BuildingName`, `SpaceUnitNumber` match complaint data
- Run query:
```sql
SELECT * 
FROM ApplicationAllocatedProperty aap
INNER JOIN PropertyLeaseApplications pla ON aap.PropertyApplicationId = pla.Id
WHERE aap.OfferedComplexId = [RespondentComplexId]
  AND aap.BuildingName = '[RespondentBlockNumber]'
  AND aap.SpaceUnitNumber = '[RespondentUnitNumber]'
```

---

### Issue: Emails/SMS not being sent

**Cause**: Email/SMS configuration missing or incorrect

**Solution**:
1. Check SMTP settings in `Web.config`
2. Test email connectivity:
```csharp
EmailHelper.SendEmail("test@example.com", "Test Subject", "Test Body");
```
3. Check application logs for errors
4. Verify firewall allows SMTP traffic on port 587/465

---

### Issue: Case reference numbers are not unique

**Cause**: Multiple submissions happening simultaneously

**Solution**:
- Add database unique constraint:
```sql
CREATE UNIQUE INDEX UX_TenantComplaints_CaseReferenceNumber 
ON TenantComplaints(CaseReferenceNumber)
```
- Implement retry logic in `GenerateCaseReferenceNumber()` (already included)

---

### Issue: File uploads fail with "file too large" error

**Cause**: IIS/ASP.NET file size limits

**Solution**:
Update `Web.config`:
```xml
<system.web>
  <httpRuntime maxRequestLength="10240" /> <!-- 10MB in KB -->
</system.web>

<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="10485760" /> <!-- 10MB in bytes -->
    </requestFiltering>
  </security>
</system.webServer>
```

---

### Issue: Sub-letting complaints not triggering lease termination

**Cause**: Complaint type key mismatch

**Solution**:
- Verify complaint type key matches constant:
```sql
SELECT * FROM ComplaintTypes WHERE [Key] = 'complaint_type_subletting'
```
- Check `ComplaintTypeKeys.SubLetting` constant in code
- Ensure `IsSubLettingComplaint()` method uses correct key

---

### Issue: SLA dashboard shows incorrect statistics

**Cause**: `DateSubmitted` field not populated

**Solution**:
- Check complaints have `DateSubmitted` values:
```sql
SELECT * FROM TenantComplaints WHERE DateSubmitted IS NULL
```
- Update migration script to set default:
```sql
UPDATE TenantComplaints 
SET DateSubmitted = CreatedDateTime 
WHERE DateSubmitted IS NULL
```

---

### Issue: Audit trail not showing actions

**Cause**: `ComplaintWorkflowEngine.LogAuditTrail()` not being called

**Solution**:
- Verify all controller actions call `LogAuditTrail()`
- Check `ComplaintAuditLogs` table exists:
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ComplaintAuditLogs'
```
- Run migration script if table missing

---

## 📝 Summary

This Complaints/Transgressions Module provides a complete end-to-end workflow for handling tenant complaints with:

✅ **7-day SLA tracking** (BR22, BR24)  
✅ **Automated CSO assignment** (round-robin)  
✅ **Warning letter automation** with 3-warning threshold (BR25)  
✅ **Sub-letting auto-eviction** (BR26)  
✅ **Comprehensive audit trail** (BR27)  
✅ **Alternative appointment proposals**  
✅ **External referral tracking**  
✅ **Email/SMS notifications** at all stages  
✅ **Role-based access control**  
✅ **Evidence file uploads**  

### Database Scripts Execution Order
1. **First**: `ComplaintsDatabaseMigration.sql` (adds columns, tables, indexes)
2. **Second**: `ComplaintsMasterData.sql` (inserts statuses, categories, types)

### Minimum Requirements for Testing
- ✅ Database scripts executed
- ✅ "Client Services Officer" role exists
- ✅ At least 1 CSO user assigned
- ✅ At least 2 tenant accounts (complainant + respondent)
- ✅ Email/SMS configuration completed

---

**For support or questions, contact the development team.**

Last Updated: December 2024  
Version: 1.0
