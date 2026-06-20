-- =====================================================================
-- Check Migration History Status
-- =====================================================================
-- This checks what migrations have been applied to the database
-- =====================================================================

USE [CRMPLMDEV_2025]
GO

PRINT '=== Last 20 Migrations Applied ==='
PRINT ''

SELECT TOP 20
    MigrationId,
    ContextKey,
    ProductVersion
FROM [dbo].[__MigrationHistory]
ORDER BY MigrationId DESC

PRINT ''
PRINT '=== Checking for HasDSTV-related migrations ==='
PRINT ''

SELECT 
    MigrationId,
    ContextKey
FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '%202603280915000%'  -- MakeHasDSTVNullable
   OR MigrationId LIKE '%202603270810000%'  -- AddDSTVAndDepositFields
   OR MigrationId LIKE '%nullablefields%'
ORDER BY MigrationId

PRINT ''
PRINT '=== Checking for Banking Details migrations ==='
PRINT ''

SELECT 
    MigrationId,
    ContextKey
FROM [dbo].[__MigrationHistory]
WHERE MigrationId LIKE '%202603260709130%'  -- AddBankingDetailsOnly
   OR MigrationId LIKE '%202603280900000%'  -- AddBankingDetailsToLeaseAgreementMaster
ORDER BY MigrationId

PRINT ''
PRINT '=== Total Migration Count ==='
SELECT COUNT(*) AS TotalMigrations FROM [dbo].[__MigrationHistory]
