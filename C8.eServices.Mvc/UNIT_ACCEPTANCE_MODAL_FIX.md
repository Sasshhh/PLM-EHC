# UNIT ACCEPTANCE MODAL FIX - TESTING GUIDE

## Issues Fixed
1. ✅ Blue alert box too stretched (now uses proper Bootstrap panels)
2. ✅ Missing deposit amount (now displays from `Model.AllocatedUnit.RequiedDepositAmount`)
3. ✅ Created rollback script to reset application for re-testing

---

## Changes Made

### 1. View: `UnitDetails.cshtml` (Lines 339-371)

**Before:**
```html
<div class="modal-content" style="width:650px;">
    <div class="alert alert-info">
        <h5><strong>Banking Details</strong></h5>
        ...
    </div>
</div>
```

**After:**
```html
<div class="modal-content"> <!-- Responsive width -->
    <!-- Deposit Amount Alert (NEW) -->
    <div class="alert alert-warning">
        <h5>Deposit Payment Required</h5>
        <p style="font-size: 18px;">
            <strong>Amount to Pay: R @String.Format("{0:N2}", Model.AllocatedUnit.RequiedDepositAmount)</strong>
        </p>
    </div>

    <!-- Banking Details Panel (FIXED LAYOUT) -->
    <div class="panel panel-info">
        <div class="panel-heading">
            <h5>Banking Details</h5>
        </div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-6">...</div> <!-- Better spacing -->
                <div class="col-md-6">...</div>
            </div>
        </div>
    </div>
</div>
```

### 2. Key Improvements
- ✅ **Responsive Modal**: Changed from fixed `width:650px` to `modal-lg` class
- ✅ **Deposit Display**: Added prominent yellow alert with formatted amount (R X,XXX.XX)
- ✅ **Better Layout**: Used Bootstrap grid (`row`/`col-md-6`) for banking details
- ✅ **Visual Hierarchy**: Green header, yellow deposit alert, blue info panel
- ✅ **Professional Look**: Panel instead of stretched alert box

---

## How to Test (Complete Flow)

### Step 1: Rollback Previous Acceptance

1. Open SQL Server Management Studio
2. Connect to your database: `CRMPLMDEV_2025`
3. Open: `C8.eServices.Mvc/Scripts/rollback_unit_acceptance.sql`
4. Change the `@AppId` variable to your application ID:
   ```sql
   DECLARE @AppId INT = 5216; -- YOUR APPLICATION ID
   ```
5. Execute the script
6. Verify output shows:
   ```
   Application Status: Reset to Awaited (s_rcs_awaited)
   Unit: Reset to Available
   Matched Units: Reset IsAccepted to FALSE
   ```

### Step 2: Test Unit Acceptance Flow

1. **Stop Visual Studio debugger** (if running)
2. **Rebuild Solution** (Ctrl+Shift+B)
3. **Start Debugging** (F5)

4. **Login as Applicant**:
   - Username: (your test applicant)
   - Password: (your test password)

5. **Navigate to Unit Offer**:
   - Go to "My Applications" or Inbox
   - Find application ID 5216 (or your test app)
   - Click "View Unit Details"

6. **Accept Unit Offer**:
   - Click the "Accept Unit Offer" button
   - Modal should pop up

### Step 3: Verify Modal Display

**Check the following:**

✅ **Modal Layout**:
- [ ] Modal is properly sized (not too narrow, not stretched)
- [ ] Green header with checkmark icon
- [ ] Content is well-spaced and readable

✅ **Deposit Amount**:
- [ ] Yellow/orange alert box displays at the top
- [ ] Shows "Deposit Payment Required"
- [ ] Displays formatted amount: **"Amount to Pay: R X,XXX.XX"**
- [ ] Amount matches the unit's deposit (from `ApplicationAllocatedProperty.RequiedDepositAmount`)

✅ **Banking Details**:
- [ ] Blue panel (not stretched alert)
- [ ] Proper grid layout (2 columns)
- [ ] All fields visible:
  - Bank Name: Standard Bank
  - Account Name: Ekurhuleni Metropolitan Municipality
  - Account Number: 001844075
  - Branch Code: 011545
  - Reference: Your Application Reference Number

✅ **Footer**:
- [ ] Green button with "OK, I Understand" text
- [ ] Button has checkmark icon

---

## Expected Result

### Before (Issues):
```
┌─────────────────────────────────────────────┐
│ Unit Offer Accepted Successfully!          │
├─────────────────────────────────────────────┤
│ Congratulations! You have...               │
│                                             │
│ ┌─────────────────────────────────────────┐│ ← STRETCHED
│ │ Banking Details (ALERT BOX)              ││
│ │ Bank Name: Standard Bank                 ││
│ │ Account Name: Ekurhuleni...              ││
│ │ (NO DEPOSIT AMOUNT SHOWN)                ││ ← MISSING
│ └─────────────────────────────────────────┘│
│                                             │
│                    [OK]                     │
└─────────────────────────────────────────────┘
```

