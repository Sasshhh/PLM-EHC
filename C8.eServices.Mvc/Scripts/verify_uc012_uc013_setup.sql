-- =============================================
-- VERIFY RESPONSIBILITY TYPES FOR UC012/UC013 WORKFLOW
-- =============================================
-- Purpose: Check which responsibility types exist and which are missing
-- =============================================

PRINT '=========================================='
PRINT 'RESPONSIBILITY TYPES VERIFICATION'
PRINT '=========================================='
PRINT ''

-- =============================================
-- 1. Check for Maintenance Job Sheet responsibility type
-- =============================================
PRINT '-------------------------------------------'
PRINT '1. Maintenance Job Sheet (UC012):'
PRINT '-------------------------------------------'
IF EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_maintanance_job_sheet' AND IsDeleted = 0)
BEGIN
    SELECT 
        '✅ EXISTS' AS Status,
        [Key], 
        [Name], 
        [Description],
        IsActive
    FROM ResponsibilityTypes 
    WHERE [Key] = 'r_maintanance_job_sheet'
END
ELSE
BEGIN
    PRINT '❌ MISSING: r_maintanance_job_sheet'
    PRINT '   This is used for routing to Maintenance Manager (UC012)'
END
PRINT ''

-- =============================================
-- 2. Check for Property & Facilities Manager Review responsibility type
-- =============================================
PRINT '-------------------------------------------'
PRINT '2. Property & Facilities Manager Review (UC013):'
PRINT '-------------------------------------------'
IF EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review' AND IsDeleted = 0)
BEGIN
    SELECT 
        '✅ EXISTS' AS Status,
        [Key], 
        [Name], 
        [Description],
        IsActive
    FROM ResponsibilityTypes 
    WHERE [Key] = 'r_property_facilities_manager_review'
END
ELSE
BEGIN
    PRINT '❌ MISSING: r_property_facilities_manager_review'
    PRINT '   This is used for routing to Facilities Manager (UC013)'
    PRINT ''
    PRINT '   ACTION REQUIRED:'
    PRINT '   Run script: add_facilities_manager_responsibility_type.sql'
END
PRINT ''

-- =============================================
-- 3. List ALL active Responsibility Types
-- =============================================
PRINT '-------------------------------------------'
PRINT '3. ALL Active Responsibility Types:'
PRINT '-------------------------------------------'
SELECT 
    [Key],
    [Name],
    [Description],
    IsActive
FROM ResponsibilityTypes
WHERE IsDeleted = 0
ORDER BY [Name]

PRINT ''

-- =============================================
-- 4. Check for Maintenance Manager AppSetting
-- =============================================
PRINT '-------------------------------------------'
PRINT '4. Maintenance Manager AppSetting:'
PRINT '-------------------------------------------'
IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'MaintenanceManagerId')
BEGIN
    SELECT 
        '✅ EXISTS' AS Status,
        [Key], 
        [Value] AS UserID,
        c.FirstName + ' ' + c.LastName AS ManagerName,
        c.Email
    FROM AppSettings a
    INNER JOIN Customers c ON CAST(a.Value AS INT) = c.Id
    WHERE a.[Key] = 'MaintenanceManagerId'
END
ELSE
BEGIN
    PRINT '❌ MISSING: MaintenanceManagerId AppSetting'
    PRINT '   This is used as fallback when complex has no assigned Maintenance Manager'
    PRINT ''
    PRINT '   ACTION REQUIRED:'
    PRINT '   Run script: add_maintenance_manager_appsettings.sql'
END
PRINT ''

-- =============================================
-- 5. Check for Property & Facilities Manager AppSetting
-- =============================================
PRINT '-------------------------------------------'
PRINT '5. Property & Facilities Manager AppSetting:'
PRINT '-------------------------------------------'
IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
BEGIN
    SELECT 
        '✅ EXISTS' AS Status,
        [Key], 
        [Value] AS UserID,
        c.FirstName + ' ' + c.LastName AS ManagerName,
        c.Email
    FROM AppSettings a
    INNER JOIN Customers c ON CAST(a.Value AS INT) = c.Id
    WHERE a.[Key] = 'PropertyFacilitiesManagerId'
