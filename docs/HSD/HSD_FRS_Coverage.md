---
title: "HSD Property Lease Management: FRS Coverage"
subtitle: "What the system covers against the HSD FRS (Version 3)"
date: "11 November 2026"
---

# About this document

This document lists, for each item in the HSD FRS, whether the system covers it, partially covers it or does not cover it. It ends with the bugs found in the current system. How each screen works step by step is in *HSD_Use_Cases_As_Built*.

- **Covered:** the system does what the FRS asks.
- **Partially covered:** the screen or step exists, but part of the requirement is missing or works differently.
- **Not covered:** nothing in the system does this.

# Summary

| Area | Covered | Partially covered | Not covered |
|---|---|---|---|
| Functional requirements (27) | 12 | 13 | 2 |
| Business rules (15) | 7 | 4 | 4 |
| Use cases (21) | 12 | 9 | 0 |
| Reports and integration (8) | 0 | 0 | 8 |

The core lease flow works end to end: capture, the agreement chain, renewal, notice, termination, exit inspection and vacating are all on screens with work queues. The main gaps are:

1. **No reports.** None of IR1 to IR8 exist for HSD.
2. **No finance link.** Finance is not told about a new lease (FR4.7), the rental amount is not calculated from the tariff (FR5.1, BR18), arrears cannot be captured (FR13.5), and there is no SOLAR account balance.
3. **Renewals do not start by themselves.** Leases only enter the renewal queue when someone opens a renewal page (FR7.1).
4. **Bugs.** 14 bugs were found, 5 of them high severity. Some lease outcomes are routed wrongly or drop out of every list. See *Bugs found* at the end.

# Functional requirements

