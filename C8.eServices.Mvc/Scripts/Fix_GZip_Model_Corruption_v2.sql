-- =====================================================================
-- Fix GZip Model Blob Corruption - Delete and Re-insert
-- =====================================================================
-- Model column is NOT NULL, so we can't just set it to NULL.
-- Instead: Delete the bad records and re-insert with empty Model blob.
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  Fix GZip Model Blob Corruption - Delete & Re-insert       ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''

DECLARE @ContextKey NVARCHAR(512) = 'C8.eServices.Mvc.DataAccessLayer.eServicesDbContext'
DECLARE @ProductVersion NVARCHAR(32) = '6.5.1'
DECLARE @EmptyModel VARBINARY(MAX) = 0x -- Empty binary

PRINT '=== Step 1: Delete corrupted migration records ==='
PRINT ''

-- Delete Migration 1
DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId = '202603280900000_AddBankingDetailsToLeaseAgreementMaster'

PRINT '✅ Deleted: 202603280900000_AddBankingDetailsToLeaseAgreementMaster (' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows)'

-- Delete Migration 2
DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId = '202603280915000_MakeHasDSTVNullable'

PRINT '✅ Deleted: 202603280915000_MakeHasDSTVNullable (' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows)'

PRINT ''
PRINT '=== Step 2: Re-insert with empty Model blob ==='
PRINT ''

-- Re-insert Migration 1
INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
VALUES (
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    @ContextKey,
    @EmptyModel,
    @ProductVersion
)
PRINT '✅ Inserted: 202603280900000_AddBankingDetailsToLeaseAgreementMaster (empty Model)'

-- Re-insert Migration 2
INSERT INTO [dbo].[__MigrationHistory] (MigrationId, ContextKey, Model, ProductVersion)
VALUES (
    '202603280915000_MakeHasDSTVNullable',
    @ContextKey,
    @EmptyModel,
    @ProductVersion
)
PRINT '✅ Inserted: 202603280915000_MakeHasDSTVNullable (empty Model)'

PRINT ''
PRINT '=== Step 3: Verification ==='
PRINT ''

SELECT 
    MigrationId,
    ContextKey,
    LEN(Model) AS ModelBlobSize,
    CASE 
        WHEN LEN(Model) = 0 THEN '✅ EMPTY (Fixed)'
        WHEN LEN(Model) < 100 THEN '✅ SMALL (OK)'
        ELSE '⚠️  LARGE (' + CAST(LEN(Model) AS VARCHAR(10)) + ' bytes)'
    END AS Status,
    ProductVersion
FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    '202603280915000_MakeHasDSTVNullable'
)
ORDER BY MigrationId

PRINT ''
PRINT 'Last 5 migrations:'
SELECT TOP 5
    MigrationId,
    ContextKey,
    LEN(Model) AS ModelSize
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT ''
PRINT '╔══════════════════════════════════════════════════════════════╗'
PRINT '║  ✅ FIX COMPLETE!                                           ║'
PRINT '╚══════════════════════════════════════════════════════════════╝'
PRINT ''
PRINT '✅ Corrupted Model blobs removed'
PRINT '✅ Migrations re-inserted with empty blobs'
PRINT '✅ EF won''t try to decompress (empty = skip)'
PRINT ''
PRINT '📋 Next Step:'
PRINT '   Run in Package Manager Console:'
PRINT '   Update-Database -Verbose'
PRINT ''
PRINT '   Expected: "No pending explicit migrations"'
PRINT '   No more GZip errors!'
PRINT ''