### After (Fixed):
```
┌───────────────────────────────────────────────┐
│ ✓ Unit Offer Accepted Successfully!          │ ← Green Header
├───────────────────────────────────────────────┤
│ Congratulations! You have...                 │
│                                               │
│ ┌───────────────────────────────────────────┐│
│ │ ⚠ Deposit Payment Required                ││ ← NEW: Yellow Alert
│ │ Amount to Pay: R 3,500.00                 ││ ← DEPOSIT AMOUNT
│ └───────────────────────────────────────────┘│
│                                               │
│ Next Steps:                                   │
│                                               │
│ ╔═══════════════════════════════════════════╗│
│ ║ 🏦 Banking Details                         ║│ ← Panel (not alert)
│ ╠═══════════════════════════════════════════╣│
│ ║ Bank Name:        Account Name:           ║│ ← 2 Column Layout
│ ║ Standard Bank     Ekurhuleni Metro...     ║│
│ ║                                            ║│
│ ║ Account Number:   Branch Code:            ║│
│ ║ 001844075         011545                  ║│
│ ╚═══════════════════════════════════════════╝│
│                                               │
│ ℹ Important: After making payment...         │
│                                               │
│           ✓ OK, I Understand                 │ ← Green Button
└───────────────────────────────────────────────┘
```

---

## Troubleshooting

### Issue: Modal still looks stretched

**Solution:**
1. Clear browser cache (Ctrl+Shift+Delete)
2. Hard refresh (Ctrl+F5)
3. Check CSS is not being overridden by custom styles

### Issue: Deposit amount shows "R 0.00"

**Possible Causes:**
1. Unit record has `RequiedDepositAmount = 0` in database
2. Wrong unit selected

**Solution:**
```sql
-- Check unit deposit amount
SELECT 
    AAP.Id,
    AAP.SpaceUnitNumber,
    AAP.RequiedDepositAmount,
    MU.PropertyLeaseApplicationId
FROM ApplicationAllocatedProperty AAP
INNER JOIN MatchedUnits MU ON AAP.Id = MU.ApplicationAllocatedPropertyId
WHERE MU.PropertyLeaseApplicationId = 5216; -- Your App ID

-- Update if needed
UPDATE ApplicationAllocatedProperty
SET RequiedDepositAmount = 3500.00  -- Set correct amount
WHERE Id = (SELECT TOP 1 ApplicationAllocatedPropertyId 
            FROM MatchedUnits 
            WHERE PropertyLeaseApplicationId = 5216);
```

### Issue: Deposit amount doesn't display at all

**Check:**
1. `Model.AllocatedUnit` is not null
2. View has access to the model property

**Solution:**
Add null check in View if needed:
```csharp
@if (Model.AllocatedUnit != null && Model.AllocatedUnit.RequiedDepositAmount > 0)
{
    <div class="alert alert-warning">
        ...
    </div>
}
```

---

## Database State After Acceptance

**Verify these changes occur:**

```sql
-- Application Status should be "Awaiting Deposit Paid"
SELECT S.[Name]
FROM PropertyLeaseApplications PLA
INNER JOIN [Status] S ON PLA.StatusId = S.Id
WHERE PLA.Id = 5216;

-- MatchedUnit should have IsAccepted = TRUE
SELECT IsAccepted
FROM MatchedUnits
WHERE PropertyLeaseApplicationId = 5216
AND IsDeleted = 0
ORDER BY Id DESC;

-- Unit should be marked as taken
SELECT IsTaken, PropertyLeaseApplicationId
FROM ApplicationAllocatedProperty
WHERE Id = (SELECT TOP 1 ApplicationAllocatedPropertyId 
            FROM MatchedUnits 
            WHERE PropertyLeaseApplicationId = 5216);
```

---

## Next Steps After Testing

If modal looks correct:
1. ✅ Test with different deposit amounts
2. ✅ Test on different screen sizes (mobile/tablet)
3. ✅ Verify email notification also shows deposit amount
4. ✅ Document for production deployment

---

## Rollback Script Location

**File:** `C8.eServices.Mvc/Scripts/rollback_unit_acceptance.sql`

**Quick Rollback Command:**
```sql
-- Reset application 5216 for re-testing
DECLARE @AppId INT = 5216;
-- (See full script for complete rollback logic)
```

---

## Summary

✅ **Fixed:** Stretched blue alert → Professional panel layout
✅ **Added:** Deposit amount display with formatting
✅ **Improved:** Visual hierarchy and spacing
✅ **Created:** Rollback script for easy re-testing

**Test Result:** □ Pass □ Fail

**Notes:**
_____________________________________________________
_____________________________________________________
_____________________________________________________
