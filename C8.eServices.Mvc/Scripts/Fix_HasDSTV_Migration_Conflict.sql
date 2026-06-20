-- =====================================================================
-- Fix HasDSTV Migration Conflict
-- =====================================================================
-- This script checks and adds missing HasDSTV columns before running
-- the nullable migration. Run this BEFORE Update-Database.
-- =====================================================================

USE [eServices_db]
GO

PRINT '=== Checking HasDSTV Column Status ==='
PRINT ''

-- Check PropertyLeaseApplications
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') 
    AND name = 'HasDSTV'
)
BEGIN
    PRINT '❌ PropertyLeaseApplications.HasDSTV - MISSING'
    PRINT '   Adding column...'
    ALTER TABLE dbo.PropertyLeaseApplications 
    ADD HasDSTV BIT NULL
    PRINT '   ✅ Added'
END
ELSE
BEGIN
    PRINT '✅ PropertyLeaseApplications.HasDSTV - EXISTS'
END
PRINT ''

-- Check PropertyLeaseApplicationAudits
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplicationAudits') 
    AND name = 'HasDSTV'
)
BEGIN
    PRINT '❌ PropertyLeaseApplicationAudits.HasDSTV - MISSING'
    PRINT '   Adding column...'
    ALTER TABLE dbo.PropertyLeaseApplicationAudits 
    ADD HasDSTV BIT NULL
    PRINT '   ✅ Added'
END
ELSE
BEGIN
    PRINT '✅ PropertyLeaseApplicationAudits.HasDSTV - EXISTS'
END
PRINT ''

-- Check PropertyLeaseAgreementMasters
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') 
    AND name = 'HasDSTV'
)
BEGIN
    PRINT '❌ PropertyLeaseAgreementMasters.HasDSTV - MISSING'
    PRINT '   Adding column...'
    ALTER TABLE dbo.PropertyLeaseAgreementMasters 
    ADD HasDSTV BIT NULL
    PRINT '   ✅ Added'
END
ELSE
BEGIN
    PRINT '✅ PropertyLeaseAgreementMasters.HasDSTV - EXISTS'
END
PRINT ''

-- Check PropertyLeaseAgreementMasterAudits
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') 
    AND name = 'HasDSTV'
)
BEGIN
    PRINT '❌ PropertyLeaseAgreementMasterAudits.HasDSTV - MISSING'
    PRINT '   Adding column...'
    ALTER TABLE dbo.PropertyLeaseAgreementMasterAudits 
    ADD HasDSTV BIT NULL
    PRINT '   ✅ Added'
END
ELSE
BEGIN
    PRINT '✅ PropertyLeaseAgreementMasterAudits.HasDSTV - EXISTS'
END
PRINT ''

PRINT '=== Summary ==='
SELECT 
    'PropertyLeaseApplications' AS TableName,
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplications') AND name = 'HasDSTV') 
         THEN 'EXISTS' ELSE 'MISSING' END AS HasDSTV_Column
UNION ALL
SELECT 
    'PropertyLeaseApplicationAudits',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseApplicationAudits') AND name = 'HasDSTV') 
         THEN 'EXISTS' ELSE 'MISSING' END
UNION ALL
SELECT 
    'PropertyLeaseAgreementMasters',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasters') AND name = 'HasDSTV') 
         THEN 'EXISTS' ELSE 'MISSING' END
UNION ALL
SELECT 
    'PropertyLeaseAgreementMasterAudits',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMasterAudits') AND name = 'HasDSTV') 
         THEN 'EXISTS' ELSE 'MISSING' END

PRINT ''
PRINT '=== Next Steps ==='
PRINT '1. Verify all columns show EXISTS above'
PRINT '2. In Visual Studio, set AutomaticMigrationsEnabled = false'
PRINT '3. Run: Update-Database'
PRINT ''
