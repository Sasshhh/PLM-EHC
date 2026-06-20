-- =============================================
-- PRODUCTION DEPLOYMENT SCRIPT
-- Maintenance Defect Workflow - Complete Setup
-- 
-- IMPORTANT: Review all sections before executing
-- Test on staging environment first if available
-- =============================================

USE [eServicesDb]  -- UPDATE THIS WITH YOUR PRODUCTION DATABASE NAME
GO

SET NOCOUNT ON
SET XACT_ABORT ON  -- Automatic rollback on error
GO

PRINT '=========================================='
PRINT 'PRODUCTION DEPLOYMENT: Maintenance Workflow'
PRINT 'Start Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '=========================================='
PRINT ''

-- Create deployment log table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeploymentLog')
BEGIN
    CREATE TABLE DeploymentLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DeploymentName NVARCHAR(200),
        ScriptSection NVARCHAR(100),
        Status NVARCHAR(50),
        Message NVARCHAR(MAX),
        ExecutedAt DATETIME DEFAULT GETDATE()
    )
    PRINT 'DeploymentLog table created.'
END
GO

-- Start transaction for rollback capability
BEGIN TRANSACTION
BEGIN TRY

    DECLARE @DeploymentName NVARCHAR(200) = 'Maintenance_Defect_Workflow_v1.0'
    
    INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
    VALUES (@DeploymentName, 'START', 'INFO', 'Deployment started')

    -- =============================================
    -- SECTION 1: Database Schema Changes
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 1: Database Schema Changes'
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
        
        PRINT '✓ Column MaintenanceManagerId added to PreferredComplexAreas table.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'SCHEMA', 'SUCCESS', 'MaintenanceManagerId column added')
        
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
            
            PRINT '✓ Foreign key constraint FK_PreferredComplexAreas_MaintenanceManager added.'
            
            INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
            VALUES (@DeploymentName, 'SCHEMA', 'SUCCESS', 'Foreign key constraint added')
        END
    END
    ELSE
    BEGIN
        PRINT '⚠ MaintenanceManagerId column already exists in PreferredComplexAreas.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'SCHEMA', 'SKIPPED', 'MaintenanceManagerId column already exists')
    END

    -- =============================================
    -- SECTION 2: AppSettings Configuration
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 2: AppSettings Configuration'
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
        PRINT '✓ Maintenance Manager AppSetting added.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'APPSETTINGS', 'SUCCESS', 'u_maintenance_manager AppSetting added')
    END
    ELSE
    BEGIN
        PRINT '⚠ Maintenance Manager AppSetting already exists.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'APPSETTINGS', 'SKIPPED', 'u_maintenance_manager AppSetting already exists')
    END

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
        PRINT '✓ Property & Facilities Manager AppSetting added.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'APPSETTINGS', 'SUCCESS', 'u_property_facilities_manager AppSetting added')
    END
    ELSE
    BEGIN
        PRINT '⚠ Property & Facilities Manager AppSetting already exists.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'APPSETTINGS', 'SKIPPED', 'u_property_facilities_manager AppSetting already exists')
    END

    -- =============================================
    -- SECTION 3: ResponsibilityTypes
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 3: ResponsibilityTypes'
    PRINT '=========================================='

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
        PRINT '✓ Property & Facilities Manager Review responsibility type added.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'RESPONSIBILITY_TYPES', 'SUCCESS', 'r_property_facilities_manager_review added')
    END
    ELSE
    BEGIN
        PRINT '⚠ Property & Facilities Manager Review responsibility type already exists.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'RESPONSIBILITY_TYPES', 'SKIPPED', 'r_property_facilities_manager_review already exists')
    END

    -- =============================================
    -- SECTION 4: ASP.NET Roles
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 4: ASP.NET Roles'
    PRINT '=========================================='

    -- Create Maintenance Manager Role
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Maintenance Manager')
    BEGIN
        INSERT INTO dbo.AspNetRoles (Id, Name)
        VALUES (NEWID(), 'Maintenance Manager')
        PRINT '✓ Maintenance Manager role created.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'ROLES', 'SUCCESS', 'Maintenance Manager role created')
    END
    ELSE
    BEGIN
        PRINT '⚠ Maintenance Manager role already exists.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'ROLES', 'SKIPPED', 'Maintenance Manager role already exists')
    END

    -- Create Property & Facilities Manager Role
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Property & Facilities Manager')
    BEGIN
        INSERT INTO dbo.AspNetRoles (Id, Name)
        VALUES (NEWID(), 'Property & Facilities Manager')
        PRINT '✓ Property & Facilities Manager role created.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'ROLES', 'SUCCESS', 'Property & Facilities Manager role created')
    END
    ELSE
    BEGIN
        PRINT '⚠ Property & Facilities Manager role already exists.'
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'ROLES', 'SKIPPED', 'Property & Facilities Manager role already exists')
    END

    -- =============================================
    -- VERIFICATION SECTION
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'VERIFICATION: All Changes'
    PRINT '=========================================='

    -- Verify schema changes
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PreferredComplexAreas' AND COLUMN_NAME = 'MaintenanceManagerId')
    BEGIN
        PRINT '✓ PreferredComplexAreas.MaintenanceManagerId column verified'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: MaintenanceManagerId column not found', 16, 1)
    END

    -- Verify AppSettings
    DECLARE @MaintenanceMgrCount INT, @FacilitiesMgrCount INT
    SELECT @MaintenanceMgrCount = COUNT(*) FROM dbo.AppSettings WHERE [Key] = 'u_maintenance_manager'
    SELECT @FacilitiesMgrCount = COUNT(*) FROM dbo.AppSettings WHERE [Key] = 'u_property_facilities_manager'
    
    IF @MaintenanceMgrCount = 1 AND @FacilitiesMgrCount = 1
    BEGIN
        PRINT '✓ AppSettings verified (2 entries)'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: AppSettings entries not found', 16, 1)
    END

    -- Verify ResponsibilityType
    IF EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')
    BEGIN
        PRINT '✓ ResponsibilityType verified'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: ResponsibilityType not found', 16, 1)
    END

    -- Verify Roles
    DECLARE @MaintenanceRoleCount INT, @FacilitiesRoleCount INT
    SELECT @MaintenanceRoleCount = COUNT(*) FROM dbo.AspNetRoles WHERE Name = 'Maintenance Manager'
    SELECT @FacilitiesRoleCount = COUNT(*) FROM dbo.AspNetRoles WHERE Name = 'Property & Facilities Manager'
    
    IF @MaintenanceRoleCount = 1 AND @FacilitiesRoleCount = 1
    BEGIN
        PRINT '✓ AspNetRoles verified (2 roles)'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: AspNetRoles not found', 16, 1)
    END

    -- All validations passed - commit transaction
    INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
    VALUES (@DeploymentName, 'VERIFICATION', 'SUCCESS', 'All verifications passed')
    
    INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
    VALUES (@DeploymentName, 'END', 'SUCCESS', 'Deployment completed successfully')

    COMMIT TRANSACTION
    
    PRINT ''
    PRINT '=========================================='
    PRINT '✓ DEPLOYMENT COMPLETED SUCCESSFULLY'
    PRINT 'End Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
    PRINT '=========================================='

