# Roles Consolidation Plan

## End Goal

Login with **Letting Officer** OR **Client Services Officer** -> see ALL work (previously split between LO + Housing Supervisor).

## Strategy

Keep both DB fields in sync. Write same CSO person to both `LettingOfficerId` AND `HousingSuperId`. Nothing breaks.

---

## HARD RULES — READ BEFORE MAKING ANY CHANGE

1. **DO NOT touch HumanSettlementApplication controllers or views** — that is a completely separate module used by a different team. Any change there is out of scope and risks breaking unrelated functionality.
2. **DO NOT update functionality we are not actively using** — if a feature, controller, or view is not part of UC01–UC015, leave it alone.
3. **ADDITIVE ONLY** — never remove or replace existing HS/LO checks. Only add CSO/LO alongside them.
4. **Scope is strictly PLM (Property Lease Management)** — the UC list in this document defines the boundary. Nothing outside it.

---

## Run History (13 files changed, branch: RenamedRoles)

### Keys & Infrastructure (DONE)
- `Keys/AppSettingKeys.cs` — Added `ClientServicesOfficer = "u_client_services_officer"`
- `Keys/IdentityAccessManagementKeys.cs` — Added `ClientServicesOfficer = "PLM ClientServicesOfficer"`
- `DataAccessLayer/IdentityManager.cs` — IAM role mapping: `LettingOfficer`, `HousingSupervisor`, AND `ClientServicesOfficer` all map to `"Client Services Officer"` identity role (fall-through switch)
- `C8.eServices.Mvc.csproj` — Updated

### Controllers (DONE)
- `AreaManagerController.cs` — EditArea GET: single CSO fetch + `ViewBag.CSOUsers`. EditArea POST + EditWorkAllocation POST: safe sync both fields
- `OpenLinksController.cs` — `GetBackOfficeId`: both branches return `LettingOfficerId`. Comments updated to CSO
- `PropertyLeaseApplicationController.cs` — `GetBackOfficeId`: both LF branches return `LettingOfficerId`. EditArea GET fallback changed from `"Letting Officer"` to `"Client Services Officer"`. AppSettings fallbacks changed from `HousingSupervisor` to `LettingOfficer` (lines ~10553, 10608, 10692, 11427). IsInRole checks at lines 2865, 2899, 2933, 3631, 4313, 4539 all now include `|| IsInRole("Letting Officer")`. Authorize attributes at lines 3621, 3691, 4277 now include `Letting Officer`. FindUsersInRole at line 3918 now unions CSO + LO results.
- `AccountController.cs` — Both login paths: CSO added to LO check, now sends CSO users to `UpdateLeaseDetails`. HS redirect to `PropertyLeaseInspections` unchanged.

### Views (DONE)
- `Views/AreaManager/EditArea.cshtml` — Single "CSO-Assign To" dropdown, `ViewBag.CSOUsers`
- `Views/PropertyLeaseApplication/EditArea.cshtml` — Same
- `Views/HumanSettlementApplication/EditArea.cshtml` — ?? Was changed but SHOULD NOT have been. This view is out of scope. Revert this file if possible.
- `Views/PropertyLeaseApplication/EHCComplexAreaDashboard.cshtml` — HS column removed, LO renamed to CSO

---

## Phase 2 — Remaining Work

### PRIORITY 1: Login Redirect (AccountController.cs) — DONE
| Line | Change Made |
|------|------------|
| 473 | CSO added to LO check — CSO lands on `UpdateLeaseDetails`. HS untouched. |
| 2543 | Same (duplicate login path) |

### PRIORITY 2: UC010 Schedule Inspection (was Housing Supervisor only)
This is the critical use case — was exclusively HS.

**HumanSettlementApplicationController.cs:**

| Line | Current | Change To |
|------|---------|-----------|
| 2084 | `IsInRole("Housing Supervisor")` | `IsInRole("Client Services Officer") \|\| IsInRole("Letting Officer")` |
| 2119 | `IsInRole("Housing Supervisor")` | Same |
| 5667 | `[Authorize(Roles = "Revenue Officer,Housing Supervisor")]` | `"Revenue Officer,Client Services Officer,Letting Officer"` |
| 5703 | `IsInRole("Housing Supervisor")` | `IsInRole("Client Services Officer") \|\| IsInRole("Letting Officer")` |

### PRIORITY 3: IsInRole checks in PropertyLeaseApplicationController.cs — DONE
| Line | Change Made |
|------|------------|
| 2865 | `IsInRole("Client Services Officer") \|\| IsInRole("Letting Officer")` |
| 2899 | Same |
| 2933 | Same |
| 3631 | Same |
| 4313 | `else if` version |
| 4539 | Added to Lease Official check |

### PRIORITY 4: Authorize attributes in PropertyLeaseApplicationController.cs — DONE
| Line | Change Made |
|------|------------|
| 3621 | `Letting Officer` added |
| 3691 | `Letting Officer` added |
| 4277 | `Letting Officer` added |

### PRIORITY 5: LeaseDetailsController.cs (UC015)

