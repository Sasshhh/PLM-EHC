# UC17D — Service Request Workflow Knowledge Base

## Overview
Service Requests allow tenants to log maintenance or support requests against their unit. Upon submission, the system automatically routes the request to the **Letting Officer** of the tenant's complex using the **Round Robin Queue**. Each lifecycle stage has a corresponding queue entry that is opened and closed as the status progresses.

---

## Roles & Permissions

| Role | Can Create | Queue Visibility | Can Change Status |
|---|---|---|---|
| **Tenant** | ✅ (own complex only) | Own requests only | ❌ |
| **Client Services Officer (CSO)** | ✅ | Assigned to them via complex routing only | ✅ |
| **Back Office System Administrator** | ✅ | All requests | ✅ |
| **Super Administrator** | ✅ | All requests | ✅ |

---

## Workflow Lifecycle

```
TENANT SUBMITS → [Open] → Assigned to Complex LettingOfficer
                              ↓
                        CSO moves to [In Progress]
                              ↓
                        CSO moves to [Resolved] → Email + SMS to Tenant → Redirect to Index
```

### Status Keys (`ServiceRequestStatusKeys`)
| Status | Key |
|---|---|
| Open | `sr_status_open` |
| In Progress | `sr_status_in_progress` |
| Resolved | `sr_status_resolved` |
| Closed | `sr_status_closed` |
| Deleted | `sr_status_deleted` |

---

## Complex-Based Routing (BR - UC17D)

- On **Create**, `ServiceRequestEngine.AssignToLettingOfficer()` is called.
- Looks up `PreferredComplexArea` by `ComplexId`.
- Uses `PreferredComplexArea.LettingOfficerId` as the assigned CSO.
- If no LettingOfficer is configured for the complex, falls back to the global `AppSettings["u_letting_officer"]` value.
- Sets `ServiceRequest.AssignedToId` and `ServiceRequest.DateAssigned`.
- Creates a `RoundRobinQueue` entry with `ResponsibilityTypeKey = "r_service_request_open"`.
- Queues an assignment email to the Letting Officer's system user email address.

---

## Round Robin Queue Transitions

| Status Change | Queue Action |
|---|---|
| **Submitted → Open** | New RR entry created (`r_service_request_open`) |
| **Open → In Progress** | Close `r_service_request_open`, open `r_service_request_in_progress` |
| **In Progress → Resolved** | Close `r_service_request_in_progress` |
| **Resolved** | No further queue entries. Tenant notified via Email + SMS. CSO redirected to Index. |

### Responsibility Type Keys Added
```csharp
ResponsibilityTypeKeys.ServiceRequestOpen       = "r_service_request_open"
ResponsibilityTypeKeys.ServiceRequestInProgress = "r_service_request_in_progress"
```

> **DB SEED REQUIRED** — Run the following SQL before first use:
> ```sql
> INSERT INTO ResponsibilityTypes (Name, Key, IsActive, IsDeleted, DepartmentId, CreatedDateTime)
> VALUES
>   ('Service Request - Open', 'r_service_request_open', 1, 0, 1, GETDATE()),
>   ('Service Request - In Progress', 'r_service_request_in_progress', 1, 0, 1, GETDATE());
> ```

---

## Notifications

| Trigger | Recipient | Channel |
|---|---|---|
| SR Created | Assigned Letting Officer | Email |
| SR Resolved | Tenant (ReportedByName, EmailAddress) | Email + SMS |

---

## Business Rules

| Rule | Description |
|---|---|
| **BR37** | Only the creator (tenant) can edit or delete a Service Request, and only while status = Open |
| **BR36** | Initial status at creation is always Open |
| **BR34** | SLA deadlines calculated based on priority; escalation triggered at 50% elapsed time |
| **BR23** | Maintenance requests: SLA resolution = 30 working days |
| **BR24** | Non-maintenance requests: SLA resolution = 7 working days |
| **UC17D Routing** | On submit, SR is routed to `PreferredComplexArea.LettingOfficerId`. Falls back to global AppSetting if not set. |

---

## Model Fields Added (Migration Required)

### `ServiceRequest`
| Field | Type | Purpose |
|---|---|---|
| `AssignedToId` | `int?` FK → `Customer` | The Letting Officer assigned at submission |
| `DateAssigned` | `DateTime?` | When the assignment was made |

### `RoundRobinQueue`
| Field | Type | Purpose |
|---|---|---|
| `ServiceRequestId` | `int?` FK → `ServiceRequest` | Links the queue entry to a Service Request |

> **DB MIGRATION REQUIRED** — Add these columns via EF migration or direct SQL:
> ```sql
> ALTER TABLE ServiceRequests ADD AssignedToId INT NULL, DateAssigned DATETIME NULL;
> ALTER TABLE RoundRobinQueues ADD ServiceRequestId INT NULL;
> ALTER TABLE RoundRobinQueues ADD CONSTRAINT FK_RRQ_ServiceRequest FOREIGN KEY (ServiceRequestId) REFERENCES ServiceRequests(Id);
> ```
