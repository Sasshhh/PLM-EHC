# UNIT ACCEPTANCE MODAL - FIX SUMMARY

## Problem
When applicant accepts a unit offer, the success modal had two issues:
1. **Stretched Layout**: Blue banking details alert was too wide and looked unprofessional
2. **Missing Deposit**: No deposit amount shown, but users need to know how much to pay

## Solution

### Files Modified
1. ✅ `Views/MatchedUnits/UnitDetails.cshtml` - Fixed modal layout
2. ✅ `Scripts/rollback_unit_acceptance.sql` - Reset database for testing
3. ✅ `Scripts/Rollback-UnitAcceptance.ps1` - Quick rollback tool
4. ✅ `UNIT_ACCEPTANCE_MODAL_FIX.md` - Complete testing guide

---

## Quick Start (Testing)

### Option 1: PowerShell Script (EASIEST)
```powershell
cd C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts
.\Rollback-UnitAcceptance.ps1 -ApplicationId 5216
```

### Option 2: SQL Script
1. Open `Scripts/rollback_unit_acceptance.sql`
2. Change `@AppId` to your application ID
3. Execute in SSMS

### Option 3: Manual SQL
```sql
-- Quick reset for app 5216
UPDATE MatchedUnits SET IsAccepted = 0 WHERE PropertyLeaseApplicationId = 5216;
UPDATE ApplicationAllocatedProperty SET IsTaken = 0, PropertyLeaseApplicationId = NULL 
WHERE Id = (SELECT TOP 1 ApplicationAllocatedPropertyId FROM MatchedUnits WHERE PropertyLeaseApplicationId = 5216);
UPDATE PropertyLeaseApplications SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_rcs_awaited') WHERE Id = 5216;
```

---

## What Changed in the Modal

### Before
```
┌────────────────────────────┐
│ Unit Accepted!             │
├────────────────────────────┤
│ Congratulations...         │
│                            │
│ [────────────────────────] │ ← STRETCHED BLUE ALERT
│ │ Banking Details         │ │
│ │ Bank: Standard Bank     │ │
│ │ (NO DEPOSIT SHOWN)      │ │ ← MISSING AMOUNT
│ [────────────────────────] │
│                            │
│        [OK]                │
└────────────────────────────┘
```

### After
```
┌──────────────────────────────┐
│ ✓ Unit Accepted!             │ ← Green header
├──────────────────────────────┤
│ Congratulations...           │
│                              │
│ ┌──────────────────────────┐│
│ │ ⚠ Deposit Required        ││ ← NEW: Yellow alert
│ │ R 3,500.00               ││ ← DEPOSIT AMOUNT
│ └──────────────────────────┘│
│                              │
│ ╔════════════════════════╗ │
│ ║ 🏦 Banking Details      ║ │ ← Panel (not alert)
│ ╠════════════════════════╣ │
│ ║ Bank:    Account:      ║ │ ← 2 Columns
│ ║ Standard Ekurhuleni... ║ │
│ ║                        ║ │
│ ║ Acct#:   Branch:       ║ │
│ ║ 001844075  011545      ║ │
│ ╚════════════════════════╝ │
│                              │
│   ✓ OK, I Understand        │ ← Green button
└──────────────────────────────┘
```

---

## Code Changes

### View: UnitDetails.cshtml (Line 339-371)

**Key Changes:**
```csharp
// 1. Added Deposit Amount Display
<div class="alert alert-warning">
    <h5>Deposit Payment Required</h5>
    <p style="font-size: 18px;">
        <strong>Amount to Pay: R @String.Format("{0:N2}", Model.AllocatedUnit.RequiedDepositAmount)</strong>
    </p>
</div>

// 2. Changed from Alert to Panel
<div class="panel panel-info"> // Was: <div class="alert alert-info">
    <div class="panel-heading">
        <h5>Banking Details</h5>
    </div>
    <div class="panel-body">
        // 3. Added Bootstrap Grid for Better Layout
        <div class="row">
            <div class="col-md-6">Bank Name: ...</div>
            <div class="col-md-6">Account Name: ...</div>
        </div>
    </div>
</div>

// 4. Made Modal Responsive
<div class="modal-dialog modal-lg"> // Was: fixed width
```

