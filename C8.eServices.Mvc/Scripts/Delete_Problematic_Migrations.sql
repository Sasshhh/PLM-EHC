-- =====================================================================
-- SIMPLE FIX - Just Delete The Problematic Migrations
-- =====================================================================
-- Since you're the only developer and columns exist, just remove them.
-- EF won't try to reference them anymore.
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '=== Deleting problematic migrations ==='

DELETE FROM [dbo].[__MigrationHistory]
WHERE MigrationId IN (
    '202603280900000_AddBankingDetailsToLeaseAgreementMaster',
    '202603280915000_MakeHasDSTVNullable'
)

PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' migration(s)'
PRINT ''
PRINT '=== Verification ==='

SELECT TOP 5 MigrationId, ContextKey
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT ''
PRINT '✅ DONE! Try Update-Database now.'
