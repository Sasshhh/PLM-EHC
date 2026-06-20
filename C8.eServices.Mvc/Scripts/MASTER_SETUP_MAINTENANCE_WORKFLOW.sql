-- =============================================
-- MASTER SCRIPT: Complete Maintenance Defect Workflow Setup
-- Run this script in the following order to set up the entire system
-- =============================================

USE [eServicesDb]
GO

PRINT '=========================================='
PRINT 'STEP 1: Add Maintenance Manager Column to PreferredComplexAreas'
PRINT '=========================================='

-- Add MaintenanceManagerId column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PreferredComplexAreas' 
    AND COLUMN_NAME = 'MaintenanceManagerId'
)
BEGIN
    ALTER TABLE dbo.PreferredComplexAreas
    ADD MaintenanceManagerId INT NULL
    
    PRINT 'Column MaintenanceManagerId added to PreferredComplexAreas table.'
    
    -- Add foreign key constraint
    IF NOT EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE CONSTRAINT_NAME = 'FK_PreferredComplexAreas_MaintenanceManager'
    )
    BEGIN
        ALTER TABLE dbo.PreferredComplexAreas
        ADD CONSTRAINT FK_PreferredComplexAreas_MaintenanceManager
        FOREIGN KEY (MaintenanceManagerId) REFERENCES dbo.Customers(Id)
        
        PRINT 'Foreign key constraint added for MaintenanceManagerId.'
    END
END
ELSE
BEGIN
    PRINT 'MaintenanceManagerId column already exists in PreferredComplexAreas.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT 'STEP 2: Add AppSettings for Both Roles'
PRINT '=========================================='

-- Add Maintenance Manager AppSetting
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = 'u_maintenance_manager')
BEGIN
    INSERT INTO dbo.AppSettings ([Key], [Value], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'u_maintenance_manager',
        '0',
        'System-wide fallback Maintenance Manager user ID. Used when complex-specific Maintenance Manager is not assigned.',
        1,
        0,
        GETDATE(),
        GETDATE()
    )
    PRINT 'Maintenance Manager AppSetting added.'
END
ELSE
BEGIN
    PRINT 'Maintenance Manager AppSetting already exists.'
END
GO

-- Add Property & Facilities Manager AppSetting
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = 'u_property_facilities_manager')
BEGIN
    INSERT INTO dbo.AppSettings ([Key], [Value], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'u_property_facilities_manager',
        '0',
        'System-wide Property & Facilities Manager user ID. This role is NOT per-complex and applies across the entire system.',
        1,
        0,
        GETDATE(),
        GETDATE()
    )
    PRINT 'Property & Facilities Manager AppSetting added.'
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager AppSetting already exists.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT 'STEP 3: Add Property & Facilities Manager ReviewResponsibilityType'
PRINT '=========================================='

-- Add PropertyFacilitiesManagerReview ResponsibilityType
IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')
BEGIN
    INSERT INTO dbo.ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'r_property_facilities_manager_review',
        'Property & Facilities Manager Review',
        'Property & Facilities Manager reviews and approves major defect maintenance completion before unit re-inspection',
        1,
        0,
        GETDATE(),
        GETDATE()
    )
    PRINT 'Property & Facilities Manager Review responsibility type added.'
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager Review responsibility type already exists.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT 'STEP 4: Create Roles in AspNetRoles'
PRINT '=========================================='

-- Create Maintenance Manager Role
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Maintenance Manager')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name)
    VALUES (NEWID(), 'Maintenance Manager')
    PRINT 'Maintenance Manager role created.'
END
ELSE
BEGIN
    PRINT 'Maintenance Manager role already exists.'
END
GO

-- Create Property & Facilities Manager Role
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Property & Facilities Manager')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name)
    VALUES (NEWID(), 'Property & Facilities Manager')
    PRINT 'Property & Facilities Manager role created.'
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager role already exists.'
END
GO

PRINT ''
PRINT '=========================================='
PRINT 'VERIFICATION: Check All Additions'
PRINT '=========================================='

PRINT ''
PRINT '-- PreferredComplexAreas columns:'
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'PreferredComplexAreas'
AND c.COLUMN_NAME IN ('LettingOfficerId', 'HousingSuperId', 'MaintenanceManagerId')
ORDER BY c.ORDINAL_POSITION

PRINT ''
PRINT '-- AppSettings for new roles:'
SELECT [Key], [Value], [Description]
FROM dbo.AppSettings
WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')

PRINT ''
PRINT '-- ResponsibilityType for facilities manager:'
SELECT [Key], [Name], [Description]
FROM dbo.ResponsibilityTypes
WHERE [Key] = 'r_property_facilities_manager_review'

PRINT ''
PRINT '-- AspNetRoles for new roles:'
SELECT Id, Name
FROM dbo.AspNetRoles
WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')

PRINT ''
PRINT '=========================================='
PRINT 'NEXT STEPS (MANUAL CONFIGURATION REQUIRED)'
PRINT '=========================================='
PRINT ''
PRINT '1. CREATE USER ACCOUNTS:'
PRINT '   - Navigate to the application and create user accounts for Maintenance Manager and Property & Facilities Manager'
PRINT '   - OR manually insert into AspNetUsers and Customers tables'
PRINT ''
PRINT '2. ASSIGN USERS TO ROLES:'
PRINT '   - Use the application interface to assign users to the new roles'
PRINT '   - OR manually insert into AspNetUserRoles table'
PRINT ''
PRINT '3. UPDATE APPSETTINGS WITH ACTUAL CUSTOMER IDS:'
PRINT '   -- Example:'
PRINT '   -- UPDATE dbo.AppSettings SET [Value] = ''12345'' WHERE [Key] = ''u_maintenance_manager'''
PRINT '   -- UPDATE dbo.AppSettings SET [Value] = ''67890'' WHERE [Key] = ''u_property_facilities_manager'''
PRINT ''
PRINT '4. ASSIGN MAINTENANCE MANAGERS TO COMPLEXES:'
PRINT '   -- Example:'
PRINT '   -- UPDATE dbo.PreferredComplexAreas'
PRINT '   -- SET MaintenanceManagerId = (SELECT Id FROM Customers WHERE SystemUserId = ''USER_ID'')'
PRINT '   -- WHERE ComplexOrAreaName = ''Complex Name'''
PRINT ''
PRINT '=========================================='
PRINT 'SETUP COMPLETE!'
PRINT '=========================================='

GO