---

## Testing Checklist

Before testing, run rollback script to reset:
```powershell
.\Rollback-UnitAcceptance.ps1 -ApplicationId 5216
```

Then verify:
- [ ] Modal opens when clicking "Accept Unit Offer"
- [ ] Deposit amount shows: **R X,XXX.XX** (formatted with commas)
- [ ] Banking details in clean panel (not stretched alert)
- [ ] Layout is responsive (test different screen sizes)
- [ ] Green header and button look professional
- [ ] Amount matches unit's deposit in database

---

## Database Verification

Check deposit amount is correct:
```sql
SELECT 
    PLA.Id AS AppId,
    PLA.ApplicationReferenceNumber,
    AAP.SpaceUnitNumber,
    AAP.RequiedDepositAmount,
    AAP.MonthlyRentalAmount
FROM PropertyLeaseApplications PLA
INNER JOIN MatchedUnits MU ON PLA.Id = MU.PropertyLeaseApplicationId
INNER JOIN ApplicationAllocatedProperty AAP ON MU.ApplicationAllocatedPropertyId = AAP.Id
WHERE PLA.Id = 5216;
```

Expected Output:
```
AppId | RefNo          | Unit    | Deposit  | Monthly
------|----------------|---------|----------|--------
5216  | EHC2026...     | A-101   | 3500.00  | 2500.00
```

---

## Troubleshooting

### Deposit shows R 0.00
**Cause:** Unit has no deposit set in database

**Fix:**
```sql
UPDATE ApplicationAllocatedProperty
SET RequiedDepositAmount = 3500.00
WHERE Id = (SELECT ApplicationAllocatedPropertyId 
            FROM MatchedUnits 
            WHERE PropertyLeaseApplicationId = 5216);
```

### Modal still stretched
**Cause:** Browser cache

**Fix:**
1. Hard refresh: `Ctrl + Shift + R` (Chrome) or `Ctrl + F5` (Edge)
2. Clear cache: `Ctrl + Shift + Delete`

### Can't re-test (already accepted)
**Cause:** Database still shows accepted

**Fix:**
```powershell
.\Rollback-UnitAcceptance.ps1 -ApplicationId 5216
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `Views/MatchedUnits/UnitDetails.cshtml` | Modal UI (Lines 339-371) |
| `Controllers/MatchedUnitsController.cs` | Accept logic (Line 92+) |
| `Models/ApplicationAllocatedProperty.cs` | Unit model with deposit |
| `Scripts/rollback_unit_acceptance.sql` | Reset for testing |
| `Scripts/Rollback-UnitAcceptance.ps1` | Quick rollback tool |
| `UNIT_ACCEPTANCE_MODAL_FIX.md` | Full testing guide |

---

## Production Deployment

When deploying to production:

1. ✅ No database changes needed (uses existing fields)
2. ✅ No controller changes needed (only View changed)
3. ✅ Just deploy updated `UnitDetails.cshtml`
4. ✅ Test with staging data first

**Deployment Steps:**
```powershell
# 1. Build Release
msbuild /p:Configuration=Release

# 2. Deploy View file
Copy-Item "Views\MatchedUnits\UnitDetails.cshtml" -Destination "\\PROD\wwwroot\Views\MatchedUnits\"

# 3. Recycle IIS App Pool
Restart-WebAppPool -Name "PLM-EHC"
```

---

## Success Criteria

✅ Modal displays deposit amount prominently
✅ Banking details in professional panel layout
✅ Layout is responsive and not stretched
✅ Easy to read on mobile and desktop
✅ Applicant knows exactly how much to pay

---

## Next Steps

After confirming modal looks correct:
1. Test with different deposit amounts (R0, R500, R10,000)
2. Test on mobile devices (responsive layout)
3. Update email notification to include deposit amount
4. Document for user training

---

**Status:** ✅ READY FOR TESTING

**Last Updated:** 2025-03-28
