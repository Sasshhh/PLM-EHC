-- =====================================================================
-- Force Mark Migrations as Applied (USE WITH CAUTION)
-- =====================================================================
-- This script manually marks migrations as applied in __MigrationHistory
-- ONLY use this if:
-- 1. The columns/tables ALREADY EXIST in the database
-- 2. You verified this with Check_Existing_Columns.sql
-- 3. Update-Database is failing due to state mismatch
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '=== Checking Current Migration Status ==='
PRINT ''

-- First, let's see what's already there
SELECT MigrationId, ContextKey
FROM [dbo].[__MigrationHistory]
WHERE MigrationId >= '202603260000000'
ORDER BY MigrationId DESC

PRINT ''
PRINT '=== Checking if target migrations are already recorded ==='
PRINT ''

DECLARE @ContextKey NVARCHAR(300) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'

-- Check each migration we care about
SELECT 
    CASE WHEN EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603260709130_AddBankingDetailsOnly')
         THEN '✅ EXISTS' ELSE '❌ MISSING' END AS [AddBankingDetailsOnly],
    CASE WHEN EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270800000_AddLeaseAgreementEnhancements')
         THEN '✅ EXISTS' ELSE '❌ MISSING' END AS [AddLeaseAgreementEnhancements],
    CASE WHEN EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication')
         THEN '✅ EXISTS' ELSE '❌ MISSING' END AS [AddDSTVAndDepositFields],
    CASE WHEN EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster')
         THEN '✅ EXISTS' ELSE '❌ MISSING' END AS [AddBankingDetailsToLeaseAgreementMaster],
    CASE WHEN EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280915000_MakeHasDSTVNullable')
         THEN '✅ EXISTS' ELSE '❌ MISSING' END AS [MakeHasDSTVNullable]

PRINT ''
PRINT '=== STOP HERE AND REVIEW ===='
PRINT ''
PRINT 'Before running the INSERT statements below:'
PRINT '1. Verify that columns ACTUALLY EXIST using Check_Existing_Columns.sql'
PRINT '2. Make sure you have a database backup'
PRINT '3. Only run INSERT for migrations marked as MISSING above'
PRINT ''
PRINT 'To execute, uncomment the INSERT statements below'
PRINT ''

/*
-- =====================================================================
-- DANGER ZONE: Uncomment to force-mark migrations as applied
-- =====================================================================

DECLARE @ContextKey NVARCHAR(300) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'
DECLARE @ProductVersion NVARCHAR(32) = '6.5.1'

-- Get a sample Model blob from existing migration (we'll reuse it as placeholder)
DECLARE @ModelBlob VARBINARY(MAX)
SELECT TOP 1 @ModelBlob = Model FROM [dbo].[__MigrationHistory] ORDER BY MigrationId DESC

-- 1. AddBankingDetailsOnly
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603260709130_AddBankingDetailsOnly')
BEGIN
    PRINT 'Inserting: 202603260709130_AddBankingDetailsOnly'
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603260709130_AddBankingDetailsOnly', @ContextKey, @ModelBlob, @ProductVersion)
END

-- 2. AddLeaseAgreementEnhancements
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270800000_AddLeaseAgreementEnhancements')
BEGIN
    PRINT 'Inserting: 202603270800000_AddLeaseAgreementEnhancements'
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603270800000_AddLeaseAgreementEnhancements', @ContextKey, @ModelBlob, @ProductVersion)
END

-- 3. AddDSTVAndDepositFieldsToPropertyLeaseApplication
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication')
BEGIN
    PRINT 'Inserting: 202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication'
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication', @ContextKey, @ModelBlob, @ProductVersion)
END

-- 4. AddBankingDetailsToLeaseAgreementMaster
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster')
BEGIN
    PRINT 'Inserting: 202603280900000_AddBankingDetailsToLeaseAgreementMaster'
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603280900000_AddBankingDetailsToLeaseAgreementMaster', @ContextKey, @ModelBlob, @ProductVersion)
END

-- 5. MakeHasDSTVNullable
IF NOT EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280915000_MakeHasDSTVNullable')
BEGIN
    PRINT 'Inserting: 202603280915000_MakeHasDSTVNullable'
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES ('202603280915000_MakeHasDSTVNullable', @ContextKey, @ModelBlob, @ProductVersion)
END

PRINT ''
PRINT '✅ Migration records inserted'
PRINT ''
PRINT 'Verify with:'
PRINT 'SELECT MigrationId FROM [dbo].[__MigrationHistory] WHERE MigrationId >= ''202603260000000'' ORDER BY MigrationId DESC'
*/
