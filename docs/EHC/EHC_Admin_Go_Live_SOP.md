---
title: "EHC Property Lease Management: Go-Live Administration SOP"
subtitle: "Users, roles, work routing and the scheduled checks the administrator must run"
date: "10 November 2026"
---

# About this document

This SOP is for the **Back Office System Administrator** and the **Area Manager** of the Ekurhuleni Housing Company (EHC). It covers what must be set up before EHC users can work in the PLM system, and the checks the administrator must run on a schedule, because the build packaged on 3 November 2026 (`e27d7f4`) does not run them by itself. It was written from the factory test findings (*DEFECT_REGISTER.md*, *TESTER_GUIDE_Users_By_Use_Case.md*) and the application code.

Day-to-day procedures for each role are in the *EHC SOPs*.

# 1. Before go-live: set up users and roles

## SOP-G1 Create and assign staff users

1. Click **Manage Users** (Back Office System Administrator, or Area Manager for their own area).
2. For each staff member, create or find the user and make sure their department is **Ekurhuleni Housing Company**. A user in the wrong department will not see the EHC menus, and the Real Estate menus are hidden from EHC users.
3. Assign the system role from the table below. One person may hold more than one role.

| Job | Role to assign |
|---|---|
| Client Services Officer | Client Services Officer |
| Client Services Supervisor | Housing Supervisor |
| Bookkeeper / Financial Officer | Revenue Officer |
| Revenue Manager | Revenue Manager |
| CEO | Property Manager **and** CEO |
| Maintenance Manager | Maintenance Manager |
| Property & Facilities Manager | Property & Facilities Manager |
| Area Manager | Area Manager |
| System administrator | Back Office System Administrator |

4. Ask each user to sign out and back in after a role change; the menus are built at sign-in.

Accounts flagged as Active Directory users sign in with their network password. Local accounts get a temporary password and are sent to *Manage Account* to change it on first sign-in.

## SOP-G2 Give the CEO the CEO role

The CEO approves applications, leases, renewals and evictions under the **Property Manager** role, but the deposit refund authorisation (**Termination › Refund Authorisations**) needs the separate **CEO** role. In the factory database the CEO role had no holders, so refunds could not be authorised (D038).

1. Run `PLM-CEO-Setup.sql` from the release folder, after replacing the placeholder with the CEO's real username. It creates the CEO role if missing, grants it, and sets the `s_ceo` setting (SOP-G3).
2. Ask the CEO to sign out and back in. **Termination › Refund Authorisations** must now appear.

# 2. Before go-live: set up work routing

Each workflow step is routed to a named user. Some steps go to a user named in the **AppSettings** table; new applications go to the letting officer of the applicant's preferred complex. If routing points at the wrong person, the work silently lands in that person's queue.

## SOP-G3 Check the queue owners in AppSettings

Run `PLM_Inspect_TargetDatabase.sql` (read only) on the production database. It lists every queue owner. Each of these settings must hold the customer id of a real, active user who holds the matching role:

| AppSettings key | Must point at a user with role | Used for |
|---|---|---|
| `r_property_manager` | Property Manager (the CEO) | CEO approvals |
| `r_revenue_manager` | Revenue Manager | Revenue Manager steps |
| `r_revenue_officer` | Revenue Officer | Fee and deposit validation, termination appraisal |
| `s_ceo` | CEO | Deposit refund authorisation |
| `u_housing_super_visor` | Housing Supervisor | Inspections |
| `u_letting_officer` | Letting Officer / CSO | Letting steps |
| `u_maintenance_manager` | Maintenance Manager | Job cards |
| `u_property_facilities_manager` | Property & Facilities Manager | Job card approval |

In the factory database two of these were wrong: `u_housing_super_visor` pointed at a Property Manager and `u_letting_officer` at a Property & Facilities Manager (D011, D012). Confirm the right person for each queue with the business (decision DEC-04) and correct the values before go-live.

## SOP-G4 Give every complex a letting officer

New applications are routed to the letting officer of the applicant's preferred complex (`PreferredComplexAreas.LettingOfficerId`). In the factory database **19 of the 28 active complexes had no letting officer**, so applications for those complexes reached nobody.

1. As Area Manager, click **Work Queues › Complex Area Assigning**.
2. Assign an officer to every active complex.
3. Re-run `PLM_Inspect_TargetDatabase.sql` and confirm no active complex is unassigned.

## SOP-G5 Check the round robin

**Manage RoundRobin** (Back Office System Administrator) lists the users who receive work automatically. Make sure every active officer is in it and nobody who has left is. Use **Work Queues › Manual Work Allocation** or **Bulk Work Allocation** to move work from a user who is away.

## SOP-G6 Load document templates and settings

1. **Content Management › Document Templates**: check the lease agreement, renewal agreement, debit order authority, eviction notice and cancellation templates are loaded.
2. **Content Management › Email Content** and **Message Content**: check the notification wording.
3. After a database restore, update the AppSettings paths and URLs (document folders, report server) for the new server. See the deployment kit README.

# 3. Running the system: scheduled checks

In this build, four time-based business rules only run when someone presses a button on the administrator's **Tools** menu. Until they are moved into a scheduled job, the administrator must run them.

**Use only the Run buttons.** The Tools screens also have **Backdate**, **Send & Activate** and **Build History** buttons that rewrite dates or add demo records on live data. They exist for testing and must never be used on production data.

| How often | Tools screen | Button | What it does | Rule |
|---|---|---|---|---|
| Weekly (Monday) | **Tools › Backdate Active Lease** | **Run Lease Renewal Scanner** | Flags every lease within three months of expiry for renewal and notifies the CSOs. Without it, no lease ever enters the renewal flow (D044) | Renewal notice date |
| Weekly | **Tools › Unit Offer Expiry** | **Run BR09 Unit Offer Checker** | Withdraws unit offers not accepted within 30 days and returns the applicant to the waiting list | BR09 |
| Weekly | **Tools › Lease Signing Expiry** | **Run BR19 Lease Signing Checker** | Disregards applications whose lease agreement was not signed within 30 days | BR19 |
| Monthly | **Tools › Backdate Waiting List** | **Run 5-Month Re-Entry Checker** | Asks waiting-list applicants to confirm they still need a unit. (It also runs when applicants open their Inbox.) | UC09 |

After running the renewal scanner, check **Renewals › Lease Review** as a CSO: the newly flagged leases show *In Awaiting Application Lease Renewal Review*.

# 4. Running the system: recover stuck work

| Situation | What to do |
|---|---|
| An item is in the wrong person's queue | **Work Queues › Manual Work Allocation**: move it |
| A CSO chose **Not Vacated** on Confirm Move-Out and the lease disappeared (D051) | The lease status must be put back by a database fix; log a support ticket with the reference number. Tell CSOs not to use Not Vacated |
| A refund is stuck at *Awaiting Refund Authorisation* | Check the CEO holds the CEO role and that `s_ceo` is the CEO's customer id (SOP-G2) |
| The Revenue Manager cannot find the refund response queue | Give them the **Update Refund Response** link (*/PropertyLeaseApplication/UpdateRefundResponse*); the menu item is only in the CSO navigation |
| A user cannot sign in after a role change | Ask them to sign out and back in; check the account is not locked |

# 5. Audit and reports

* **Audit Trail** (Active Users & Login History, Activities / Transactions, Admin Activities, Password Reset Trail, Role Modifications) shows who did what. Rows are filtered by department, so EHC administrators see EHC activity.
* **Reports › Application Report** and **Reports › New Lease Report** are the EHC reports. **PLM Applications › All Leases** lists every lease with its status.
