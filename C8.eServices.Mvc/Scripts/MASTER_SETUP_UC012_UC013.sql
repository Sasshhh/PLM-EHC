-- =============================================
-- MASTER SETUP FOR UC012 → UC013 WORKFLOW
-- =============================================
-- Purpose: Complete database setup for Maintenance Job Sheet workflow
-- This script runs all required setup steps in the correct order
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'UC012 → UC013 WORKFLOW MASTER SETUP'
PRINT '=========================================='
PRINT 'This script will set up:'
PRINT '  1. Property & Facilities Manager Review ResponsibilityType'
PRINT '  2. PropertyFacilitiesManagerId AppSetting (if needed)'
PRINT '  3. MaintenanceManagerId AppSetting (if needed)'
PRINT '  4. Verification of all requirements'
PRINT ''
PRINT 'Press CTRL+C to cancel, or wait 5 seconds to continue...'
PRINT ''
WAITFOR DELAY '00:00:05'
GO

-- =============================================
-- STEP 1: Add Property & Facilities Manager Review ResponsibilityType
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'STEP 1: Add ResponsibilityType'
PRINT '=========================================='

IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')
BEGIN
    INSERT INTO dbo.ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
    VALUES (
        'r_property_facilities_manager_review',
        'Property & Facilities Manager Review',
        'Property & Facilities Manager reviews and approves major defect maintenance completion before unit re-inspection',
        1, -- IsActive
        0, -- IsDeleted
        GETDATE(),
        GETDATE()
    )

    PRINT '✅ Property & Facilities Manager Review responsibility type added successfully.'
END
ELSE
BEGIN
    PRINT '✓ Property & Facilities Manager Review responsibility type already exists.'
END

-- Verify
SELECT 'Verification:' AS Step, * FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review'
PRINT ''
GO

-- =============================================
-- STEP 2: Add PropertyFacilitiesManagerId AppSetting
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'STEP 2: Add PropertyFacilitiesManagerId AppSetting'
PRINT '=========================================='

-- Find suitable user
DECLARE @FacilitiesManagerId INT = NULL
DECLARE @FacilitiesManagerName NVARCHAR(200)

SELECT TOP 1
    @FacilitiesManagerId = c.Id,
    @FacilitiesManagerName = c.FirstName + ' ' + c.LastName
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

IF @FacilitiesManagerId IS NULL
BEGIN
    PRINT '❌ ERROR: No suitable Facilities Manager user found!'
    PRINT '   Please create a user with Property Manager or Facilities Manager role first.'
    PRINT ''
END
ELSE IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
BEGIN
    PRINT '✓ PropertyFacilitiesManagerId AppSetting already exists.'
    
    SELECT 'Current Setting:' AS Step,
        a.[Key],
        a.Value AS UserID,
        c.FirstName + ' ' + c.LastName AS ManagerName
    FROM AppSettings a
    LEFT JOIN Customers c ON CAST(a.Value AS INT) = c.Id
    WHERE a.[Key] = 'PropertyFacilitiesManagerId'
END
ELSE
BEGIN
    INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
    VALUES (
        'PropertyFacilitiesManagerId',
        CAST(@FacilitiesManagerId AS NVARCHAR(50)),
        'Default Property & Facilities Manager for UC013 maintenance approval workflow',
        1, 0, GETDATE(), GETDATE()
    )

    PRINT '✅ PropertyFacilitiesManagerId AppSetting created: ' + @FacilitiesManagerName + ' (ID: ' + CAST(@FacilitiesManagerId AS NVARCHAR(10)) + ')'
END

PRINT ''
GO

-- =============================================
-- STEP 3: Verify MaintenanceManagerId AppSetting
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'STEP 3: Verify MaintenanceManagerId AppSetting'
PRINT '=========================================='

IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'MaintenanceManagerId')
BEGIN
    PRINT '✓ MaintenanceManagerId AppSetting exists.'
    
    SELECT 'Current Setting:' AS Step,
        a.[Key],
        a.Value AS UserID,
        c.FirstName + ' ' + c.LastName AS ManagerName
    FROM AppSettings a
    LEFT JOIN Customers c ON CAST(a.Value AS INT) = c.Id
    WHERE a.[Key] = 'MaintenanceManagerId'
