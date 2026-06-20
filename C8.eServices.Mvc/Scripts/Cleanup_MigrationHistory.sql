-- =====================================================================
-- Clean Up Automatic Migration from __MigrationHistory
-- =====================================================================
-- This removes the failed automatic migration entry
-- Run this AFTER Fix_HasDSTV_Migration_Conflict.sql
-- =====================================================================

USE [eServices_db]
GO

PRINT '=== Checking __MigrationHistory for automatic migrations ==='
PRINT ''

-- Show what will be deleted
SELECT 
    MigrationId,
    ContextKey,
    Model,
    ProductVersion
FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '202603260831073_%'
   OR MigrationId LIKE '%AutomaticMigration%'
ORDER BY MigrationId

PRINT ''
PRINT '=== Deleting automatic migration(s) ==='

-- Delete the automatic migration
DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '202603260831073_%'
   OR MigrationId LIKE '%AutomaticMigration%'

PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' row(s) deleted'
PRINT ''

PRINT '=== Current Migration History (Last 10) ==='
SELECT TOP 10
    MigrationId,
    ContextKey
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT ''
PRINT '✅ Cleanup complete!'
PRINT ''
PRINT 'Next step: Run Update-Database in Package Manager Console'
