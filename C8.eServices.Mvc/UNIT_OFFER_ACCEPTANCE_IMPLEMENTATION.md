# Unit Offer Acceptance - Email & Modal Implementation

## Date: 2026-03-14
## Author: GitHub Copilot
## Status: ✅ Complete

---

## Overview

This implementation adds two key features to the unit offer acceptance process:

1. **Automated Email with Banking Details** - Sends deposit payment banking details after unit acceptance
2. **Success/Rejection Modals** - Displays user-friendly modal popups after accepting or rejecting a unit offer

---

## 1. Email Notification with Banking Details

### Changes Made

#### A. Added New Email Content Key
**File:** `C8.eServices.Mvc\Keys\EmailContentKeys.cs`

```csharp
public const string UnitAcceptedDepositPaymentDetails = "plm_unit_accepted_deposit_payment_details";
```

#### B. Updated Controller Logic
**File:** `C8.eServices.Mvc\Controllers\MatchedUnitsController.cs`

- Modified `AcceptMatchedUnit` method to send **TWO emails**:
  1. **Existing Email**: Unit acceptance confirmation (`EmailContentKeys.Accetproperty`)
  2. **NEW Email**: Deposit payment banking details (`EmailContentKeys.UnitAcceptedDepositPaymentDetails`)

```csharp
// Send unit accepted email
int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.Accetproperty).Id;
EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

// Send deposit payment email with banking details
int depositEmailId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitAcceptedDepositPaymentDetails)?.Id ?? 0;
if (depositEmailId > 0)
{
    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, depositEmailId);
}
```

#### C. Database Email Template
**File:** `C8.eServices.Mvc\Scripts\add_deposit_payment_email_template.sql`

**Banking Details (Hardcoded as requested):**
- **Bank Name:** Standard Bank
- **Account Name:** Ekurhuleni Metropolitan Municipality
- **Account Number:** 001844075
- **Branch Code:** 011545
- **Reference:** Application Reference Number

**To Deploy:**
```sql
-- Run the SQL script in SQL Server Management Studio
-- Path: C8.eServices.Mvc\Scripts\add_deposit_payment_email_template.sql
```

---

## 2. Success/Rejection Modals

### Changes Made

#### A. Added Three New Modals
**File:** `C8.eServices.Mvc\Views\MatchedUnits\UnitDetails.cshtml`

1. **Unit Acceptance Success Modal** (`#unitAcceptanceModal`)
   - Shows congratulations message
   - Displays banking details
   - Instructions for deposit payment
   - Styled with green header and success icon

2. **Unit Rejection Modal** (`#unitRejectionModal`)
   - Confirms rejection
   - Informs user about next steps
   - Styled with orange header and info icon

3. **Error Modal** (`#unitActionErrorModal`)
   - Displays error message
   - Styled with red header and error icon

#### B. Updated JavaScript Functions

**Accept Unit:**
```javascript
function AcceptPropertyListing() {
    $.getJSON("../MatchedUnits/AcceptMatchedUnit?id=" + @Model.MatchedUnitId,
        function (result) {
            if (result == "Success") {
                $("#unitAcceptanceModal").modal('show');
            } else {
                $("#unitActionErrorModal").modal('show');
            }
        });
}
```

**Reject Unit:**
```javascript
function RejectPropertyListing() {
    $.getJSON("../MatchedUnits/RejectMatchedUnit?id=" + @Model.MatchedUnitId,
        function (result) {
            if (result == "Success") {
                $("#unitRejectionModal").modal('show');
            } else {
                $("#unitActionErrorModal").modal('show');
            }
        });
}
```

**Redirect Function:**
```javascript
function redirectToInbox() {
    window.location.href = "@Url.Action("Inbox", "PropertyLeaseApplication")";
}
```

---

## User Flow

### Accept Unit Flow:
1. User clicks "Accept" button
2. System validates waiting list sorting
3. System accepts the unit via API
4. **Success Modal appears** with banking details
5. User clicks "OK"
6. System redirects to Inbox
7. **Email sent** with banking details

### Reject Unit Flow:
1. User clicks "Reject" button
2. System validates waiting list sorting
3. System rejects the unit via API
4. **Rejection Modal appears** with confirmation
5. User clicks "OK"
6. System redirects to Inbox
7. Email sent confirming rejection

---

## Testing Checklist

- [ ] **Email Template Created** in database
- [ ] **Accept Unit** - Verify success modal displays
- [ ] **Accept Unit** - Verify banking details are correct
- [ ] **Accept Unit** - Verify TWO emails are sent
- [ ] **Accept Unit** - Verify redirect to Inbox after clicking OK
- [ ] **Reject Unit** - Verify rejection modal displays
- [ ] **Reject Unit** - Verify redirect to Inbox after clicking OK
- [ ] **Error Handling** - Test when API call fails
- [ ] **Email Content** - Verify placeholders are replaced (CustomerName, ApplicationReference, etc.)

---

## Deployment Steps

1. **Deploy Code Changes**
   - `EmailContentKeys.cs`
   - `MatchedUnitsController.cs`
   - `UnitDetails.cshtml`

2. **Run SQL Script**
   ```sql
   -- Execute in SQL Server Management Studio
   -- File: C8.eServices.Mvc\Scripts\add_deposit_payment_email_template.sql
   ```

3. **Verify Email Template**
   ```sql
   SELECT * FROM EmailContentTypes 
   WHERE [Key] = 'plm_unit_accepted_deposit_payment_details'
   ```

4. **Test End-to-End**
   - Accept a unit offer
   - Check modal appears with banking details
   - Check TWO emails are received
   - Verify redirect to Inbox

---

## Banking Details (For Reference)

| Field | Value |
|-------|-------|
| Bank Name | Standard Bank |
| Account Name | Ekurhuleni Metropolitan Municipality |
| Account Number | 001844075 |
| Branch Code | 011545 |
| Reference | Application Reference Number |

---

## Future Enhancements (Optional)

1. **Dynamic Banking Details** - Pull from database instead of hardcoding
2. **SMS Notification** - Send banking details via SMS as well
3. **Payment Integration** - Add direct payment gateway link
4. **Deposit Amount** - Display exact deposit amount in modal
5. **Application Reference** - Include in modal for easier payment reference

---

## Files Modified

| File | Changes |
|------|---------|
| `C8.eServices.Mvc\Keys\EmailContentKeys.cs` | Added `UnitAcceptedDepositPaymentDetails` key |
| `C8.eServices.Mvc\Controllers\MatchedUnitsController.cs` | Updated `AcceptMatchedUnit` to send deposit email |
| `C8.eServices.Mvc\Views\MatchedUnits\UnitDetails.cshtml` | Added 3 modals, updated JavaScript |

## Files Created

| File | Purpose |
|------|---------|
| `C8.eServices.Mvc\Scripts\add_deposit_payment_email_template.sql` | Database script for email template |
| `C8.eServices.Mvc\UNIT_OFFER_ACCEPTANCE_IMPLEMENTATION.md` | This documentation |

---

## Contact

For any questions or issues, please refer to this documentation or contact the development team.

**Implementation Complete** ✅