| FR | Requirement | Rating | What the system does today |
|---|---|---|---|
| FR3.5 | Conduct unit inspection and upload supporting documents and pictures | Partially covered | Post-Inspection lets the HLO download the inspection template, upload the completed form and an additional document, and pick an outcome. There is no photo upload slot and no comment field. All three outcomes lead to the same next step. |
| FR4.1 | Capture lease details: property, period, amount, billing cycle, maintenance responsibilities, escalations | Partially covered | Capture Master captures the tenant, spouse, contact person, unit, dates and billing amounts. There are no fields for billing cycle, maintenance responsibilities or escalations. The Occupants and Business Activities sections are hidden. |
| FR4.2 | Generate lease agreement | Covered | Agreements › Generate Agreement creates the PDF from the HSD lease template. It generates as soon as the button is clicked, with no confirmation. |
| FR4.3 | Senior Housing Specialist reviews the generated agreement | Covered | Agreements › Agreement Review, with Approve or Reject (reason required). |
| FR4.4 | Senior Manager Operations approves the agreement | Covered | Agreements › Agreement Approval, done by the Regional Manager role. The Regional Manager can approve without first applying their signature. |
| FR4.5 | Tenant validates and accepts the agreement | Covered | Agreements › Agreement Signature for the HLO, or the e-mailed link for the tenant, both protected by a 4-digit OTP. |
| FR4.6 | Upload signed lease agreement | Covered | Agreements › Finalized Agreement. Submit is blocked until the final agreement is uploaded. |
| FR4.7 | Notify Finance of the new lease | Not covered | Nothing is sent to Finance when the signed agreement is uploaded or when a lease is captured. |
| FR4.8 | Record personal details for tenants and occupants, including a photograph and proof of identity | Partially covered | Department Tenants › Manage Tenants records occupants and takes lease documents including an identity document. There is no photograph field or document type, and documents are held per lease, not per occupant. |
| FR4.9 | Transfer a lease on death or incapacity | Covered | Department Tenants › Transfer Lease, with reasons *Tenant Deceased* and *Other*. Incapacity is handled under *Other*. |
| FR4.10 | Officials capture the tenant transfer | Partially covered | The HLO captures and completes the transfer in one step. There is no review or approval and no history of the previous main tenant on the lease record, because the tenant details are overwritten. |
| FR5.1 | Capture billing information including penalties and rental escalations | Partially covered | Rental amount, deposit, VAT (15%), store rooms and parking are captured. The rental amount is typed in, not calculated from the tariff. There are no penalty or escalation fields. |
| FR7.1 | Automatic notification three months before the lease ends | Partially covered | The tenant is notified and the lease enters the renewal queue from the Renewal Notice date, which is two months before the end month, not three. This only happens when a user opens a renewal list page. Nothing runs on a schedule. |
| FR7.2 | HLO recommends renewal | Covered | Agreement Renewal › Recommendations, with outcome and comment. The account balance panel shows the deposit, not the real balance, and the complaints table is always empty. |
| FR7.3 | Senior Housing Specialist reviews renewal | Covered | Agreement Renewal › Pending Review (*Seconded* / *Not Seconded*). Both outcomes go to the Regional Manager. |
| FR7.4 | Senior Manager Operations approves renewal | Partially covered | Agreement Renewal › Pending Approval exists, but the routing is wrong for *Not Renew* recommendations and for second renewals. See BUG-01 and BUG-02. |
| FR7.5 | HLO or tenant (self-service) indicates whether to continue or terminate | Partially covered | Agreement Renewal › Pending Acceptance works with an OTP. The tenant e-mail link opens but cannot submit (BUG-03). When the tenant declines, the lease is marked as ending at period end, but no work item is created, so no one is prompted to terminate it. |
| FR7.6 | Upload the final renewed lease; tenant submits latest documents | Covered | Agreement Renewal › Pending Renewal takes the six documents, extends the lease and sends it back through the agreement chain (Generate to Finalized Agreement). |
| FR8.1 | Officials capture notice to terminate | Covered | Notice › Serve Notice records the notice date, with the one-calendar-month and last-day-of-month rules. |
| FR8.2 | Capture termination details, including date and reason | Partially covered | Terminations › Terminate Agreement captures date and reason. The FRS reason *End of lease term (60th month)* is not offered in the drop-down. |
| FR8.3 | Review termination and validate account | Partially covered | Terminations › Pending Review takes financial documents and approves. The account balance shown is the deposit only (no SOLAR balance), and there is no Reject option. |
| FR8.4 | Confirm the tenant has vacated | Partially covered | Terminations › Pending Take-Off records *Vacated*. *Not Vacated* closes the work item and the lease drops out of every list (BUG-07). Nobody is told the unit is free. |
| FR8.5 | Capture a serve notice | Covered | Terminations › Noticed Agreements lists the noticed leases and carries them into termination with reason *Tenant Notice*. |
| FR9.1 | Create individual letters, texts or e-mails ad hoc | Partially covered | Communication › New Communication sends SMS and e-mail to one tenant or whole complexes. The *Postal* tick box does nothing, so no letter is produced. |
| FR9.2 | Store copies of communications against the record | Partially covered | Individual messages are copied to the lease's history log. Complex-wide messages and system messages are only in the outgoing mail/SMS queue and cannot be viewed from the lease. |
| FR13.4 | Login | Covered | Username and password, with optional Active Directory login. The WSO2/IAM option in the FRS is no longer used. |
| FR13.5 | Capture arrears: amount and days in arrears | Not covered | There is no screen or field for arrears. |

# Business rules

