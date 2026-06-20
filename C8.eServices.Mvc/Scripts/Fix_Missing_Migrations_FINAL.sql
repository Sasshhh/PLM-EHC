-- =====================================================================
-- Fix Missing Migrations in __MigrationHistory
-- =====================================================================
-- Based on diagnosis, these migrations are missing from your database:
--   1. 202603280900000_AddBankingDetailsToLeaseAgreementMaster
--   2. 202603280915000_MakeHasDSTVNullable
--
-- This script will:
--   1. Verify the columns exist (from previous SQL scripts)
--   2. Insert the missing migration records
--   3. Sync EF's state with the database
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

SET NOCOUNT ON

PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  Fix Missing Migrations - Automated Resolution             ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''

-- =====================================================================
-- STEP 1: Verify all required columns exist
-- =====================================================================
PRINT '=== STEP 1: Verifying Database Schema ==='
PRINT ''

DECLARE @MissingColumns INT = 0

-- Check PropertyLeaseApplications.HasDSTV
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') AND name = 'HasDSTV')
BEGIN
    PRINT '❌ ERROR: PropertyLeaseApplications.HasDSTV column missing!'
    PRINT '   Run Fix_HasDSTV_Migration_Conflict.sql first'
    SET @MissingColumns = @MissingColumns + 1
END
ELSE
    PRINT '✅ PropertyLeaseApplications.HasDSTV exists'

-- Check PropertyLeaseApplicationAudits.HasDSTV
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplicationAudits') AND name = 'HasDSTV')
BEGIN
    PRINT '❌ ERROR: PropertyLeaseApplicationAudits.HasDSTV column missing!'
    SET @MissingColumns = @MissingColumns + 1
END
ELSE
    PRINT '✅ PropertyLeaseApplicationAudits.HasDSTV exists'

-- Check PropertyLeaseAgreementMasters.HasDSTV
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'HasDSTV')
BEGIN
    PRINT '❌ ERROR: PropertyLeaseAgreementMasters.HasDSTV column missing!'
    SET @MissingColumns = @MissingColumns + 1
END
ELSE
    PRINT '✅ PropertyLeaseAgreementMasters.HasDSTV exists'

-- Check PropertyLeaseAgreementMasterAudits.HasDSTV
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'HasDSTV')
BEGIN
    PRINT '❌ ERROR: PropertyLeaseAgreementMasterAudits.HasDSTV column missing!'
    SET @MissingColumns = @MissingColumns + 1
END
ELSE
    PRINT '✅ PropertyLeaseAgreementMasterAudits.HasDSTV exists'

-- Check PropertyLeaseAgreementMasters.TenantBankName (from 202603280900000)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'TenantBankName')
BEGIN
    PRINT '❌ ERROR: PropertyLeaseAgreementMasters.TenantBankName column missing!'
    SET @MissingColumns = @MissingColumns + 1
END
ELSE
    PRINT '✅ PropertyLeaseAgreementMasters.TenantBankName exists'

PRINT ''

IF @MissingColumns > 0
BEGIN
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
    PRINT '❌ CANNOT PROCEED: ' + CAST(@MissingColumns AS VARCHAR(10)) + ' column(s) missing!'
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
    PRINT ''
    PRINT 'Action Required:'
    PRINT '1. Run: Fix_HasDSTV_Migration_Conflict.sql'
    PRINT '2. Run: Apply_Banking_Details_Migration.sql (if exists)'
    PRINT '3. Then run this script again'
    PRINT ''
    RAISERROR('Missing columns detected. Fix schema first.', 16, 1)
    RETURN
END

PRINT '✅ All required columns exist in database'
PRINT ''

-- =====================================================================
-- STEP 2: Use the CORRECT ContextKey (FIXED)
-- =====================================================================
PRINT '=== STEP 2: Using Correct ContextKey ==='
PRINT ''

-- ALWAYS use this ContextKey - some recent migrations have WRONG key
DECLARE @ContextKey NVARCHAR(512) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'

PRINT '📝 Using ContextKey: ' + @ContextKey
PRINT '   (Fixed: was using inconsistent key from recent migrations)'
PRINT ''

-- =====================================================================
-- STEP 3: Get Model blob from recent migration
-- =====================================================================
PRINT '=== STEP 3: Preparing Migration Model ==='
PRINT ''

DECLARE @ModelBlob VARBINARY(MAX)
DECLARE @ProductVersion NVARCHAR(32)

-- Get model blob from most recent migration
SELECT TOP 1 
    @ModelBlob = Model,
    @ProductVersion = ProductVersion
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT '📦 Model blob retrieved from latest migration'
PRINT '🔢 Product Version: ' + ISNULL(@ProductVersion, 'N/A')
PRINT ''

-- =====================================================================
-- STEP 4: Insert missing migration records
-- =====================================================================
PRINT '=== STEP 4: Inserting Missing Migration Records ==='
PRINT ''

-- Migration 1: AddBankingDetailsToLeaseAgreementMaster
IF NOT EXISTS (
    SELECT 1 FROM [dbo].[__MigrationHistory] 
    WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster'
)
BEGIN
    PRINT '📝 Inserting: 202603280900000_AddBankingDetailsToLeaseAgreementMaster'
    
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES (
        '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
        @ContextKey,
        @ModelBlob,
        @ProductVersion
    )
    
    PRINT '   ✅ Inserted'
END
ELSE
BEGIN
    PRINT '⏭️  Skipped: 202603280900000_AddBankingDetailsToLeaseAgreementMaster (already exists)'
END

PRINT ''

-- Migration 2: MakeHasDSTVNullable
IF NOT EXISTS (
    SELECT 1 FROM [dbo].[__MigrationHistory] 
    WHERE MigrationId = '202603280915000_MakeHasDSTVNullable'
)
BEGIN
    PRINT '📝 Inserting: 202603280915000_MakeHasDSTVNullable'
    
    INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
    VALUES (
        '202603280915000_MakeHasDSTVNullable',
        @ContextKey,
        @ModelBlob,
        @ProductVersion
    )
    
    PRINT '   ✅ Inserted'
END
ELSE
BEGIN
    PRINT '⏭️  Skipped: 202603280915000_MakeHasDSTVNullable (already exists)'
END

PRINT ''

-- =====================================================================
-- STEP 5: Verify the fix
-- =====================================================================
PRINT '=== STEP 5: Verification ==='
PRINT ''

PRINT 'Last 5 migrations in database:'
SELECT TOP 5
    MigrationId,
    ContextKey
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT ''

-- Check if our migrations are now present
IF EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster')
   AND EXISTS (SELECT 1 FROM [dbo].[__MigrationHistory] WHERE MigrationId = '202603280915000_MakeHasDSTVNullable')
BEGIN
    PRINT '╔══════════════════════════════════════════════════════════════╗'
    PRINT '║  ✅ SUCCESS! Migration History Synchronized                ║'
    PRINT '╚══════════════════════════════════════════════════════════════╝'
    PRINT ''
    PRINT '✅ Both missing migrations have been recorded'
    PRINT '✅ Database schema matches migration history'
    PRINT ''
    PRINT '📋 Next Steps:'
    PRINT '   1. Close this window'
    PRINT '   2. In Package Manager Console, run:'
    PRINT '      Update-Database -Verbose'
    PRINT ''
    PRINT '   Expected result: "No pending explicit migrations"'
    PRINT ''
END
ELSE
BEGIN
    PRINT '⚠️  WARNING: Verification incomplete'
    PRINT '   Please check __MigrationHistory table manually'
END

SET NOCOUNT OFF