| Line | Current | Change To |
|------|---------|-----------|
| 1199 | `IsInRole("Letting Officer")` | Add `\|\| IsInRole("Client Services Officer")` |
| 1382 | `IsInRole("Housing Supervisor")` | `IsInRole("Client Services Officer") \|\| IsInRole("Letting Officer")` |
| 2370 | `IsInRole("Letting Officer")` | Add `\|\| IsInRole("Client Services Officer")` |

### PRIORITY 6: EditArea GET/POST duplicates (still old pattern)

**PropertyLeaseApplicationController.cs:**

| Line | What | Change |
|------|------|--------|
| 8401-8424 | EditArea GET: dual HS+LO fetch | Single CSO fetch (same as AreaManagerController) |
| 8431-8433 | EditArea POST: writes HousingSuperId separately | Safe sync both fields |

**? HumanSettlementApplicationController.cs EditArea — OUT OF SCOPE. DO NOT TOUCH.**

### PRIORITY 7: FindUsersInRole queries

| File | Line | Current | Change |
|------|------|---------|--------|
| `PropertyLeaseApplicationController.cs` | 3918 | `FindUsersInRole("Client Services Officer")` | Union with `"Letting Officer"` results |
| `RiskAssessmentOutcomesController.cs` | 226 | `FindUsersInRole("Letting Officer")` | Union with `"Client Services Officer"` results |
| `RiskAssessmentOutcomesController.cs` | 1094 | Same | Same |

### PRIORITY 8: Role filter admin queries (add "Client Services Officer")

| File | Lines |
|------|-------|
| `AreaManagerController.cs` | 1772, 1815, 2406, 2444 |
| `AccountController.cs` | 850, 874 |
| `ApplicationUserRoleController.cs` | 554, 719 |

### PRIORITY 9: Broad Authorize attributes (add "Client Services Officer" to lists)

| File | Count |
|------|-------|
| `ProfileController.cs` | 9 locations |
| `DocumentController.cs` | 8 locations |
| `FileController.cs` | 8 locations |

---

## Use Case Reference

| UC | Name | Old Role | New Role | Critical? |
|----|------|----------|----------|-----------|
| UC01 | Submit Application | Letting Officer | CSO | |
| UC02 | Upload App Fee Proof | Letting Officer | CSO | |
| UC04 | Risk Assessment | Letting Officer | CSO | |
| UC06 | Upload Deposit Proof | Letting Officer | CSO | |
| UC07 | Waiting List Confirm | Letting Officer | CSO | |
| UC09 | Tenant Training | Letting Officer | CSO | |
| UC010 | Schedule Inspection | **Housing Supervisor** | CSO | **YES** |
| UC011 | Conduct Inspection | Letting Officer | CSO | |
| UC014 | Generate Lease | Letting Officer | CSO | |
| UC015 | Sign Lease | Letting Officer | CSO | |

## Constants

```
AppSettingKeys.LettingOfficer        = "u_letting_officer"
AppSettingKeys.HousingSupervisor     = "u_housing_super_visor"
AppSettingKeys.ClientServicesOfficer = "u_client_services_officer"
IamKeys.LettingOfficer        = "PLM LettingOfficer"
IamKeys.HousingSupervisor     = "PLM HousingSupervisor"
IamKeys.ClientServicesOfficer = "PLM ClientServicesOfficer"
Identity roles: "Letting Officer", "Housing Supervisor", "Client Services Officer"
```

---

## Manual Find & Replace Instructions (VS Ctrl+H)

### STRATEGY: ADDITIVE ONLY
- Never remove existing HS or LO checks
- Only ADD new CSO/LO checks alongside existing ones
- HS code keeps working exactly as before
- CSO and LO users get the same access as HS/LO had

---

### FILE 1 — AccountController.cs
**Open file ? Ctrl+H ? Match case ON ? Regular expressions OFF**

**DO NOT touch the Housing Supervisor redirect** — HS users still go to `PropertyLeaseInspections`. That is correct and must stay.

**One operation only** — Add CSO to the existing Letting Officer login check (appears TWICE in file, replace both):

Find:
```
if ((UserManager.IsInRole(user.Id, "Lease Official")) || (UserManager.IsInRole(user.Id, "Letting Officer")))
```
Replace:
```
if ((UserManager.IsInRole(user.Id, "Lease Official")) || (UserManager.IsInRole(user.Id, "Letting Officer")) || (UserManager.IsInRole(user.Id, "Client Services Officer")))
```
Click **Replace All** ? expect **2 replacements** (one per login path)

> Result: CSO users now land on `UpdateLeaseDetails` alongside LO. HS users unchanged, still go to `PropertyLeaseInspections`.

**Total replacements in AccountController.cs: 2**

---

### FILE 2 — PropertyLeaseApplicationController.cs
**Open file ? Ctrl+H ? Match case ON ? Regular expressions OFF**

**Replace 1** — Standalone CSO IsInRole checks (hits lines ~2865, 2899, 2933, 3631):

Find:
```
if (User.IsInRole("Client Services Officer"))
```
Replace:
```
if (User.IsInRole("Client Services Officer") || User.IsInRole("Letting Officer"))
```

