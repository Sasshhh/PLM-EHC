-- =============================================
-- DIAGNOSE USERS AND ROLES FOR UC012/UC013 SETUP
-- =============================================
-- Purpose: Find existing users and roles to assign managers
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'USER & ROLE DIAGNOSTIC'
PRINT '=========================================='
PRINT ''

-- =============================================
-- 1. Check what roles exist in the system
-- =============================================
PRINT '-------------------------------------------'
PRINT '1. ALL ROLES IN SYSTEM:'
PRINT '-------------------------------------------'
SELECT 
    r.Id AS RoleId,
    r.Name AS RoleName,
    COUNT(ur.UserId) AS UserCount
FROM AspNetRoles r
LEFT JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
GROUP BY r.Id, r.Name
ORDER BY r.Name

PRINT ''

-- =============================================
-- 2. Find users who might be suitable for Facilities Manager
-- =============================================
PRINT '-------------------------------------------'
PRINT '2. POTENTIAL FACILITIES MANAGER USERS:'
PRINT '-------------------------------------------'
SELECT 
    c.Id AS CustomerId,
    c.FirstName + ' ' + c.LastName AS FullName,
    c.EmailAddress AS Email,
    u.UserName,
    r.Name AS CurrentRole,
    c.IsActive,
    c.IsDeleted
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE c.IsDeleted = 0
  AND c.IsActive = 1
  AND r.Name IN (
      'Property Manager', 
      'Facilities Manager', 
      'Housing Supervisor',
      'Revenue Manager',
      'Letting Officer',
      'Community Development Officer',
      'Client Services Officer'
  )
ORDER BY 
    CASE r.Name 
        WHEN 'Facilities Manager' THEN 1
        WHEN 'Property Manager' THEN 2
        WHEN 'Housing Supervisor' THEN 3
        WHEN 'Revenue Manager' THEN 4
        ELSE 5
    END,
    c.FirstName

PRINT ''

-- =============================================
-- 3. Find users who might be suitable for Maintenance Manager
-- =============================================
PRINT '-------------------------------------------'
PRINT '3. POTENTIAL MAINTENANCE MANAGER USERS:'
PRINT '-------------------------------------------'
SELECT 
    c.Id AS CustomerId,
    c.FirstName + ' ' + c.LastName AS FullName,
    c.EmailAddress AS Email,
    u.UserName,
    r.Name AS CurrentRole,
    c.IsActive,
    c.IsDeleted
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE c.IsDeleted = 0
  AND c.IsActive = 1
  AND r.Name IN (
      'Maintenance Manager',
      'Property Manager',
      'Housing Supervisor',
      'Letting Officer'
  )
ORDER BY 
    CASE r.Name 
        WHEN 'Maintenance Manager' THEN 1
        WHEN 'Property Manager' THEN 2
        WHEN 'Housing Supervisor' THEN 3
        ELSE 4
    END,
    c.FirstName

PRINT ''

-- =============================================
-- 4. Show ALL active users (first 20)
-- =============================================
PRINT '-------------------------------------------'
PRINT '4. ALL ACTIVE USERS (First 20):'
PRINT '-------------------------------------------'
SELECT TOP 20
    c.Id AS CustomerId,
    c.FirstName + ' ' + c.LastName AS FullName,
    c.EmailAddress AS Email,
    u.UserName,
    STRING_AGG(r.Name, ', ') AS Roles
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE c.IsDeleted = 0
  AND c.IsActive = 1
GROUP BY c.Id, c.FirstName, c.LastName, c.EmailAddress, u.UserName
ORDER BY c.FirstName

PRINT ''

-- =============================================
-- 5. DECISION HELPER
-- =============================================
PRINT '=========================================='
PRINT 'DECISION HELPER:'
PRINT '=========================================='
PRINT ''

DECLARE @HasFacilitiesMgr BIT = 0
DECLARE @HasMaintenanceMgr BIT = 0
DECLARE @FacilitiesUserId INT = NULL
DECLARE @MaintenanceUserId INT = NULL

