-- =============================================
-- DEV ENVIRONMENT SETUP
-- Configure Maintenance Manager and Property & Facilities Manager
-- Database: CRMPLMDEV_2025
-- =============================================

USE [CRMPLMDEV_2025]
GO

SET NOCOUNT ON
GO

PRINT '=========================================='
PRINT 'DEV SETUP: Maintenance Workflow Configuration'
PRINT '=========================================='
PRINT ''

-- =============================================
-- STEP 1: Get User and Customer IDs
-- =============================================
PRINT 'STEP 1: Finding Users'
PRINT '=========================================='

DECLARE @MaintenanceMgrSystemUserId INT
DECLARE @MaintenanceMgrCustomerId INT
DECLARE @FacilitiesMgrSystemUserId INT
DECLARE @FacilitiesMgrCustomerId INT
DECLARE @MaintenanceRoleId NVARCHAR(128)
DECLARE @FacilitiesRoleId NVARCHAR(128)

-- Find Maintenance Manager User (COESolarDev11)
-- SystemUsers.EmailAddress -> AspNetUsers.Email -> SystemUsers.Id
SELECT @MaintenanceMgrSystemUserId = su.Id
FROM SystemUsers su
INNER JOIN AspNetUsers u ON su.EmailAddress = u.Email
WHERE u.UserName = 'COESolarDev11'