END
ELSE
BEGIN
    PRINT '⚠️  MaintenanceManagerId AppSetting does NOT exist.'
    PRINT '   This is used as fallback when complex has no assigned Maintenance Manager.'
    PRINT ''
    PRINT '   ACTION REQUIRED: Run add_maintenance_manager_appsettings.sql'
END

PRINT ''
GO

-- =============================================
-- FINAL VERIFICATION
-- =============================================
PRINT ''
PRINT '=========================================='
PRINT 'FINAL VERIFICATION'
PRINT '=========================================='
PRINT ''

-- Check all requirements
DECLARE @AllGood BIT = 1

PRINT '-------------------------------------------'
PRINT 'Requirement Checklist:'
PRINT '-------------------------------------------'

-- 1. ResponsibilityType
IF EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review' AND IsDeleted = 0)
    PRINT '✅ PropertyFacilitiesManagerReview ResponsibilityType exists'
ELSE
BEGIN
    PRINT '❌ PropertyFacilitiesManagerReview ResponsibilityType MISSING'
    SET @AllGood = 0
END

-- 2. Facilities Manager AppSetting
IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
    PRINT '✅ PropertyFacilitiesManagerId AppSetting exists'
ELSE
BEGIN
    PRINT '❌ PropertyFacilitiesManagerId AppSetting MISSING'
    SET @AllGood = 0
END

-- 3. Maintenance Manager AppSetting (optional but recommended)
IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'MaintenanceManagerId')
    PRINT '✅ MaintenanceManagerId AppSetting exists'
ELSE
    PRINT '⚠️  MaintenanceManagerId AppSetting missing (optional)'

-- 4. Maintenance Job Sheet ResponsibilityType (should already exist)
IF EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_maintanance_job_sheet' AND IsDeleted = 0)
    PRINT '✅ MaintenanceJobSheet ResponsibilityType exists'
ELSE
BEGIN
    PRINT '❌ MaintenanceJobSheet ResponsibilityType MISSING'
    SET @AllGood = 0
END

PRINT ''
PRINT '-------------------------------------------'
PRINT 'Summary:'
PRINT '-------------------------------------------'

IF @AllGood = 1
BEGIN
    PRINT '🎉 ALL REQUIREMENTS MET!'
    PRINT ''
    PRINT 'You can now test the UC012 → UC013 workflow:'
    PRINT '  1. Sign maintenance job card (UC012)'
    PRINT '  2. Verify PropertyFacilitiesManagerReview queue created'
    PRINT '  3. Verify notification sent to Facilities Manager'
    PRINT '  4. Verify application status (minor: unchanged, major: CustomerQueryPending)'
    PRINT ''
    PRINT 'Test Application:'
    PRINT '  Application ID: 5218 (EHC2026032600001)'
    PRINT '  Maintenance ID: 2008'
    PRINT '  Defect Type: Habitable - Minor defects'
    PRINT ''
END
ELSE
BEGIN
    PRINT '❌ SETUP INCOMPLETE'
    PRINT '   Please fix the missing requirements above.'
    PRINT ''
END

PRINT '=========================================='
PRINT 'Setup Complete'
PRINT '=========================================='
GO

-- =============================================
-- SHOW FINAL STATE
-- =============================================
PRINT ''
PRINT 'Responsibility Types:'
SELECT [Key], [Name], [Description], IsActive
FROM ResponsibilityTypes
WHERE [Key] IN ('r_maintanance_job_sheet', 'r_property_facilities_manager_review')
  AND IsDeleted = 0

PRINT ''
PRINT 'AppSettings:'
SELECT 
    a.[Key],
    a.Value AS UserID,
    c.FirstName + ' ' + c.LastName AS ManagerName,
    c.EmailAddress AS Email,
    a.Description
FROM AppSettings a
LEFT JOIN Customers c ON CAST(a.Value AS INT) = c.Id
WHERE a.[Key] IN ('MaintenanceManagerId', 'PropertyFacilitiesManagerId')
GO