**Replace 2** — else-if CSO check (hits line ~4313):

Find:
```
else if (User.IsInRole("Client Services Officer"))
```
Replace:
```
else if (User.IsInRole("Client Services Officer") || User.IsInRole("Letting Officer"))
```

**Replace 3** — Lease Official + CSO check (hits line ~4539):

Find:
```
if ((User.IsInRole("Lease Official")) || (User.IsInRole("Client Services Officer")))
```
Replace:
```
if ((User.IsInRole("Lease Official")) || (User.IsInRole("Client Services Officer")) || (User.IsInRole("Letting Officer")))
```

**Replace 4** — Authorize attribute on PropertyTenantCommunication GET+POST (hits lines ~3621 and ~3691):

Find:
```
[Authorize(Roles = "Client Services Officer,Community Development Officer,Property Manager,Revenue Manager,RevenueOfficer")]
```
Replace:
```
[Authorize(Roles = "Client Services Officer,Letting Officer,Community Development Officer,Property Manager,Revenue Manager,RevenueOfficer")]
```

**Replace 5** — Authorize attribute on lease signing action (hits line ~4277):

Find:
```
[Authorize(Roles = "Revenue Officer,Client Services Officer")]
```
Replace:
```
[Authorize(Roles = "Revenue Officer,Client Services Officer,Letting Officer")]
```

**Replace 6** — FindUsersInRole for CSO inbox query (hits line ~3918):

Find:
```
IEnumerable<Int32> systemIdentityUsers = IdentityManager.FindUsersInRole("Client Services Officer").Select(a => a.SystemUserId);
```
Replace:
```
IEnumerable<Int32> systemIdentityUsers = IdentityManager.FindUsersInRole("Client Services Officer").Concat(IdentityManager.FindUsersInRole("Letting Officer")).Select(a => a.SystemUserId).Distinct();
```

**Replace 7** — EditArea GET: keep HS+LO lists but add CSOUsers ViewBag (hits lines ~8418-8421).
Find the two ViewBag lines that set BOUsers and LOUsers:

Find:
```
ViewBag.BOUsers = new SelectList(HSUsersList, "SystemUser.Id", "SystemUser.FullName");
```
Replace:
```
ViewBag.CSOUsers = new SelectList(LOUsersList, "SystemUser.Id", "SystemUser.FullName");
            ViewBag.BOUsers = new SelectList(HSUsersList, "SystemUser.Id", "SystemUser.FullName");
```
> This ADDS CSOUsers (pointing at LO list) while keeping the existing BOUsers and LOUsers intact.

**Replace 8** — EditArea POST: safe sync — write same person to both fields (hits line ~8432):

Find:
```
area.HousingSuperId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.NewBackOfficeUser).Id;
            area.LettingOfficerId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.LettingOfficerId).Id;
```
Replace:
```
var csoCustomerId = db.Customers.FirstOrDefault(x => x.SystemUserId == vm.LettingOfficerId).Id;
            area.LettingOfficerId = csoCustomerId;
            area.HousingSuperId = csoCustomerId;
```

**Total replacements in PropertyLeaseApplicationController.cs: 8**

---

### FILE 3 — LeaseDetailsController.cs (UC015)
**Open file ? Ctrl+H ? Match case ON**

**Replace 1** — Letting Officer lease signing checks (hits lines ~1199 and ~2370):

Find:
```
(User.IsInRole("Lease Official")) || (User.IsInRole("Letting Officer"))
```
Replace:
```
(User.IsInRole("Lease Official")) || (User.IsInRole("Letting Officer")) || (User.IsInRole("Client Services Officer"))
```

**Replace 2** — Housing Supervisor lease check (hits line ~1382). ADDITIVE: keep HS, add CSO+LO:

Find:
```
if (User.IsInRole("Housing Supervisor"))
```
Replace:
```
if (User.IsInRole("Housing Supervisor") || User.IsInRole("Client Services Officer") || User.IsInRole("Letting Officer"))
```

**Total replacements in LeaseDetailsController.cs: 3**

---

### FILE 4 — HumanSettlementApplicationController.cs
**? OUT OF SCOPE — DO NOT TOUCH THIS FILE.**
This controller belongs to a separate module. All UC010 PLM inspection changes are already handled in `PropertyLeaseApplicationController`.

---

### After all changes: Build solution and confirm 0 errors.

**Grand total across all files: ~13 replacements (3 files only)**

### UC test readiness after these changes:
| UC | Ready to test? | Notes |
|----|---------------|-------|
| UC01 | YES | Login + inbox works for CSO and LO |
| UC02 | YES | Document Authorize lists already include LO+HS |
| UC03 | YES | Bookkeeper only, no change needed |
| UC04 | YES | FindUsersInRole now includes both roles |
| UC05 | YES | Applicant only, no change needed |
| UC010 | YES | PLM inspection routing fixed via GetBackOfficeId (Phase 1) |
| UC014/015 | YES after FILE 3 | LeaseDetailsController replacements needed |
