# UC015: Deposit Payment Workflow - Knowledge Base

This document details the status transitions, responsibilities, and logic for the Deposit Payment validation process in the PLM system.

## Workflow Overview

The Deposit Payment workflow occurs after a unit has been matched to a tenant. The tenant must upload a Proof of Payment (POP) for the deposit, which is then validated by a Revenue Officer (Bookkeeper).

---

## Status Transition Table

| Stage | From Status | Trigger Action | To Status | Responsibility |
| :--- | :--- | :--- | :--- | :--- |
| **1. Awaiting Payment** | `s_debit_order_sent` (Awaiting Deposit Paid) | Unit Matched to Tenant | `s_debit_order_sent` | Tenant |
| **2. POP Uploaded** | `s_debit_order_sent` | Tenant uploads Deposit POP | `s_rcs_pending_assessment_payment_validation` | System (EHCRoundRobin) |
| **3. Validation (Approved - Natural Person)** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Approves | `s_awaiting_risk_assessment` | Revenue Officer |
| **4. Validation (Approved - Company)** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Approves | `s_property_accounts` | Revenue Officer |
| **5. Validation (Rejected)** | `s_rcs_pending_assessment_payment_validation` | Bookkeeper Rejects | `s_debit_order_sent` | Revenue Officer |

---

## Technical Details

### 1. Tenant POP Upload
- **Controller**: `FileController.cs`
- **Logic**: If the application status is `s_debit_order_sent`, uploading a document triggers:
  - Status change to `s_rcs_pending_assessment_payment_validation` (Pending Assessment Fee Payment Validation).
  - Creation of a Round Robin Queue entry for `ValidateDepositPayment`.
- **Note**: The system reuses the status key `s_rcs_pending_assessment_payment_validation` for both Application Fee and Deposit validation.

### 2. Bookkeeper Dashboard
- **URL**: `/PropertyLeaseApplication/PropertyLeaseDeposit`
- **Action**: `PropertyLeaseDeposit()` in `PropertyLeaseApplicationController.cs`.
- **Filter**: Displays applications where `Status` is `s_rcs_pending_assessment_payment_validation` AND there is a pending `ValidateDepositPayment` task in the Round Robin Queue for the current user.

### 3. Validation Logic
- **Controller**: `PropertyLeaseApplicationController.cs`
- **Action**: `AssessmentFeeValidation` (POST)
- **Status Keys**:
  - **Rejection**: Reverts to `s_debit_order_sent` (Awaiting Deposit Paid).
  - **Approval (Natural Person)**: Moves to `s_awaiting_risk_assessment`.
  - **Approval (Company)**: Moves to `s_property_accounts`.
- **Notifications**:
  - `DepositPaymentApprove` email sent on approval.
  - `DepositPaymentReject` email sent on rejection.

---

## Frequently Asked Questions

**Q: Why does the dashboard show "Deposit Payments" but the status is "Pending Assessment Fee Validation"?**
A: The system reuses the same status key for both stages. The differentiation happens via the `ResponsibilityType` in the Round Robin Queue.

**Q: Where can I see the uploaded deposit document?**
A: In the "Attached Documents" section of the `AssessmentFeeValidation` view.

**Q: What happens if I reject the deposit?**
A: The application goes back to the tenant's portal, and they are notified to re-upload the proof of payment.
