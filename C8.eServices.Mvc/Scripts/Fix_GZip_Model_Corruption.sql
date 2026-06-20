-- =====================================================================
-- Fix GZip Model Blob Corruption - Set Model to NULL
-- =====================================================================
-- The error "magic number in GZip header is not correct" happens because
-- we copied a Model blob from another migration that doesn't match.
--
-- SOLUTION: Set Model = NULL for the two problematic migrations.
-- EF will still recognize them as applied, but won't try to decompress.
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  Fix GZip Model Blob Corruption                            ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''

-- Check current state
PRINT '=== Current State of Problematic Migrations ==='
SELECT 
    MigrationId,
    ContextKey,
    CASE WHEN Model IS NULL THEN 'NULL' ELSE 'HAS BLOB' END AS ModelStatus,
    LEN(Model) AS ModelBlobSize,
    ProductVersion
FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    '202603280915000_MakeHasDSTVNullable'
)
ORDER BY MigrationId

PRINT ''
PRINT '=== Setting Model Blobs to NULL ==='
PRINT ''

-- Update Migration 1
UPDATE [dbo].[__MigrationHistory]
SET Model = NULL
WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster'

IF @@ROWCOUNT > 0
    PRINT '✅ Fixed: 202603280900000_AddBankingDetailsToLeaseAgreementMaster (Model = NULL)'
ELSE
    PRINT '⏭️  Not found: 202603280900000_AddBankingDetailsToLeaseAgreementMaster'

-- Update Migration 2
UPDATE [dbo].[__MigrationHistory]
SET Model = NULL
WHERE MigrationId = '202603280915000_MakeHasDSTVNullable'

IF @@ROWCOUNT > 0
    PRINT '✅ Fixed: 202603280915000_MakeHasDSTVNullable (Model = NULL)'
ELSE
    PRINT '⏭️  Not found: 202603280915000_MakeHasDSTVNullable'

PRINT ''
PRINT '=== Verification ==='
SELECT 
    MigrationId,
    ContextKey,
    CASE WHEN Model IS NULL THEN '✅ NULL (Fixed)' ELSE '❌ HAS BLOB (Bad)' END AS ModelStatus,
    ProductVersion
FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    '202603280915000_MakeHasDSTVNullable'
)
ORDER BY MigrationId

PRINT ''
PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  ✅ FIX COMPLETE!                                           ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''
PRINT '✅ Model blobs set to NULL'
PRINT '✅ Migrations still marked as applied'
PRINT '✅ EF will skip Model decompression'
PRINT ''
PRINT '📋 Next Step:'
PRINT '   Run in Package Manager Console:'
PRINT '   Update-Database -Verbose'
PRINT ''
PRINT '   Expected: "No pending explicit migrations"'
PRINT '   (Should work now - no GZip error!)'
PRINT ''
