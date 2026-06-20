-- =====================================================================
-- Fix Incorrect ContextKey in Recent Migrations
-- =====================================================================
-- Two migrations have WRONG ContextKey and need to be corrected:
--   1. 202603270800000_AddLeaseAgreementEnhancements
--   2. 202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication
--
-- WRONG:  C8.eServices.Mvc.Migrations.Configuration
-- CORRECT: C8.eServices.Mvc.DataAccessLayer.eServicesDbContext
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  Fix Incorrect ContextKey in Migrations                    ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''

DECLARE @CorrectContextKey NVARCHAR(512) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'
DECLARE @WrongContextKey NVARCHAR(512) = 'C8.eServices.Mvc.Migrations.Configuration'

-- Check current state
PRINT '=== Current State ==='
SELECT 
    MigrationId,
    ContextKey,
    CASE 
        WHEN ContextKey = @CorrectContextKey THEN '✅ CORRECT'
        WHEN ContextKey = @WrongContextKey THEN '❌ WRONG'
        ELSE '⚠️  UNKNOWN'
    END AS Status
FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603270800000_AddLeaseAgreementEnhancements',
    '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication'
)

PRINT ''
PRINT '=== Fixing ContextKey ==='
PRINT ''

-- Fix Migration 1
UPDATE [dbo].[__MigrationHistory]
SET ContextKey = @CorrectContextKey
WHERE MigrationId = '202603270800000_AddLeaseAgreementEnhancements'
  AND ContextKey = @WrongContextKey

IF @@ROWCOUNT > 0
    PRINT '✅ Fixed: 202603270800000_AddLeaseAgreementEnhancements'
ELSE
    PRINT '⏭️  Skipped: 202603270800000_AddLeaseAgreementEnhancements (already correct or missing)'

-- Fix Migration 2
UPDATE [dbo].[__MigrationHistory]
SET ContextKey = @CorrectContextKey
WHERE MigrationId = '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication'
  AND ContextKey = @WrongContextKey

IF @@ROWCOUNT > 0
    PRINT '✅ Fixed: 202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication'
ELSE
    PRINT '⏭️  Skipped: 202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication (already correct or missing)'

PRINT ''
PRINT '=== Verification ==='
SELECT 
    MigrationId,
    ContextKey,
    CASE 
        WHEN ContextKey = @CorrectContextKey THEN '✅ CORRECT'
        ELSE '❌ STILL WRONG'
    END AS Status
FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603270800000_AddLeaseAgreementEnhancements',
    '202603270810000_AddDSTVAndDepositFieldsToPropertyLeaseApplication'
)

PRINT ''
PRINT '✅ ContextKey correction complete!'
PRINT ''
PRINT 'All migrations now use: C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'