END
ELSE
BEGIN
    PRINT '❌ MISSING: PropertyFacilitiesManagerId AppSetting'
    PRINT '   This is used as fallback when complex has no assigned Facilities Manager'
    PRINT ''
    PRINT '   ACTION REQUIRED:'
    PRINT '   INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CapturedDateTime)'
    PRINT '   VALUES (''PropertyFacilitiesManagerId'', ''<USER_ID>'', ''Default Property & Facilities Manager for maintenance approvals'', 1, 0, GETDATE())'
END
PRINT ''

-- =============================================
-- 6. Check Complex Assignments
-- =============================================
PRINT '-------------------------------------------'
PRINT '6. Complex Manager Assignments:'
PRINT '-------------------------------------------'
SELECT 
    pc.Name AS ComplexName,
    mm.FirstName + ' ' + mm.LastName AS MaintenanceManager,
    fm.FirstName + ' ' + fm.LastName AS FacilitiesManager
FROM PreferredComplexAreas pc
LEFT JOIN Customers mm ON pc.MaintenanceManagerId = mm.Id
LEFT JOIN Customers fm ON pc.PropertyFacilitiesManagerId = fm.Id
WHERE pc.IsDeleted = 0
ORDER BY pc.Name

PRINT ''
PRINT '=========================================='
PRINT 'NEXT ACTIONS:'
PRINT '=========================================='
PRINT ''

-- Determine what needs to be done
DECLARE @MissingFacilitiesRT BIT = 0
DECLARE @MissingMaintenanceMgrSetting BIT = 0
DECLARE @MissingFacilitiesMgrSetting BIT = 0

IF NOT EXISTS (SELECT 1 FROM ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review' AND IsDeleted = 0)
    SET @MissingFacilitiesRT = 1

IF NOT EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'MaintenanceManagerId')
    SET @MissingMaintenanceMgrSetting = 1

IF NOT EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
    SET @MissingFacilitiesMgrSetting = 1

IF @MissingFacilitiesRT = 1
BEGIN
    PRINT '1. ❌ RUN SCRIPT: add_facilities_manager_responsibility_type.sql'
    PRINT '   Location: C8.eServices.Mvc\Scripts\'
    PRINT ''
END

IF @MissingMaintenanceMgrSetting = 1
BEGIN
    PRINT '2. ❌ RUN SCRIPT: add_maintenance_manager_appsettings.sql'
    PRINT '   Location: C8.eServices.Mvc\Scripts\'
    PRINT ''
END

IF @MissingFacilitiesMgrSetting = 1
BEGIN
    PRINT '3. ❌ CREATE AppSetting for PropertyFacilitiesManagerId'
    PRINT '   Find a user with Property Manager or Facilities Manager role'
    PRINT '   INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CapturedDateTime)'
    PRINT '   VALUES (''PropertyFacilitiesManagerId'', <USER_ID>, ''Default Property & Facilities Manager'', 1, 0, GETDATE())'
    PRINT ''
END

IF @MissingFacilitiesRT = 0 AND @MissingMaintenanceMgrSetting = 0 AND @MissingFacilitiesMgrSetting = 0
BEGIN
    PRINT '✅ ALL REQUIRED DATA EXISTS!'
    PRINT ''
    PRINT 'You can now test UC012 → UC013 workflow:'
    PRINT '1. Sign maintenance job card (UC012)'
    PRINT '2. Verify routing to Facilities Manager (UC013)'
    PRINT '3. Check PropertyFacilitiesManagerReview queue created'
END

PRINT ''
PRINT '=========================================='
PRINT 'Verification Complete'
PRINT '=========================================='