END TRY
BEGIN CATCH
    -- Error occurred - rollback all changes
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE()
        DECLARE @ErrorLine INT = ERROR_LINE()
        
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@DeploymentName, 'ERROR', 'FAILED', 
                'Error at line ' + CAST(@ErrorLine AS VARCHAR) + ': ' + @ErrorMessage)
        
        PRINT ''
        PRINT '=========================================='
        PRINT '❌ DEPLOYMENT FAILED - ALL CHANGES ROLLED BACK'
        PRINT 'Error Line: ' + CAST(@ErrorLine AS VARCHAR)
        PRINT 'Error Message: ' + @ErrorMessage
        PRINT '=========================================='
        
        -- Re-throw error
        THROW
    END
END CATCH
GO

-- Display deployment summary
PRINT ''
PRINT '=========================================='
PRINT 'DEPLOYMENT SUMMARY'
PRINT '=========================================='

SELECT 
    ScriptSection,
    Status,
    Message,
    ExecutedAt
FROM DeploymentLog
WHERE DeploymentName = 'Maintenance_Defect_Workflow_v1.0'
ORDER BY Id DESC

PRINT ''
PRINT '=========================================='
PRINT 'POST-DEPLOYMENT MANUAL STEPS REQUIRED'
PRINT '=========================================='
PRINT ''
PRINT '1. CREATE USER ACCOUNTS (via application UI or manual SQL)'
PRINT '2. ASSIGN USERS TO ROLES (AspNetUserRoles table)'
PRINT '3. UPDATE APPSETTINGS WITH CUSTOMER IDs:'
PRINT '   UPDATE AppSettings SET [Value] = ''CUSTOMER_ID'' WHERE [Key] = ''u_maintenance_manager'''
PRINT '   UPDATE AppSettings SET [Value] = ''CUSTOMER_ID'' WHERE [Key] = ''u_property_facilities_manager'''
PRINT '4. ASSIGN MAINTENANCE MANAGERS TO COMPLEXES:'
PRINT '   UPDATE PreferredComplexAreas SET MaintenanceManagerId = CUSTOMER_ID WHERE ...'
PRINT ''
PRINT 'See PRODUCTION_DEPLOYMENT_CHECKLIST.md for detailed steps'
PRINT '=========================================='

GO

SET NOCOUNT OFF
