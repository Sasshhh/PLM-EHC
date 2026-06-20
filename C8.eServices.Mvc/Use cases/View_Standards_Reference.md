# PLM EHC – Master View Standards & Pattern Reference

> **Purpose:** Single source of truth for view file conventions in `C8.eServices.Mvc`.  
> Always consult this file before creating or editing any `.cshtml` view.

---

## 1. Razor Compatibility Rules (.NET 4.x / ASP.NET 4.8)

| ❌ NOT SUPPORTED in Razor inline `@` | ✅ Use Instead |
|---|---|
| `@Model.Prop?.Child` | `@(Model.Prop != null ? Model.Prop.Child : "")` |
| `@(Model.Prop?.Child ?? "N/A")` | `@(Model.Prop != null && Model.Prop.Child != null ? Model.Prop.Child : "N/A")` |
| `@Model.Prop?.Child.ToString()` | `@(Model.Prop != null ? Model.Prop.Child.ToString() : "")` |

**Rule:** The `?.` null-conditional operator is **never allowed** in Razor inline expressions in this project. Always use explicit `!= null` ternary checks.

---

## 2. Block Scope Nesting & Parser Errors (Crucial)

When writing C# inside markup nested under a helper like `@using (Html.BeginForm(...))`, nested block constructs (like `@if`) can trigger ASP.NET Razor parser errors (`Unexpected "if" keyword after "@"`). 

### ❌ Wrong — Triggers Parser Error
```html
@using (Html.BeginForm(...))
{
    @Html.AntiForgeryToken()
    @if (Model.LeaseDetails != null)
    {
        <input type="hidden" name="id" value="@Model.LeaseDetails.Id" />
    }
}
```

### ✅ Correct — Flat Inline Ternary
```html
@using (Html.BeginForm(...))
{
    @Html.AntiForgeryToken()
    <input type="hidden" name="id" value="@(Model.LeaseDetails != null ? Model.LeaseDetails.Id : 0)" />
}
```

---

## 3. Button Patterns

### ✅ Correct — SweetAlert confirmation + form submit
```html
<!-- One button only: type="button" with JS handler -->
<button type="button" id="btnSubmitAction" class="btn btn-success" onclick="clickSubmit()">Submit</button>
&nbsp;
@Html.ActionLink("Cancel", "BackAction", "ControllerName", null, new { @class = "btn btn-default" })
```
```javascript
function clickSubmit() {
    swal({ icon: "info", title: "Are you sure?", buttons: ['No!', 'Yes, I am sure!'] })
    .then(function (isConfirm) {
        if (isConfirm) {
            document.getElementById('myFormId').submit();
        }
    });
}
```

### ❌ Wrong — Hidden submit leaks as visible box
```html
<!-- NEVER do this — the hidden attribute causes a rendering artifact in Bootstrap -->
<input type="button" onclick="clickSubmit()" class="btn btn-primary" />
<input type="submit" style="display:none;" hidden id="btnApproval" class="btn btn-primary" />
```

**Rule:** Always give the form an `id`. Use `document.getElementById('formId').submit()`. Never use a secondary hidden submit button.

---

## 3. Standard View Structure (UC023 Termination Pattern)

Every termination review view follows this 4-section layout:

```
Section 1: Evaluating Application  ← PropertyLeaseApplication info
Section 2: Tenant Lease Details    ← LeaseDetails info  
Section 3: Termination Details     ← LeaseTermination record (date, reason, who)
Section 4: Action / Decision Form  ← Role-specific form (CSO, Revenue Officer, etc.)
```

### Data Sources per Section

| Section | Model / ViewBag Source | Loaded In |
|---|---|---|
| Application info | `vm.PropertyLeaseApplications` (include `.Customer`, `.Status`, `.PurchaserType`) | Controller GET |
| Lease Details | `vm.LeaseDetails` (include `.Status`, `.PurchaserType`) | Controller GET |
| Termination Details | `vm.LeaseTermination` (from `db.LeaseTerminations` by `PropertyLeaseApplicationId`) | Controller GET |
| Date / Reason | `ViewBag.TerminationDate`, `ViewBag.TerminationReason` | Controller GET |
| Decision dropdowns | `ViewBag.ApprovalStatus` as `SelectList` | Controller GET |

---

## 4. UC023 Termination Workflow — Role → Action → View Map

