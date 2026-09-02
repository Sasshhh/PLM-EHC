# Service Requests Module (UC17D/UC17E)

## Overview
The Service Requests module enables tenants and Client Services Officers to create, view, edit, and delete service request tickets within the Property Lease Management system.

- **UC17D** - Capture Service Request (create new tickets)
- **UC17E** - Edit Service Request (view, edit, delete existing tickets)

## Business Rules Implemented

| Rule ID | Description | Implementation |
|---------|-------------|----------------|
| BR23 | Maintenance complaints resolved within 30 working days | `ServiceRequestEngine.CalculateSlaDeadlines()` |
| BR24 | Non-maintenance complaints resolved within 7 working days | `ServiceRequestEngine.CalculateSlaDeadlines()` |
| BR34 | Tiered SLA escalation at 50% time breach | `ServiceRequestEngine.ShouldEscalate()` |
| BR35 | Only authenticated users can create tickets | `[Authorize]` attribute on controller |
| BR36 | Statuses: Open, Deleted, In Progress, Resolved, Closed | Status master data + workflow |
| BR37 | Edit/Delete only when status is Open | `ServiceRequestEngine.CanEditOrDelete()` |

### Priority SLA Times (BR34)
| Priority | Level | Response Time | Resolution Time |
|----------|-------|---------------|-----------------|
| Emergency/Critical | 1 | 30 minutes | 24 hours |
| High | 2 | 1 hour | 72 hours |
| Medium | 3 | 24 hours | 5 business days |
| Low | 4 | 48 hours | 30 business days |

## Architecture

### Models
| File | Description |
|------|-------------|
| `Models/ServiceRequest.cs` | Main entity with all ticket fields |
| `Models/ServiceRequestCategory.cs` | Categories (Maintenance, Specialised Care, Compensation/Disputes) |
| `Models/ServiceRequestPriority.cs` | Priorities with SLA response/resolution times |
| `Models/ServiceRequestDocument.cs` | Uploaded document attachments |
| `Models/ServiceRequestAuditLog.cs` | Audit trail / history log |

### Engine
| File | Description |
|------|-------------|
| `Engines/ServiceRequestEngine.cs` | Ticket reference generation (EHC_SR_###_YYYY), SLA calculation, escalation checks, status updates, audit logging |

### Controller
| File | Description |
|------|-------------|
| `Controllers/ServiceRequestsController.cs` | All HTTP endpoints for UC17D and UC17E |

### Keys
| File | Description |
|------|-------------|
| `Keys/ServiceRequestKeys.cs` | Constants for category keys, priority keys, and status keys |

## Controller Actions

| Action | Method | Description | Access |
|--------|--------|-------------|--------|
| `Index` | GET | List service requests (tenant sees own, CSO sees all) | All authenticated |
| `Create` | GET/POST | Capture new service request (UC17D) | All authenticated |
| `Details` | GET | View request details, history, attachments | Creator or CSO |
| `Edit` | GET/POST | Edit open request (UC17E) | Creator only, Open status only |
| `Delete` | POST | Soft-delete open request (UC17E) | Creator only, Open status only, reason required |
| `UpdateStatus` | POST | Change request status | CSO only |

## Ticket Reference Format
`EHC_SR_###_YYYY` (e.g., EHC_SR_001_2025)

## Database Tables
1. `ServiceRequestCategories` - 3 categories
2. `ServiceRequestPriorities` - 4 priority levels with SLA times
3. `ServiceRequests` - Main tickets table
4. `ServiceRequestDocuments` - File attachments
5. `ServiceRequestAuditLogs` - Activity history

## Database Scripts
1. **Run first:** `MasterData/ServiceRequestsDatabaseMigration.sql` - Creates tables and indexes
2. **Run second:** `MasterData/ServiceRequestsMasterData.sql` - Inserts reference data (status type, 5 statuses, 3 categories, 4 priorities)

## Key Constraints
- Only the **creator** of a service request can edit or delete it (UC17E Note 1)
- Edit and delete are only available when status is **Open** (BR37)
- Deletion requires a mandatory **reason** (UC17E Step 9)
- SLA escalation is automatically triggered at **50%** of resolution time (BR34)
- Email notification is sent on ticket creation with the reference number (UC17D Step 20)