-- Check for Facilities Manager
SELECT TOP 1 @FacilitiesUserId = c.Id, @HasFacilitiesMgr = 1
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name IN ('Property Manager', 'Facilities Manager', 'Housing Supervisor')
  AND c.IsDeleted = 0
  AND c.IsActive = 1
ORDER BY 
    CASE r.Name 
        WHEN 'Facilities Manager' THEN 1
        WHEN 'Property Manager' THEN 2
        WHEN 'Housing Supervisor' THEN 3
    END

-- Check for Maintenance Manager
SELECT TOP 1 @MaintenanceUserId = c.Id, @HasMaintenanceMgr = 1
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name IN ('Maintenance Manager', 'Property Manager', 'Housing Supervisor')
  AND c.IsDeleted = 0
  AND c.IsActive = 1

IF @HasFacilitiesMgr = 1
BEGIN
    PRINT '✅ FOUND suitable Facilities Manager candidate!'
    SELECT 
        '✅ USE THIS USER:' AS Recommendation,
        c.Id AS CustomerId,
        c.FirstName + ' ' + c.LastName AS FullName,
        r.Name AS Role
    FROM Customers c
    INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE c.Id = @FacilitiesUserId
    PRINT ''
END
ELSE
BEGIN
    PRINT '❌ NO suitable Facilities Manager found!'
    PRINT ''
    PRINT 'OPTION 1: Create "Facilities Manager" or "Property Manager" role'
    PRINT '  Run: create_facilities_manager_role.sql'
    PRINT ''
    PRINT 'OPTION 2: Assign existing user to Property Manager role'
    PRINT '  Run: assign_property_manager_role.sql'
    PRINT ''
    PRINT 'OPTION 3: Use an existing user with similar role'
    PRINT '  Pick a user from section 4 above and manually insert AppSetting'
    PRINT ''
END

IF @HasMaintenanceMgr = 1
BEGIN
    PRINT '✅ FOUND suitable Maintenance Manager candidate!'
    SELECT 
        '✅ USE THIS USER:' AS Recommendation,
        c.Id AS CustomerId,
        c.FirstName + ' ' + c.LastName AS FullName,
        r.Name AS Role
    FROM Customers c
    INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE c.Id = @MaintenanceUserId
    PRINT ''
END
ELSE
BEGIN
    PRINT '❌ NO suitable Maintenance Manager found!'
    PRINT ''
    PRINT 'OPTION 1: Create "Maintenance Manager" role'
    PRINT '  Run: create_maintenance_manager_role.sql'
    PRINT ''
    PRINT 'OPTION 2: Use an existing user'
    PRINT '  Pick a user from section 4 above and manually insert AppSetting'
    PRINT ''
END

PRINT '=========================================='
PRINT 'NEXT STEPS:'
PRINT '=========================================='
PRINT ''

IF @HasFacilitiesMgr = 0 OR @HasMaintenanceMgr = 0
BEGIN
    PRINT '1. Review the users and roles listed above'
    PRINT '2. Choose ONE of these options:'
    PRINT ''
    PRINT '   OPTION A: Create missing roles and assign users'
    PRINT '   Run: create_missing_manager_roles_and_users.sql'
    PRINT ''
    PRINT '   OPTION B: Manually set AppSettings with existing user IDs'
    PRINT '   Run: manual_set_manager_appsettings.sql'
    PRINT ''
    PRINT '3. Re-run MASTER_SETUP_UC012_UC013.sql'
    PRINT ''
END
ELSE
BEGIN
    PRINT '✅ ALL REQUIRED USERS FOUND!'
    PRINT ''
    PRINT '1. Re-run: MASTER_SETUP_UC012_UC013.sql'
    PRINT '   It should now succeed'
    PRINT ''
END

PRINT '=========================================='
PRINT 'Diagnostic Complete'
PRINT '=========================================='
