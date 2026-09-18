# 🎯 QUICK REFERENCE - UNIT ACCEPTANCE MODAL FIX

## ⚡ Quick Test (3 Steps)

### 1️⃣ Rollback Database
```powershell
cd C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts
.\Rollback-UnitAcceptance.ps1 -ApplicationId 5216
```

### 2️⃣ Rebuild & Run
- Press `Ctrl + Shift + B` (Rebuild)
- Press `F5` (Start Debugging)

### 3️⃣ Test Flow
1. Login as applicant
2. Go to "My Applications"
3. View Unit Details
4. Click "Accept Unit Offer"
5. **CHECK MODAL:**
   - ✅ Deposit amount visible (e.g., R 3,500.00)
   - ✅ Banking details in clean panel
   - ✅ Not stretched or weird layout

---

## 🔍 What to Look For

### ✅ GOOD (After Fix)
```
┌─────────────────────────────┐
│ ✓ Unit Accepted!            │
├─────────────────────────────┤
│ ⚠ Deposit Required          │
│   R 3,500.00                │ ← VISIBLE AMOUNT
├─────────────────────────────┤
│ ╔═══════════════════════╗  │
│ ║ 🏦 Banking Details     ║  │ ← CLEAN PANEL
│ ╠═══════════════════════╣  │
│ ║ Bank: Standard Bank    ║  │
│ ║ Acct: 001844075        ║  │
│ ╚═══════════════════════╝  │
└─────────────────────────────┘
```

### ❌ BAD (Before Fix)
```
┌─────────────────────────────┐
│ Unit Accepted!              │
├─────────────────────────────┤
│ [═══════════════════════]   │ ← STRETCHED ALERT
│ │ Banking Details         │ │
│ │ (NO DEPOSIT SHOWN)      │ │ ← MISSING AMOUNT
│ [═══════════════════════]   │
└─────────────────────────────┘
```

---

## 🛠️ Manual Rollback (If Script Fails)

```sql
-- Copy-paste into SSMS, change 5216 to your App ID
DECLARE @AppId INT = 5216;

UPDATE MatchedUnits 
SET IsAccepted = 0 
WHERE PropertyLeaseApplicationId = @AppId;

UPDATE ApplicationAllocatedProperty 
SET IsTaken = 0, PropertyLeaseApplicationId = NULL 
WHERE Id = (SELECT TOP 1 ApplicationAllocatedPropertyId 
            FROM MatchedUnits 
            WHERE PropertyLeaseApplicationId = @AppId);

UPDATE PropertyLeaseApplications 
SET StatusId = (SELECT Id FROM Status WHERE [Key] = 's_rcs_awaited') 
WHERE Id = @AppId;
```

---

## 📁 Files Changed

| File | Change |
|------|--------|
| `Views/MatchedUnits/UnitDetails.cshtml` | Fixed modal (lines 339-371) |
| `Scripts/rollback_unit_acceptance.sql` | Rollback script |
| `Scripts/Rollback-UnitAcceptance.ps1` | PowerShell helper |

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| Deposit shows R 0.00 | Update DB: `UPDATE ApplicationAllocatedProperty SET RequiedDepositAmount = 3500 WHERE Id = X` |
| Modal still stretched | Hard refresh: `Ctrl + Shift + R` |
| Can't click Accept again | Run rollback script again |
| Script says "App already accepted" | That's expected - run rollback first |

---

## ✅ Success Checklist

- [ ] Deposit amount displays (R X,XXX.XX)
- [ ] Banking details in panel (not alert)
- [ ] Layout looks professional
- [ ] Modal is responsive
- [ ] Can rollback and test again

---

## 📞 Need Help?

1. Check full guide: `UNIT_ACCEPTANCE_MODAL_FIX.md`
2. Check summary: `UNIT_ACCEPTANCE_MODAL_FIX_SUMMARY.md`
3. Check rollback SQL: `Scripts/rollback_unit_acceptance.sql`

---

**Quick Command Reference:**
```powershell
# Rollback
.\Rollback-UnitAcceptance.ps1 -ApplicationId 5216

# Rebuild
Ctrl + Shift + B

# Run
F5

# Hard Refresh Browser
Ctrl + F5
```

**Status:** ✅ Ready to Test