| Step | Status Key | Role | View (Controller/Action) | Button Label |
|---|---|---|---|---|
| S1 – Tenant serves notice | `s_awaiting_cso_termination_review` | Client Services Officer | `PropertyLeaseApplication/TerminationCSOReviewDetails` | **Review Notice** (btn-warning) |
| S2a – CSO approves | `s_awaiting_termination_appraisal` | Revenue Officer | `PropertyLeaseApplication/PropertyEvictionValidation` | **Authorize Termination** (btn-success) |
| S2b – Revenue Officer action | (next step) | Revenue Manager | TBD | TBD |

### What Creates the `LeaseTermination` Row
The `ApplicationLeaseServeNotice` POST (line ~12438 of `PropertyLeaseApplicationController.cs`) creates the `LeaseTermination` record. **This must always be created here** — all downstream views depend on it.

---

## 5. ViewModel Usage

```csharp
// Standard termination review ViewModel
var vm = new DepartmentsApprovalViewModel
{
    LeaseDetails              = lease,                    // LeaseDetails (with Status, PurchaserType)
    PropertyLeaseApplications = rcsApps,                 // PropertyLeaseApplication (with Customer, Status)
    LeaseTermination          = termination,             // LeaseTermination (reason, date, ref)
    Customer                  = custmuser                // Customer who created termination
};
ViewBag.TerminationDate   = termination != null ? termination.TerminationDate.ToString("dd MMMM yyyy") : "Not set";
ViewBag.TerminationReason = termination != null ? termination.ReasonForTermination : "Not captured";
ViewBag.ApprovalStatus    = new SelectList(db.RCSActionTypes.Where(...), "Key", "Name");
```

---

## 6. `DepartmentsApprovalViewModel` Properties (Used in Termination Views)

| Property | Type | Purpose |
|---|---|---|
| `LeaseDetails` | `LeaseDetails` | Lease-level data (ref no, status, dates) |
| `PropertyLeaseApplications` | `PropertyLeaseApplication` | Application-level data (ref no, customer) |
| `LeaseTermination` | `LeaseTermination` | Termination-specific data (date, reason, ref) |
| `Customer` | `Customer` | Who submitted/created the termination |

---

## 7. Nav Sidebar — Role → Termination Link

| Role | Nav Section | Link |
|---|---|---|
| Client Services Officer | `Termination` submenu (line ~1014 of RCS_Layout) | `PropertyLeaseApplicationTerminations` |
| Revenue Officer | `Termination` submenu (line ~1511 of RCS_Layout) | `PropertyLeaseApplicationTerminations` |
| Revenue Manager | `Termination` submenu (added ~line 1459 of RCS_Layout) | `PropertyLeaseApplicationTerminations` |

**All three roles land on the same `PropertyLeaseApplicationTerminations` list view.** The controller filters by role + RRQ entries + status.

---

## 8. Controller Filter Pattern (`PropertyLeaseApplicationTerminations`)

Each role branch:
1. Gets the correct `ResponsibilityTypeId` from `ResponsibilityTypes` table
2. Queries `RoundRobinQueues` for their assigned active jobs
3. Filters `LeaseDetails` by status IDs relevant to that role's stage

| Role | ResponsibilityType Key | Status Keys Shown |
|---|---|---|
| Revenue Officer | `r_termination_validation` | `s_awaiting_tenant_account_balance_review`, `s_end_of_lease_term`, `s_lease_not_renewed`, `s_tenant_notice`, `s_awaiting_termination_appraisal` |
| Client Services Officer | `r_terminations` | `s_awaiting_cso_termination_review` |

---

## 9. Key Status Key Constants (`StatusKeys.cs`)

| Constant | DB Value |
|---|---|
| `AwaitingCSOTerminationReview` | `s_awaiting_cso_termination_review` |
| `AwaitingTerminationAppraisal` | `s_awaiting_termination_appraisal` |
| `TerminationNotSupported` | (existing key) |

---

## 10. Encryption Pattern for Query Strings

All action links use `AesCrypto` + `DecryptParameter` attribute:

```csharp
// In view — encrypting id parameter
@Html.ActionLink("Review Notice", "TargetAction", "ControllerName",
    new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + item.Id.ToString()) },
    new { @class = "btn btn-warning" })

// In controller — decorated action automatically decrypts
[DecryptParameter]
public ActionResult TargetAction(int? id) { ... }
```

**`id` in the query string = `LeaseDetails.Id`** for termination actions.
