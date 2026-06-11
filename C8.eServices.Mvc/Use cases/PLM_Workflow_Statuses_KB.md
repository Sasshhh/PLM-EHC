# PLM Workflow Master Status Guide

This document provides a centralized reference for all workflow status transitions, responsibility queues, and decision logic across the PLM system.

---

## 1. Maintenance & Facilities Workflow (UC012)

This workflow handles unit inspections and maintenance job sign-offs.

| Stage | From Status | Action | To Status | Responsibility |
| :--- | :--- | :--- | :--- | :--- |
| **Inspection** | `s_additional_property_owners_pending` | Inspector submits | `s_rcs_InProgress` | Maintenance Manager |
| **Maintenance Sign-off** | `s_rcs_InProgress` | Maint. Manager Signs | `s_rcs_awaited` | Facilities Manager |
| **Facilities Sign-off** | `s_rcs_awaited` | Facilities Manager Signs | `s_awaiting_tenant_update_record` | Letting Officer |
| **Major Defects (Sign-off)** | `s_customer_query_pending` | Facilities Manager Signs | `s_awaiting_tenant_update_record` | Letting Officer |

### Key Responsibility Mappings:
- **Maintenance Manager**: `PropertyFacilitiesMaintenanceReviews` (RoundRobin: `MaintananceJobSheet`)
- **Facilities Manager**: `PropertyFacilitiesManagerReview` (RoundRobin: `PropertyFacilitiesManagerReview`)
- **Letting Officer**: `UpdateTenantDetails` (RoundRobin: `UpdateTenantDetails`)

---

## 2. Deposit Payment Workflow (UC015)

This workflow handles the validation of tenant deposit payments after unit matching.

| Stage | From Status | Action | To Status | Responsibility |
| :--- | :--- | :--- | :--- | :--- |
| **Awaiting Payment** | `s_debit_order_sent` | Unit Matched | `s_debit_order_sent` | Tenant |
| **POP Uploaded** | `s_debit_order_sent` | Tenant uploads POP | `s_rcs_pending_assessment_payment_validation` | Revenue Officer |
| **Approved (Natural)** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Approves | `s_awaiting_risk_assessment` | Letting Officer |
| **Approved (Company)** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Approves | `s_property_accounts` | Revenue Manager |
| **Rejected** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Rejects | `s_debit_order_sent` | Tenant |

---

## 3. Key Technical Logic

### Status Reuse
- **`s_rcs_pending_assessment_payment_validation`**: This status is used for both **Application Fee Validation** and **Deposit Payment Validation**. 
- **Differentiation**: The system distinguishes between these stages using the **Round Robin Responsibility Type** (`ValidateApplicationFee` vs `ValidateDepositPayment`).

### Role-Based Actions
- **Revenue Officer (Bookkeeper)**: Responsible for validating all financial documents (Fees, Deposits).
- **Maintenance Manager**: First sign-off for unit inspections.
- **Facilities Manager**: Final sign-off for unit inspections.

### Redirects & Dashboards
- **Deposit Dashboard**: `/PropertyLeaseApplication/PropertyLeaseDeposit`
- **Maintenance Dashboard**: `/PropertyLeaseApplication/PropertyFacilitiesMaintenanceReviews`

---

## 4. Common Troubleshooting

**Q: Why can't the Bookkeeper see the application in their "Deposit Payments" dashboard?**
A: Ensure the application status is exactly `s_rcs_pending_assessment_payment_validation` and that a Round Robin Queue entry exists for the user with the `ValidateDepositPayment` responsibility.

**Q: What happens if an image is missing in the Pre-Tenancy Training?**
A: Check that the `ImagePath` in the database starts with `~/` or `/`. The system now dynamically resolves these paths relative to the application root.

---

## 5. UC018 - Lease Renewal Workflow (EHC)

### Trigger
`MatchingHelper.RenewalNotificationAtEndOfTime()` runs on a schedule. Any lease where `RenewalNotice <= today` is flagged and a CSO is assigned via Round Robin (`LeaseRenewals` responsibility).

### EHC Lease Lifecycle: Renewals
- **No new `LeaseDetails` row is created on renewal.** The same row is updated in place.
- `IsNew = true` on the active lease throughout the renewal process.
- `LeaseDetails.EndDate`, `RenewalNotice`, and `TerminationNotice` are updated **ONLY** when the tenant accepts the offer (final step).

### Staging Record: `PropertyLeaseRenewalOffer`
Holds all proposed terms and approval trail without touching `LeaseDetails`:

| Field | Purpose |
|---|---|
| `MonthsOffer` | Proposed renewal duration (12 or 24 months) |
| `ProposedEndDate` | Calculated new EndDate (not yet applied) |
| `ProposedRenewalNotice` | Calculated new RenewalNotice |
| `ProposedTerminationNotice` | Calculated new TerminationNotice |
| `CSO_Outcome / Comment / Date` | CSO decision trail |
| `RM_Outcome / Comment / Date` | Revenue Manager trail |
| `CEO_Outcome / Comment / Date` | Director/CEO trail |
| `IsAccepted` | Set true on tenant acceptance |
| `CustomerDeclineReason` | Set if tenant declines |

### Status Flow

| Step | Actor | Action | LeaseDetails Status After | LeaseDetails Dates Changed? |
|---|---|---|---|---|
| **1. Scanner** | System | `RenewalNotificationAtEndOfTime()` | `Application Up For Renewal At Three Months` | No |
| **2. CSO Reviews** | CSO | `RecommendForRenewal` POST | `Awaiting Renewal Review Outcome` | **No** - writes to `PropertyLeaseRenewalOffer` only |
| **3. CSO Not Renew** | CSO | Selects "Not Renew" | `Lease Renewal Rejected` | No |
| **4. RM Reviews** | Revenue Manager | `RevenueManagerRenewalReview` POST - Approve | `Awaiting Renewal Outcome` | No |
| **5. RM Rejects** | Revenue Manager | `RevenueManagerRenewalReview` POST - Reject | `Lease Renewal Rejected` | No |
| **6. CEO Decides** | Director | `CEORenewalDecision` POST - Approve | `Awaiting Lease Renewal Agreement Conclusion` | No |
| **7. CEO Rejects** | Director | `CEORenewalDecision` POST - Reject | `Lease Renewal Rejected` | No |
| **8. CSO Concludes** | CSO | `ConcludeRenewalAgreement` POST | `Lease Renewal Agreement Concluded` | **No** - sends offer to tenant only |
| **9. Tenant Accepts** | Tenant | `LeaseOfferValidation` POST - Approved | `Awaiting Renewal Documents` | **YES - only here** |
| **10. Tenant Declines** | Tenant | `LeaseOfferValidation` POST - Rejected | `Terminate At End Of Period` | No |

### Key Responsibility Queue Mappings (Round Robin)

| Responsibility Key | Used At | Role |
|---|---|---|
| `LeaseRenewals` | Scanner assigns to CSO | CSO |
| `LeaseRenewalRevenue` | After CSO recommends | Revenue Manager |
| `LeaseRenewalCEOApproval` | After RM approves | Director/CEO (Property Manager) |
| `LeaseRenewals` | After CEO approves (re-assigned to CSO) | CSO (conclude agreement) |

### Key Action Type Keys (`RCSActionTypeKeys`)

| Key | Meaning |
|---|---|
| `Approve12Months` | CSO recommends 12-month renewal |
| `Approve24Months` | CSO recommends 24-month renewal |
| `NotRenew` | CSO recommends not renewing |
| `Approved` | RM/CEO approves |
| `Rejected` | RM/CEO rejects |

### Key Status Keys (`StatusKeys`)

| Key | Status Name |
|---|---|
| `LeaseRenewalRejected` | Lease Renewal Rejected |
| `AwaitingRenewalReviewOutcome` | Awaiting Renewal Review Outcome |
| `AwaitingRenewalOutcome` | Awaiting Renewal Outcome |
| `LeaseRenewalApproved` | Lease Renewal Approved |
| `AwaitingLeaseRenewalAgreementConclusion` | Awaiting Lease Renewal Agreement Conclusion |
| `LeaseRenewalAgreementConcluded` | Lease Renewal Agreement Concluded |
| `AwaitingRenewalDocuments` | Awaiting Renewal Documents |
| `TerminateAtEndOfPeriod` | Terminate At End Of Period |

### Architecture Note: `LeaseRenewalService.cs`
All renewal processing logic is centralised in `Helpers/LeaseRenewalService.cs`:
- `CreateOrUpdateOffer()` - CSO step, writes to `PropertyLeaseRenewalOffer`
- `RecordRMDecision()` - Revenue Manager trail
- `RecordCEODecision()` - Director/CEO trail
- `RecordCustomerDecline()` - Customer decline trail

`MatchingHelper.UpdatePropertyLeaseDates()` remains the **single, authorised** method for modifying `LeaseDetails` dates and is called only from `LeaseOfferValidation` POST.


### Future Enhancements
- **Tenant Renewal Rejection**: Currently routes to s_lease_renewal_rejected and drops back into the CSO's renewals inbox. Will be enhanced to automatically trigger the termination workflow.

