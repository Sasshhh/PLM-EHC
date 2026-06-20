-- =============================================
-- ADD PROPERTY & FACILITIES MANAGER APPSETTING
-- =============================================
-- Purpose: Add default fallback user for PropertyFacilitiesManagerId
-- This is used when a complex doesn't have an assigned Facilities Manager
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'ADD PROPERTY & FACILITIES MANAGER APPSETTING'
PRINT '=========================================='
PRINT ''

-- =============================================
-- STEP 1: Find suitable Property/Facilities Manager user
-- =============================================
PRINT '-------------------------------------------'
PRINT 'STEP 1: Finding suitable user...'
PRINT '-------------------------------------------'

-- Look for users in Property Manager or Facilities Manager roles
DECLARE @FacilitiesManagerId INT = NULL
DECLARE @FacilitiesManagerName NVARCHAR(200)
DECLARE @FacilitiesManagerEmail NVARCHAR(200)

-- Try to find a Property Manager (they often handle facilities too)
SELECT TOP 1
    @FacilitiesManagerId = c.Id,
    @FacilitiesManagerName = c.FirstName + ' ' + c.LastName,
    @FacilitiesManagerEmail = c.Email
FROM Customers c
INNER JOIN AspNetUsers u ON c.SystemIdentityUserId = u.Id
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

IF @FacilitiesManagerId IS NOT NULL
BEGIN
    PRINT 'Found suitable user:'
    PRINT '  ID: ' + CAST(@FacilitiesManagerId AS NVARCHAR(10))
    PRINT '  Name: ' + @FacilitiesManagerName
    PRINT '  Email: ' + @FacilitiesManagerEmail
    PRINT ''
END
ELSE
BEGIN
    PRINT '❌ ERROR: No suitable user found!'
    PRINT '   Please manually create a user with Property Manager or Facilities Manager role first.'
    PRINT ''
    RETURN
END

-- =============================================
-- STEP 2: Check if AppSetting already exists
-- =============================================
PRINT '-------------------------------------------'
PRINT 'STEP 2: Checking for existing AppSetting...'
PRINT '-------------------------------------------'

IF EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = 'PropertyFacilitiesManagerId')
BEGIN
    PRINT '⚠️  AppSetting already exists:'
    
    SELECT 
        a.[Key],
        a.Value AS CurrentUserID,
        c.FirstName + ' ' + c.LastName AS CurrentManagerName,
        c.Email AS CurrentManagerEmail
    FROM AppSettings a
    LEFT JOIN Customers c ON CAST(a.Value AS INT) = c.Id
    WHERE a.[Key] = 'PropertyFacilitiesManagerId'
    
    PRINT ''
    PRINT 'Do you want to update it? (Manually uncomment UPDATE below)'
    PRINT ''
    
    -- Uncomment this to update existing setting:
    /*
    UPDATE AppSettings
    SET Value = CAST(@FacilitiesManagerId AS NVARCHAR(50)),
        ModifiedDateTime = GETDATE()
    WHERE [Key] = 'PropertyFacilitiesManagerId'
    
    PRINT '✅ AppSetting updated successfully.'
    */
END
ELSE
BEGIN
    -- =============================================
    -- STEP 3: Insert new AppSetting
    -- =============================================
    PRINT '-------------------------------------------'
    PRINT 'STEP 3: Creating new AppSetting...'
    PRINT '-------------------------------------------'
    
    INSERT INTO AppSettings ([Key], Value, Description, IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'PropertyFacilitiesManagerId',
        CAST(@FacilitiesManagerId AS NVARCHAR(50)),
        'Default Property & Facilities Manager for UC013 maintenance approval workflow. Used as fallback when complex has no assigned Facilities Manager.',
        1, -- IsActive
        0, -- IsDeleted
        GETDATE(),
        GETDATE()
    )
    
    PRINT '✅ AppSetting created successfully.'
    PRINT ''
END

-- =============================================
-- STEP 4: Verify insertion
-- =============================================
PRINT '-------------------------------------------'
PRINT 'STEP 4: Verification'
PRINT '-------------------------------------------'

SELECT 
    a.[Key],
    a.Value AS UserID,
    a.Description,
    c.FirstName + ' ' + c.LastName AS ManagerName,
    c.Email,
    u.UserName,
    r.Name AS Role,
    a.IsActive,
    a.CapturedDateTime
FROM AppSettings a
INNER JOIN Customers c ON CAST(a.Value AS INT) = c.Id
INNER JOIN AspNetUsers u ON c.SystemIdentityUserId = u.Id
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE a.[Key] = 'PropertyFacilitiesManagerId'

PRINT ''
PRINT '=========================================='
PRINT 'SETUP COMPLETE'
PRINT '=========================================='
PRINT ''
PRINT 'Next Steps:'
PRINT '1. ✅ PropertyFacilitiesManagerId AppSetting created'
PRINT '2. ✅ Fallback user assigned for UC013 workflow'
PRINT '3. Run verify_uc012_uc013_setup.sql to check all requirements'
PRINT '4. Test UC012 → UC013 workflow'
PRINT ''