| BR | Rule | Rating | What the system does today |
|---|---|---|---|
| BR1 | Lease period defaults to 24 months with an option to renew | Covered | Capture Master sets the end date 24 months from the start. Renewal adds 24 months (12 for Dormitory). |
| BR3 | A lease can only be transferred to an existing occupant | Covered | The *Preference* list only offers active occupants and the spouse on the lease. |
| BR4 | For *Other*, the previous main tenant can become an occupant, but not when deceased | Covered | The *Make main tenant an occupant?* tick box only appears for *Other*. |
| BR5 | Individual communication addresses the tenant by name and surname | Covered | Individual messages open with the tenant's full name. |
| BR6 | Group communication addresses tenants by their complex | Partially covered | Complex messages go to the right tenants but open with "Tenants", not the complex name. |
| BR7 | Officials only see complexes and tenants they are responsible for | Covered | Complex and tenant lists are filtered by the user's work allocations. |
| BR8 | Only family units, retirement flats and hostels can be renewed | Not covered | The category filter in the renewal check is commented out, so every HSD lease is put up for renewal. |
| BR10 | Preliminary and final inspection to assess repairs | Partially covered | Only the exit (final) inspection exists. There is no preliminary inspection step. |
| BR11 | Tenant must give at least a calendar month's notice | Covered | Serve Notice and Terminate Agreement reject dates less than a full calendar month away. |
| BR12 | Termination date must be the last day of the month | Covered | Both screens reject any other date. |
| BR13 | Comment is mandatory for any negative outcome | Partially covered | Agreement rejections and renewal outcomes require a comment. The exit inspection has no comment field, *Not Vacated* has none, and tenant renewal decline has none. |
| BR14 | Deposits are refundable once outstanding amounts are settled | Not covered | The system shows the deposit held but has no refund calculation, settlement check or refund step. |
| BR16 | Rent continues until the tenant is confirmed vacated | Not covered | Nothing is sent to billing when a lease is terminated or vacated, so the system neither stops nor continues rent. |
| BR17 | Leases are renewed every two years and income status is reviewed | Partially covered | The two-year renewal works. Income is not reviewed: the renewal screens do not show or update the tenant's income, apart from the payslip and bank statement uploads. |
| BR18 | Rental amount is calculated from the Housing Rental Tariff schedule | Not covered | The rental amount is typed in by hand. The tariff lookup is not used. |

BR2, BR9 and BR15 are not listed in the FRS.

# Use cases

| UC | Name | Side nav | Rating | Notes |
|---|---|---|---|---|
| UC-03 | Login | Login page | Covered | No WSO2/IAM. |
| UC-13 | Capture Lease Details | PLM Applications › Capture Master | Partially covered | No tariff calculation, no escalation or maintenance fields, no confirmation before Submit. The lease is active immediately and does not go through the agreement chain. |
| UC-14 | Generate Lease Agreement | Agreements › Generate Agreement | Covered | No confirmation before generating. |
| UC-15 | Review Lease Agreement | Agreements › Agreement Review | Covered | |
| UC-16 | Approve Lease Agreement | Agreements › Agreement Approval | Covered | Signature not enforced before Approve. The SHS is not told about a rejection. |
| UC-17 | Accept Lease Agreement | Agreements › Agreement Signature, or tenant e-mail link | Covered | |
| UC-18 | Upload Signed Lease Agreement | Agreements › Finalized Agreement | Partially covered | No Finance notification. |
| UC-19 | Capture Tenant Details | Department Tenants › Manage Tenants | Partially covered | No photograph. Duplicate ID crashes (BUG-05); lone deactivated occupant cannot be reactivated (BUG-10). |
| UC-20 | Transfer Lease | Department Tenants › Transfer Lease | Partially covered | No approval; the previous tenant's details are overwritten. |
| UC-23 | Send Communication to Tenants | Communication › New Communication | Partially covered | Postal does nothing; complex messages say "Tenants". |
| UC-24 | Recommend Lease Renewal | Agreement Renewal › Recommendations | Partially covered | Starts only when a renewal page is opened; balance is the deposit; complaints always empty. |
| UC-25 | Review Lease Renewal Recommendation | Agreement Renewal › Pending Review | Covered | |
| UC-26 | Approve Lease Renewal | Agreement Renewal › Pending Approval | Partially covered | BUG-01, BUG-02. |
| UC-27 | Accept Lease Renewal | Agreement Renewal › Pending Acceptance, or tenant e-mail link | Partially covered | The tenant link cannot submit (BUG-03). Decline creates no follow-up (BUG-08). |
| UC-28 | Renew Lease | Agreement Renewal › Pending Renewal | Covered | |
| UC-29 | Capture Notice to Terminate | Notice › Serve Notice | Covered | |
| UC-30 | Capture Lease Termination | Terminations › Terminate Agreement | Covered | 60th-month reason missing from the drop-down. |
| UC-31 | Review Termination and Validate Account | Terminations › Pending Review | Covered | No SOLAR balance, no Reject. |
| UC-32 | Conduct Exit Unit Inspection | Inspections › Post-Inspection | Covered | No photos, no comment, outcome does not change the route. |
| UC-33 | Confirm Tenant Vacated | Terminations › Pending Take-Off | Partially covered | BUG-07. |
| UC-34 | Serve Notice | Terminations › Noticed Agreements | Covered | |