IF @MaintenanceMgrSystemUserId IS NULL
BEGIN
    PRINT '❌ ERROR: SystemUser for COESolarDev11 not found!'
    PRINT 'Available users:'
    SELECT TOP 10 u.UserName, su.Id AS SystemUserId 
    FROM AspNetUsers u
    LEFT JOIN SystemUsers su ON u.Email = su.EmailAddress
    ORDER BY u.UserName
    RAISERROR('SystemUser for COESolarDev11 not found', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Found COESolarDev11 SystemUser ID: ' + CAST(@MaintenanceMgrSystemUserId AS VARCHAR)
END

-- Get Customer ID for Maintenance Manager
SELECT @MaintenanceMgrCustomerId = Id
FROM Customers
WHERE SystemUserId = @MaintenanceMgrSystemUserId

IF @MaintenanceMgrCustomerId IS NULL
BEGIN
    PRINT '❌ ERROR: Customer record not found for COESolarDev11 (SystemUserId: ' + CAST(@MaintenanceMgrSystemUserId AS VARCHAR) + ')'
    RAISERROR('Customer record not found for COESolarDev11', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Customer ID for Maintenance Manager: ' + CAST(@MaintenanceMgrCustomerId AS VARCHAR)
END

-- Find Facilities Manager User (COESolarDev05)
SELECT @FacilitiesMgrSystemUserId = su.Id
FROM SystemUsers su
INNER JOIN AspNetUsers u ON su.EmailAddress = u.Email
WHERE u.UserName = 'COESolarDev05'

IF @FacilitiesMgrSystemUserId IS NULL
BEGIN
    PRINT '❌ ERROR: SystemUser for COESolarDev05 not found!'
    PRINT 'Available users:'
    SELECT TOP 10 u.UserName, su.Id AS SystemUserId 
    FROM AspNetUsers u
    LEFT JOIN SystemUsers su ON u.Email = su.EmailAddress
    ORDER BY u.UserName
    RAISERROR('SystemUser for COESolarDev05 not found', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Found COESolarDev05 SystemUser ID: ' + CAST(@FacilitiesMgrSystemUserId AS VARCHAR)
END

-- Get Customer ID for Facilities Manager
SELECT @FacilitiesMgrCustomerId = Id
FROM Customers
WHERE SystemUserId = @FacilitiesMgrSystemUserId

IF @FacilitiesMgrCustomerId IS NULL
BEGIN
    PRINT '❌ ERROR: Customer record not found for COESolarDev05 (SystemUserId: ' + CAST(@FacilitiesMgrSystemUserId AS VARCHAR) + ')'
    RAISERROR('Customer record not found for COESolarDev05', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Customer ID for Facilities Manager: ' + CAST(@FacilitiesMgrCustomerId AS VARCHAR)
END

PRINT ''

-- =============================================
-- STEP 2: Get Role IDs
-- =============================================
PRINT 'STEP 2: Finding Roles'
PRINT '=========================================='

SELECT @MaintenanceRoleId = Id
FROM AspNetRoles
WHERE Name = 'Maintenance Manager'

IF @MaintenanceRoleId IS NULL
BEGIN
    PRINT '❌ ERROR: Maintenance Manager role not found!'
    RAISERROR('Maintenance Manager role not found. Run MASTER_SETUP script first.', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Found Maintenance Manager role: ' + @MaintenanceRoleId
END

SELECT @FacilitiesRoleId = Id
FROM AspNetRoles
WHERE Name = 'Property & Facilities Manager'

IF @FacilitiesRoleId IS NULL
BEGIN
    PRINT '❌ ERROR: Property & Facilities Manager role not found!'
    RAISERROR('Property & Facilities Manager role not found. Run MASTER_SETUP script first.', 16, 1)
END
ELSE
BEGIN
    PRINT '✓ Found Property & Facilities Manager role: ' + @FacilitiesRoleId
END

PRINT ''

-- =============================================
-- STEP 3: Assign Users to Roles
-- =============================================
PRINT 'STEP 3: Assigning Users to Roles'
PRINT '=========================================='

-- Get AspNetUsers IDs for role assignment
DECLARE @MaintenanceMgrAspNetUserId NVARCHAR(128)
DECLARE @FacilitiesMgrAspNetUserId NVARCHAR(128)

SELECT @MaintenanceMgrAspNetUserId = u.Id
FROM AspNetUsers u
INNER JOIN SystemUsers su ON u.Email = su.EmailAddress
WHERE su.Id = @MaintenanceMgrSystemUserId

SELECT @FacilitiesMgrAspNetUserId = u.Id
FROM AspNetUsers u
INNER JOIN SystemUsers su ON u.Email = su.EmailAddress
WHERE su.Id = @FacilitiesMgrSystemUserId

-- Assign Maintenance Manager role to COESolarDev11
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles 
    WHERE UserId = @MaintenanceMgrAspNetUserId 
    AND RoleId = @MaintenanceRoleId
)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@MaintenanceMgrAspNetUserId, @MaintenanceRoleId)
    PRINT '✓ COESolarDev11 assigned to Maintenance Manager role'
END
ELSE
BEGIN
    PRINT '⚠ COESolarDev11 already has Maintenance Manager role'
END

-- Assign Facilities Manager role to COESolarDev05
IF NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles 
    WHERE UserId = @FacilitiesMgrAspNetUserId 
    AND RoleId = @FacilitiesRoleId
)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@FacilitiesMgrAspNetUserId, @FacilitiesRoleId)
    PRINT '✓ COESolarDev05 assigned to Property & Facilities Manager role'
END
ELSE
BEGIN
    PRINT '⚠ COESolarDev05 already has Property & Facilities Manager role'
END

PRINT ''

-- =============================================
-- STEP 4: Update AppSettings
-- =============================================
PRINT 'STEP 4: Updating AppSettings'
PRINT '=========================================='

-- Update Maintenance Manager AppSetting
UPDATE AppSettings
SET [Value] = CAST(@MaintenanceMgrCustomerId AS VARCHAR)
WHERE [Key] = 'u_maintenance_manager'

IF @@ROWCOUNT > 0
BEGIN
    PRINT '✓ Updated u_maintenance_manager AppSetting to: ' + CAST(@MaintenanceMgrCustomerId AS VARCHAR)
END
ELSE
BEGIN
    PRINT '❌ ERROR: u_maintenance_manager AppSetting not found'
    RAISERROR('u_maintenance_manager AppSetting not found. Run MASTER_SETUP script first.', 16, 1)
END

-- Update Facilities Manager AppSetting
UPDATE AppSettings
SET [Value] = CAST(@FacilitiesMgrCustomerId AS VARCHAR)
WHERE [Key] = 'u_property_facilities_manager'

IF @@ROWCOUNT > 0
BEGIN
    PRINT '✓ Updated u_property_facilities_manager AppSetting to: ' + CAST(@FacilitiesMgrCustomerId AS VARCHAR)
END
ELSE
BEGIN
    PRINT '❌ ERROR: u_property_facilities_manager AppSetting not found'
    RAISERROR('u_property_facilities_manager AppSetting not found. Run MASTER_SETUP script first.', 16, 1)
END

PRINT ''

-- =============================================
-- STEP 5: Assign Maintenance Manager to ALL Complexes
-- =============================================
PRINT 'STEP 5: Assigning Maintenance Manager to Complexes'
PRINT '=========================================='

-- Count complexes before update
DECLARE @TotalComplexes INT
DECLARE @ComplexesToUpdate INT

SELECT @TotalComplexes = COUNT(*)
FROM PreferredComplexAreas
WHERE IsDeleted = 0

SELECT @ComplexesToUpdate = COUNT(*)
FROM PreferredComplexAreas
WHERE IsDeleted = 0
AND (MaintenanceManagerId IS NULL OR MaintenanceManagerId != @MaintenanceMgrCustomerId)

PRINT 'Total complexes: ' + CAST(@TotalComplexes AS VARCHAR)
PRINT 'Complexes to update: ' + CAST(@ComplexesToUpdate AS VARCHAR)
PRINT ''

-- Update all complexes with Maintenance Manager
UPDATE PreferredComplexAreas
SET MaintenanceManagerId = @MaintenanceMgrCustomerId,
    ModifiedDateTime = GETDATE()
WHERE IsDeleted = 0
AND (MaintenanceManagerId IS NULL OR MaintenanceManagerId != @MaintenanceMgrCustomerId)

PRINT '✓ Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' complexes with MaintenanceManagerId = ' + CAST(@MaintenanceMgrCustomerId AS VARCHAR)
PRINT ''

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '=========================================='
PRINT 'VERIFICATION: Configuration Complete'
PRINT '=========================================='
PRINT ''

-- Verify user role assignments
PRINT '-- User Role Assignments:'
SELECT 
    u.UserName,
    r.Name AS RoleName,
    c.Id AS CustomerId,
    ISNULL(c.FirstName + ' ' + c.LastName, '') AS CustomerName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
LEFT JOIN SystemUsers su ON u.Email = su.EmailAddress
LEFT JOIN Customers c ON su.Id = c.SystemUserId
WHERE u.UserName IN ('COESolarDev11', 'COESolarDev05')
ORDER BY u.UserName, r.Name

PRINT ''

-- Verify AppSettings
PRINT '-- AppSettings Configuration:'
SELECT 
    a.[Key],
    a.[Value] AS CustomerID,
    ISNULL(c.FirstName + ' ' + c.LastName, 'Not Assigned') AS AssignedUser,
    u.UserName
FROM AppSettings a
LEFT JOIN Customers c ON a.[Value] = CAST(c.Id AS VARCHAR)
LEFT JOIN SystemUsers su ON c.SystemUserId = su.Id
LEFT JOIN AspNetUsers u ON su.EmailAddress = u.Email
WHERE a.[Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')

PRINT ''

-- Verify complex assignments
PRINT '-- Complex Assignments Summary:'
SELECT 
    CASE 
        WHEN MaintenanceManagerId IS NULL THEN 'No Maintenance Manager'
        ELSE 'Has Maintenance Manager'
    END AS AssignmentStatus,
    COUNT(*) AS ComplexCount
FROM PreferredComplexAreas
WHERE IsDeleted = 0
GROUP BY 
    CASE 
        WHEN MaintenanceManagerId IS NULL THEN 'No Maintenance Manager'
        ELSE 'Has Maintenance Manager'
    END

PRINT ''

-- Show sample of assigned complexes
PRINT '-- Sample Complex Assignments (First 10):'
SELECT TOP 10
    pca.Name AS ComplexName,
    ISNULL(c.FirstName + ' ' + c.LastName, 'Not Assigned') AS MaintenanceManager,
    u.UserName,
    pca.IsActive
FROM PreferredComplexAreas pca
LEFT JOIN Customers c ON pca.MaintenanceManagerId = c.Id
LEFT JOIN SystemUsers su ON c.SystemUserId = su.Id
LEFT JOIN AspNetUsers u ON su.EmailAddress = u.Email
WHERE pca.IsDeleted = 0
ORDER BY pca.Name

PRINT ''
PRINT '=========================================='
PRINT '✓ DEV SETUP COMPLETED SUCCESSFULLY'
PRINT '=========================================='
PRINT ''
PRINT 'Summary:'
PRINT '  • COESolarDev11 assigned to Maintenance Manager role'
PRINT '  • COESolarDev05 assigned to Property & Facilities Manager role'
PRINT '  • AppSettings updated with Customer IDs'
PRINT '  • All complexes assigned to COESolarDev11 as Maintenance Manager'
PRINT ''
PRINT 'Next Steps:'
PRINT '  1. Restart your application to pick up new role assignments'
PRINT '  2. Login as COESolarDev11 to test Maintenance Manager features'
PRINT '  3. Login as COESolarDev05 to test Facilities Manager features'
PRINT '=========================================='

GO

SET NOCOUNT OFF
