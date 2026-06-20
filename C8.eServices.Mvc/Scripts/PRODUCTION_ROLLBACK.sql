-- =============================================
-- PRODUCTION ROLLBACK SCRIPT
-- Maintenance Defect Workflow - Emergency Rollback
-- 
-- WARNING: This will REMOVE all changes made by the deployment
-- Use only if critical issues are encountered
-- =============================================

USE [eServicesDb]  -- UPDATE THIS WITH YOUR PRODUCTION DATABASE NAME
GO

SET NOCOUNT ON
SET XACT_ABORT ON
GO

PRINT '=========================================='
PRINT 'PRODUCTION ROLLBACK: Maintenance Workflow'
PRINT 'Start Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '=========================================='
PRINT ''
PRINT 'WARNING: This will remove all deployment changes'
PRINT 'Press Ctrl+C to cancel within 10 seconds...'
GO

WAITFOR DELAY '00:00:10'
GO

BEGIN TRANSACTION
BEGIN TRY

    DECLARE @RollbackName NVARCHAR(200) = 'Maintenance_Workflow_Rollback_v1.0'
    
    -- Log rollback start
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeploymentLog')
    BEGIN
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@RollbackName, 'START', 'INFO', 'Rollback started')
    END

    -- =============================================
    -- SECTION 1: Remove ASP.NET Roles
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 1: Removing ASP.NET Roles'
    PRINT '=========================================='

    -- Remove role assignments first
    DELETE FROM dbo.AspNetUserRoles
    WHERE RoleId IN (
        SELECT Id FROM dbo.AspNetRoles 
        WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')
    )
    PRINT '✓ User role assignments removed'

    -- Remove roles
    DELETE FROM dbo.AspNetRoles
    WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')
    PRINT '✓ Roles removed'

    -- =============================================
    -- SECTION 2: Remove ResponsibilityTypes
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 2: Removing ResponsibilityTypes'
    PRINT '=========================================='

    -- Check for any RoundRobinQueue entries using this type
    IF EXISTS (
        SELECT 1 FROM dbo.RoundRobinQueue rq
        INNER JOIN dbo.ResponsibilityTypes rt ON rq.ResponsibilityTypeId = rt.Id
        WHERE rt.[Key] = 'r_property_facilities_manager_review'
    )
    BEGIN
        PRINT '⚠ WARNING: Found RoundRobinQueue entries using this ResponsibilityType'
        PRINT '   Marking them as archived...'
        
        UPDATE rq
        SET StatusId = (SELECT Id FROM Status WHERE [Key] = 'Archived')
        FROM dbo.RoundRobinQueue rq
        INNER JOIN dbo.ResponsibilityTypes rt ON rq.ResponsibilityTypeId = rt.Id
        WHERE rt.[Key] = 'r_property_facilities_manager_review'
    END

    DELETE FROM dbo.ResponsibilityTypes
    WHERE [Key] = 'r_property_facilities_manager_review'
    PRINT '✓ ResponsibilityType removed'

    -- =============================================
    -- SECTION 3: Remove AppSettings
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 3: Removing AppSettings'
    PRINT '=========================================='

    DELETE FROM dbo.AppSettings
    WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')
    PRINT '✓ AppSettings removed'

    -- =============================================
    -- SECTION 4: Remove Schema Changes
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'SECTION 4: Removing Schema Changes'
    PRINT '=========================================='

    -- Drop foreign key constraint first
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE CONSTRAINT_NAME = 'FK_PreferredComplexAreas_MaintenanceManager'
    )
    BEGIN
        ALTER TABLE dbo.PreferredComplexAreas
        DROP CONSTRAINT FK_PreferredComplexAreas_MaintenanceManager
        PRINT '✓ Foreign key constraint dropped'
    END

    -- Drop column
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'PreferredComplexAreas' 
        AND COLUMN_NAME = 'MaintenanceManagerId'
    )
    BEGIN
        ALTER TABLE dbo.PreferredComplexAreas
        DROP COLUMN MaintenanceManagerId
        PRINT '✓ MaintenanceManagerId column dropped'
    END

    -- =============================================
    -- VERIFICATION SECTION
    -- =============================================
    PRINT ''
    PRINT '=========================================='
    PRINT 'VERIFICATION: Rollback Complete'
    PRINT '=========================================='

    -- Verify column removed
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'PreferredComplexAreas' 
        AND COLUMN_NAME = 'MaintenanceManagerId'
    )
    BEGIN
        PRINT '✓ Column removed successfully'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: Column still exists', 16, 1)
    END

    -- Verify AppSettings removed
    IF NOT EXISTS (
        SELECT 1 FROM dbo.AppSettings 
        WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')
    )
    BEGIN
        PRINT '✓ AppSettings removed successfully'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: AppSettings still exist', 16, 1)
    END

    -- Verify ResponsibilityType removed
    IF NOT EXISTS (
        SELECT 1 FROM dbo.ResponsibilityTypes 
        WHERE [Key] = 'r_property_facilities_manager_review'
    )
    BEGIN
        PRINT '✓ ResponsibilityType removed successfully'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: ResponsibilityType still exists', 16, 1)
    END

    -- Verify Roles removed
    IF NOT EXISTS (
        SELECT 1 FROM dbo.AspNetRoles 
        WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')
    )
    BEGIN
        PRINT '✓ Roles removed successfully'
    END
    ELSE
    BEGIN
        RAISERROR('❌ CRITICAL: Roles still exist', 16, 1)
    END

    -- Log rollback completion
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeploymentLog')
    BEGIN
        INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
        VALUES (@RollbackName, 'END', 'SUCCESS', 'Rollback completed successfully')
    END

    COMMIT TRANSACTION
    
    PRINT ''
    PRINT '=========================================='
    PRINT '✓ ROLLBACK COMPLETED SUCCESSFULLY'
    PRINT 'End Time: ' + CONVERT(VARCHAR, GETDATE(), 120)
    PRINT '=========================================='
    PRINT ''
    PRINT 'IMPORTANT: You must also:'
    PRINT '1. Redeploy previous application code version'
    PRINT '2. Restart IIS/Application Pool'
    PRINT '3. Verify application functionality'
    PRINT '=========================================='

END TRY
BEGIN CATCH
    -- Error occurred - rollback the rollback (!)
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE()
        DECLARE @ErrorLine INT = ERROR_LINE()
        
        IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeploymentLog')
        BEGIN
            INSERT INTO DeploymentLog (DeploymentName, ScriptSection, Status, Message)
            VALUES (@RollbackName, 'ERROR', 'FAILED', 
                    'Error at line ' + CAST(@ErrorLine AS VARCHAR) + ': ' + @ErrorMessage)
        END
        
        PRINT ''
        PRINT '=========================================='
        PRINT '❌ ROLLBACK FAILED'
        PRINT 'Error Line: ' + CAST(@ErrorLine AS VARCHAR)
        PRINT 'Error Message: ' + @ErrorMessage
        PRINT '=========================================='
        PRINT ''
        PRINT 'CRITICAL: Manual intervention required'
        PRINT 'Contact database administrator immediately'
        PRINT '=========================================='
        
        THROW
    END
END CATCH
GO

-- Display rollback summary
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeploymentLog')
BEGIN
    PRINT ''
    PRINT '=========================================='
    PRINT 'ROLLBACK SUMMARY'
    PRINT '=========================================='

    SELECT 
        ScriptSection,
        Status,
        Message,
        ExecutedAt
    FROM DeploymentLog
    WHERE DeploymentName = 'Maintenance_Workflow_Rollback_v1.0'
    ORDER BY Id DESC
END
GO

SET NOCOUNT OFF
