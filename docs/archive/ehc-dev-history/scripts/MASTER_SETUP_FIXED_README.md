# ✅ MASTER_SETUP_UC012_UC013.sql - NOW FIXED!

## What Was Wrong:
The script had incorrect column names that didn't match your database schema:
- ❌ `CapturedDateTime` → ✅ `CreatedDateTime`  
- ❌ `SystemIdentityUserId` → ✅ Join on `CAST(c.Id AS NVARCHAR(128)) = u.Id`
- ❌ `Email` → ✅ `EmailAddress`

## ✅ All Fixed - Ready to Run!

### How to Run (Choose One):

#### Option 1: SQL Server Management Studio (Easiest)
```
1. Open: C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts\MASTER_SETUP_UC012_UC013.sql
2. Connect to: localhost (or your SQL Server instance)
3. Database: eServicesDb
4. Press F5
```

#### Option 2: PowerShell (From VS Terminal)
```powershell
cd "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts"

# If you have Invoke-Sqlcmd:
Invoke-Sqlcmd -ServerInstance "localhost" -Database "eServicesDb" -InputFile "MASTER_SETUP_UC012_UC013.sql" -Verbose

# Or if you have sqlcmd:
sqlcmd -S localhost -d eServicesDb -E -i "MASTER_SETUP_UC012_UC013.sql"
```

## What the Script Will Do:

✅ **Step 1:** Add `PropertyFacilitiesManagerReview` ResponsibilityType  
✅ **Step 2:** Add `PropertyFacilitiesManagerId` AppSetting (with auto-selected user)  
✅ **Step 3:** Verify `MaintenanceManagerId` AppSetting exists  
✅ **Step 4:** Run complete verification and show results  

## Expected Output:
```
==========================================
UC012 → UC013 WORKFLOW MASTER SETUP
==========================================
...
✅ Property & Facilities Manager Review responsibility type added successfully.
✅ PropertyFacilitiesManagerId AppSetting created: <Name> (ID: <ID>)
✅ MaintenanceManagerId AppSetting exists

🎉 ALL REQUIREMENTS MET!

You can now test the UC012 → UC013 workflow:
  1. Sign maintenance job card (UC012)
  2. Verify PropertyFacilitiesManagerReview queue created
  3. Verify notification sent to Facilities Manager
  4. Verify application status (minor: unchanged, major: CustomerQueryPending)

Test Application:
  Application ID: 5218 (EHC2026032600001)
  Maintenance ID: 2008
  Defect Type: Habitable - Minor defects
```

## After Running Successfully:

1. ✅ Navigate to: `/PropertyLeaseApplication/MaintenanceJobSheet/2008`
2. ✅ Sign the job card with test data
3. ✅ Verify workflow routing works!

## If You Get Errors:

Run the verification script to see what's missing:
```sql
C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Scripts\verify_uc012_uc013_setup.sql
```

## Files Ready:
- ✅ MASTER_SETUP_UC012_UC013.sql - **FIXED & READY**
- ✅ verify_uc012_uc013_setup.sql - Verification helper
- ✅ add_property_facilities_manager_appsetting.sql - Individual setup
- ✅ UC012_UC013_DATABASE_SETUP_REQUIRED.md - Full documentation

---

**Just run it in SSMS and you're good to go!** 🚀