# Reports and integration

| ID | Item | Rating | What exists today |
|---|---|---|---|
| IR1 | Current Lease Status report | Not covered | No HSD report screens or Excel exports exist. |
| IR2 | Tenant List Summary report | Not covered | Manage Tenants shows a list on screen, but there is no report or export. |
| IR3 | Payment Status report | Not covered | No arrears data is held (see FR13.5). |
| IR4 | Vacancy report | Not covered | No vacancy view. Vacated leases are not linked to unit availability. |
| IR6 | Complaints register | Not covered | There is no HSD complaints capture. The complaints panel on the renewal screens is always empty. |
| IR7 | Newly allocated report | Not covered | |
| IR8 | Waiting list report | Not covered | HSD has no waiting list. |
| IR01 | Login integration (WSO2/IAM) | Not covered | Replaced by system login and optional Active Directory. |

# Bugs found

These are places where the system does something wrong, not just something missing. All of them should be fixed before HSD goes live.

| # | Severity | Where | What happens |
|---|---|---|---|
| BUG-01 | High | UC-26 Approve Lease Renewal | The outcome depends on the Housing Liaison Officer's recommendation, not on the Regional Manager's decision alone. If the Regional Manager approves a *Not Renew*, the lease drops out of every user's list, the administrator's included. Rejecting a *Not Renew* offers the tenant a renewal. |
| BUG-02 | High | UC-26, second renewal | From the second renewal cycle onward, the Regional Manager's outcome is compared with the first cycle's recommendation, so the lease can be routed the wrong way. |
| BUG-03 | High | UC-27 tenant e-mail link | The tenant cannot accept or decline the renewal from the e-mailed link. The page reloads and nothing is saved. |
| BUG-04 | High | Agreement Update after a renewal | Start Date is not pre-filled. Re-entering it recalculates the dates, which undoes the renewal (for example, the end date goes back from 2030-12-31 to 2028-12-31) and sets the lease back to *Active Lease*. |
| BUG-05 | High | UC-19 add occupant | Adding an occupant whose ID number is already on the lease shows a server error page instead of a message. |
| BUG-06 | Medium | UC-24 renewal trigger | Leases are only put up for renewal when someone opens a renewal list page, and every lease category is included (BR8). If nobody opens the page, tenants are not notified. |
| BUG-07 | Medium | UC-33 *Not Vacated* | The work item is closed and the lease stays in *In Awaiting Vacating Confirmation* with no queue, so it disappears from every list. |
| BUG-08 | Medium | UC-27 tenant declines | The lease is marked *Terminating At Lease Period End*, but no work item is created, so nobody is prompted to terminate it. |
| BUG-09 | Medium | UC-32 exit inspection | All three outcomes go to vacating confirmation, so *Not Habitable* is not followed up. The tenant is sent the "You have served notice" message even when they never served notice. |
| BUG-10 | Medium | UC-19 deactivate occupant | The deactivated list only shows while another occupant is active, so a lone deactivated occupant cannot be reactivated. |
| BUG-11 | Medium | Work queues | If no user with the right role is allocated to the complex, the step is not assigned to anyone and no error is shown. Leases can stall without anyone knowing. |
| BUG-12 | Low | UC-30 termination reasons | *End of lease term (60th month)* is not offered in the drop-down. |
| BUG-13 | Low | UC-26 history | The Regional Manager's *Approve* of a *Not Renew* is recorded in the history as "Renewal Rejected". |
| BUG-14 | Low | Tenant self-service links | The renewal and signature links open without a login and rely only on a 4-digit OTP. This should get a security review. |
