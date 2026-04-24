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
