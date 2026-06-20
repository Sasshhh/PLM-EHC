-- Banking Details Migration for PropertyLeaseAgreementMaster
-- Migration: 202603280900000_AddBankingDetailsToLeaseAgreementMaster
-- Date: March 28, 2026

USE [eServices]
GO

-- Add banking fields to PropertyLeaseAgreementMasters table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' AND COLUMN_NAME = 'TenantBankName')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] ADD [TenantBankName] NVARCHAR(100) NULL;
    PRINT 'Added TenantBankName column to PropertyLeaseAgreementMasters';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' AND COLUMN_NAME = 'TenantAccountNumber')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] ADD [TenantAccountNumber] NVARCHAR(20) NULL;
    PRINT 'Added TenantAccountNumber column to PropertyLeaseAgreementMasters';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' AND COLUMN_NAME = 'TenantAccountHolderName')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] ADD [TenantAccountHolderName] NVARCHAR(200) NULL;
    PRINT 'Added TenantAccountHolderName column to PropertyLeaseAgreementMasters';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' AND COLUMN_NAME = 'TenantAccountType')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] ADD [TenantAccountType] NVARCHAR(50) NULL;
    PRINT 'Added TenantAccountType column to PropertyLeaseAgreementMasters';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' AND COLUMN_NAME = 'TenantBranchCode')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasters] ADD [TenantBranchCode] NVARCHAR(10) NULL;
    PRINT 'Added TenantBranchCode column to PropertyLeaseAgreementMasters';
END

-- Add banking fields to PropertyLeaseAgreementMasterAudits table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' AND COLUMN_NAME = 'TenantBankName')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasterAudits] ADD [TenantBankName] NVARCHAR(100) NULL;
    PRINT 'Added TenantBankName column to PropertyLeaseAgreementMasterAudits';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' AND COLUMN_NAME = 'TenantAccountNumber')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasterAudits] ADD [TenantAccountNumber] NVARCHAR(20) NULL;
    PRINT 'Added TenantAccountNumber column to PropertyLeaseAgreementMasterAudits';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' AND COLUMN_NAME = 'TenantAccountHolderName')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasterAudits] ADD [TenantAccountHolderName] NVARCHAR(200) NULL;
    PRINT 'Added TenantAccountHolderName column to PropertyLeaseAgreementMasterAudits';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' AND COLUMN_NAME = 'TenantAccountType')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasterAudits] ADD [TenantAccountType] NVARCHAR(50) NULL;
    PRINT 'Added TenantAccountType column to PropertyLeaseAgreementMasterAudits';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' AND COLUMN_NAME = 'TenantBranchCode')
BEGIN
    ALTER TABLE [dbo].[PropertyLeaseAgreementMasterAudits] ADD [TenantBranchCode] NVARCHAR(10) NULL;
    PRINT 'Added TenantBranchCode column to PropertyLeaseAgreementMasterAudits';
END

-- Verify the columns were added
SELECT 
    'PropertyLeaseAgreementMasters' as TableName,
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' 
    AND COLUMN_NAME IN ('TenantBankName', 'TenantAccountNumber', 'TenantAccountHolderName', 'TenantAccountType', 'TenantBranchCode')

UNION ALL

SELECT 
    'PropertyLeaseAgreementMasterAudits' as TableName,
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' 
    AND COLUMN_NAME IN ('TenantBankName', 'TenantAccountNumber', 'TenantAccountHolderName', 'TenantAccountType', 'TenantBranchCode')
ORDER BY TableName, COLUMN_NAME;

GO

PRINT 'Banking Details Migration Completed Successfully!';