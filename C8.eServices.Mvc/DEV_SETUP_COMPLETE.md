# DEV Environment Setup Complete! ✅

## Summary

Your development environment has been successfully configured for the Maintenance Defect Workflow.

---

## ✅ What's Been Configured

### 1. User Role Assignments

| User | Role | Customer ID | Name |
|------|------|-------------|------|
| **COESolarDev11** | Maintenance Manager | 1271 | Moloko Baholo |
| **COESolarDev05** | Property & Facilities Manager | 1287 | Mamoepi RMM |

### 2. AppSettings

| Key | Value (Customer ID) | Description |
|-----|---------------------|-------------|
| `u_maintenance_manager` | 1271 | COESolarDev11 - System-wide fallback |
| `u_property_facilities_manager` | 1287 | COESolarDev05 - System-wide assignment |

### 3. Complex Assignments

- **Total Complexes**: 98
- **Assigned to Maintenance Manager**: 98 (100%)
- **Maintenance Manager**: Moloko Baholo (COESolarDev11, Customer ID: 1271)

All 98 complexes in the `PreferredComplexAreas` table have been assigned to COESolarDev11 as their Maintenance Manager.

---

## 🧪 Testing Your Setup

### Test 1: Login as Maintenance Manager
```
Username: COESolarDev11
Expected Navigation Menu: "Maintenance" → "Maintenance Job Sheets"
```

**What to Test:**
1. Navigate to PropertyLeaseInspections
2. Verify you see maintenance job sheets assigned to you
3. Test completing a maintenance job sheet
4. Upload maintenance documentation
5. Classify defect severity (Minor vs Major)

### Test 2: Login as Property & Facilities Manager
```
Username: COESolarDev05
Expected Navigation Menu: "Facilities Management" → "Major Defect Reviews"
```

**What to Test:**
1. Navigate to PropertyLeaseInspections  
2. Verify you see major defect reviews
3. Test approving a major defect maintenance completion
4. Test rejecting (sending back for more work)
5. Verify application returns to inspection after approval

### Test 3: Complete Workflow Test

**Scenario: Minor Defect**
1. Login as Letting Officer/Client Services Officer
2. Conduct unit inspection → Mark as "Minor Defects"
3. Verify Maintenance Manager (COESolarDev11) receives work item
4. Login as COESolarDev11
5. Complete maintenance → Classify as "Minor Defects"
6. **Expected Result**: Application continues to next stage automatically (no Facilities Manager review)

**Scenario: Major Defect**
1. Conduct unit inspection → Mark as "Major Defects"
2. Verify Maintenance Manager (COESolarDev11) receives work item
3. Login as COESolarDev11
4. Complete maintenance → Classify as "Major Defects"
5. **Expected Result**: Application HALTS, Property & Facilities Manager (COESolarDev05) receives work item
6. Login as COESolarDev05
7. Review and approve
8. **Expected Result**: Application returns to inspection scheduling

---

## 📊 Database State

### PreferredComplexAreas Sample
```
Airport Park → Maintenance Manager: Moloko Baholo (COESolarDev11)
Delville → Maintenance Manager: Moloko Baholo (COESolarDev11)
Masisulu Women's Hostel → Maintenance Manager: Moloko Baholo (COESolarDev11)
... (all 98 complexes assigned)
```

### AppSettings
```
u_maintenance_manager = 1271 (COESolarDev11)
u_property_facilities_manager = 1287 (COESolarDev05)
```

### AspNetRoles
```
Maintenance Manager (ID: B3260F7C-666B-47D6-A016-6DD5470BF205)
Property & Facilities Manager (ID: E54AB40A-FEA1-4419-966F-FD9DC5FDA675)
```

---

## 🚀 Next Steps

1. **Restart Your Application**
   - Stop IIS / Application Pool
   - Start IIS / Application Pool
   - This ensures role assignments are picked up

2. **Test Navigation Menus**
   - Login as COESolarDev11
   - Verify "Maintenance" menu appears
   - Login as COESolarDev05
   - Verify "Facilities Management" menu appears

3. **Run Through Test Scenarios**
   - Use the test scenarios above
   - Document any issues encountered

4. **Ready for Production?**
   - Once dev testing is complete
   - Use `PRODUCTION_DEPLOYMENT.sql` for production
   - Follow `PRODUCTION_DEPLOYMENT_CHECKLIST.md`

---

## 📝 Configuration Details

### Database: CRMPLMDEV_2025

### Tables Modified:
- ✅ `AspNetUserRoles` - Added role assignments
- ✅ `AppSettings` - Added 2 new entries
- ✅ `PreferredComplexAreas` - All complexes assigned maintenance managers

### No Changes Needed:
- ❌ `AspNetRoles` - Already created by MASTER_SETUP script
- ❌ `ResponsibilityTypes` - Already created by MASTER_SETUP script
- ❌ `PreferredComplexAreas.MaintenanceManagerId` column - Already exists

---

## 🔍 Verification Queries

Run these to verify your setup:

```sql
-- Check user roles
SELECT 
    u.UserName,
    r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName IN ('COESolarDev11', 'COESolarDev05')
ORDER BY u.UserName, r.Name

-- Check AppSettings
SELECT [Key], [Value], [Description]
FROM AppSettings
WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')

-- Check complex assignments
SELECT 
    COUNT(*) AS TotalComplexes,
    SUM(CASE WHEN MaintenanceManagerId IS NOT NULL THEN 1 ELSE 0 END) AS AssignedCount,
    SUM(CASE WHEN MaintenanceManagerId IS NULL THEN 1 ELSE 0 END) AS UnassignedCount
FROM PreferredComplexAreas
WHERE IsDeleted = 0
```

---

## ✅ Checklist

- [x] COESolarDev11 assigned to Maintenance Manager role
- [x] COESolarDev05 assigned to Property & Facilities Manager role
- [x] AppSettings created with correct Customer IDs
- [x] All 98 complexes assigned to COESolarDev11
- [ ] Application restarted to pick up role changes
- [ ] Login tested for COESolarDev11
- [ ] Login tested for COESolarDev05
- [ ] Navigation menus verified
- [ ] Minor defect workflow tested
- [ ] Major defect workflow tested

---

## 📞 Troubleshooting

### Issue: Navigation menu doesn't show
**Solution**: Restart your application. Role changes require app restart.

### Issue: User can't see work items
**Solution**: Check RoundRobinQueue table - verify assignments are being created

### Issue: AppSettings not working
**Solution**: Verify Customer IDs are correct:
```sql
SELECT Id, FirstName, LastName, SystemUserId 
FROM Customers 
WHERE Id IN (1271, 1287)
```

---

## 🎉 You're Ready!

Your dev environment is fully configured. 

**Time to test the new maintenance workflow!**

---

**Setup Date**: 2026-03-13  
**Database**: CRMPLMDEV_2025  
**Configured By**: Automated Script  
**Status**: ✅ Complete
