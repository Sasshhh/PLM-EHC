-- =============================================
-- MANUAL SET MANAGER APPSETTINGS
-- =============================================
-- Purpose: Manually set PropertyFacilitiesManagerId and MaintenanceManagerId
-- using any available active user in the system
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'MANUAL MANAGER APPSETTINGS SETUP'
PRINT '=========================================='
PRINT ''

-- =============================================
-- STEP 1: Find FIRST available active user
-- =============================================
DECLARE @UserId INT = NULL
DECLARE @UserName NVARCHAR(200)

SELECT TOP 1
    @UserId = c.Id,
    @UserName = c.FirstName + ' ' + c.LastName
FROM Customers c
INNER JOIN AspNetUsers u ON CAST(c.Id AS NVARCHAR(128)) = u.Id
WHERE c.IsDeleted = 0
  AND c.IsActive = 1
ORDER BY c.Id

IF @UserId IS NULL
BEGIN
    PRINT '❌ ERROR: No active users found in system!'
    PRINT '   Cannot proceed with setup.'
    RETURN
END

PRINT 'Found user to use for both manager roles:'
PRINT '  User ID: ' + CAST(@UserId AS NVARCHAR(10))
PRINT '  Name: ' + @UserName
PRINT ''
PRINT 'NOTE: You can change these later to specific users if needed'
PRINT ''

-- =============================================
-- STEP 2: Set PropertyFacilitiesManagerId
-- =============================================
PRINT '-------------------------------------------'
PRINT 'Setting PropertyFacilitiesManagerId...'
PRINT '-------------------------------------------'

IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
BEGIN
    UPDATE AppSettings
    SET Value = CAST(@UserId AS NVARCHAR(50)),
        ModifiedDateTime = GETDATE()
    WHERE [Key] = 'PropertyFacilitiesManagerId'
    
    PRINT '✅ PropertyFacilitiesManagerId UPDATED to: ' + @UserName
END
ELSE
BEGIN
    INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
    VALUES (
        'PropertyFacilitiesManagerId',
        CAST(@UserId AS NVARCHAR(50)),
        'Default Property & Facilities Manager for UC013 maintenance approval workflow',
        1, 0, GETDATE(), GETDATE()
    )
    
    PRINT '✅ PropertyFacilitiesManagerId CREATED with: ' + @UserName
END

PRINT ''

-- =============================================
-- STEP 3: Set MaintenanceManagerId
-- =============================================
PRINT '-------------------------------------------'
PRINT 'Setting MaintenanceManagerId...'
PRINT '-------------------------------------------'

IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'MaintenanceManagerId')
BEGIN
    UPDATE AppSettings
    SET Value = CAST(@UserId AS NVARCHAR(50)),
        ModifiedDateTime = GETDATE()
    WHERE [Key] = 'MaintenanceManagerId'
    
    PRINT '✅ MaintenanceManagerId UPDATED to: ' + @UserName
END
ELSE
BEGIN
    INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
    VALUES (
        'MaintenanceManagerId',
        CAST(@UserId AS NVARCHAR(50)),
        'Default Maintenance Manager for UC012 maintenance job card workflow',
        1, 0, GETDATE(), GETDATE()
    )
    
    PRINT '✅ MaintenanceManagerId CREATED with: ' + @UserName
END

PRINT ''

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '=========================================='
PRINT 'VERIFICATION:'
PRINT '=========================================='
PRINT ''

SELECT 
    a.[Key],
    a.Value AS UserID,
    c.FirstName + ' ' + c.LastName AS AssignedUser,
    c.EmailAddress AS Email,
    a.Description
FROM AppSettings a
LEFT JOIN Customers c ON CAST(a.Value AS INT) = c.Id
WHERE a.[Key] IN ('PropertyFacilitiesManagerId', 'MaintenanceManagerId')

PRINT ''
PRINT '=========================================='
PRINT 'SETUP COMPLETE!'
PRINT '=========================================='
PRINT ''
PRINT '✅ Both manager AppSettings are now configured'
PRINT ''
PRINT 'IMPORTANT NOTE:'
PRINT 'This script assigned the SAME user to BOTH roles as a temporary fix.'
PRINT 'This is okay for development/testing, but in production you should:'
PRINT ''
PRINT '1. Create proper "Facilities Manager" and "Maintenance Manager" roles'
PRINT '2. Assign different users to each role'
PRINT '3. Update these AppSettings with the correct user IDs'
PRINT ''
PRINT 'To update later, run:'
PRINT '  UPDATE AppSettings SET Value = <new_user_id> WHERE [Key] = ''PropertyFacilitiesManagerId'''
PRINT '  UPDATE AppSettings SET Value = <new_user_id> WHERE [Key] = ''MaintenanceManagerId'''
PRINT ''
PRINT 'NEXT STEP:'
PRINT 'Re-run verify_uc012_uc013_setup.sql to confirm everything is ready'
PRINT ''
