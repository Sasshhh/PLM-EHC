-- Check which banking and DSTV columns already exist in the database tables
USE [eServices]
GO

PRINT 'Checking PropertyLeaseApplications table...'
SELECT 'PropertyLeaseApplications' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseApplications' 
    AND COLUMN_NAME IN ('HasDSTV', 'DSTVActivationFee', 'DSTVMonthlyLevy', 'AccessCardDeposit', 'KeyDeposit')
ORDER BY COLUMN_NAME

PRINT 'Checking PropertyLeaseAgreementMasters table...'
SELECT 'PropertyLeaseAgreementMasters' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters' 
    AND COLUMN_NAME IN ('HasDSTV', 'DSTVActivationFee', 'DSTVMonthlyLevy', 'AccessCardDeposit', 'KeyDeposit', 'CommencementDay', 'TenantBankName', 'TenantAccountNumber', 'TenantAccountHolderName', 'TenantAccountType', 'TenantBranchCode')
ORDER BY COLUMN_NAME

PRINT 'Checking PropertyLeaseAgreementMasterAudits table...'
SELECT 'PropertyLeaseAgreementMasterAudits' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseAgreementMasterAudits' 
    AND COLUMN_NAME IN ('HasDSTV', 'DSTVActivationFee', 'DSTVMonthlyLevy', 'AccessCardDeposit', 'KeyDeposit', 'CommencementDay', 'TenantBankName', 'TenantAccountNumber', 'TenantAccountHolderName', 'TenantAccountType', 'TenantBranchCode')
ORDER BY COLUMN_NAME

PRINT 'Checking PropertyLeaseApplicationAudits table...'
SELECT 'PropertyLeaseApplicationAudits' as TableName, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PropertyLeaseApplicationAudits' 
    AND COLUMN_NAME IN ('HasDSTV', 'DSTVActivationFee', 'DSTVMonthlyLevy', 'AccessCardDeposit', 'KeyDeposit')
ORDER BY COLUMN_NAME