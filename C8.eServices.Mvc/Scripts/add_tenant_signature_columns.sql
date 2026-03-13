IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PropertyLeaseAgreementMasters' AND COLUMN_NAME='TenantSignature')
    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantSignature NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PropertyLeaseAgreementMasters' AND COLUMN_NAME='TenantSigned')
    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantSigned BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='PropertyLeaseAgreementMasters' AND COLUMN_NAME='TenantSignDate')
    ALTER TABLE dbo.PropertyLeaseAgreementMasters ADD TenantSignDate NVARCHAR(50) NULL;

-- Also register this migration in EF's history table so it stays in sync
IF NOT EXISTS (SELECT 1 FROM dbo.__MigrationHistory WHERE MigrationId = '202603031200000_AddTenantSignatureToLeaseAgreementMaster')
    INSERT INTO dbo.__MigrationHistory (MigrationId, ContextKey, Model, ProductVersion)
    SELECT TOP 1 
        '202603031200000_AddTenantSignatureToLeaseAgreementMaster',
        ContextKey,
        Model,
        ProductVersion
    FROM dbo.__MigrationHistory
    ORDER BY MigrationId DESC;

-- Verify
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PropertyLeaseAgreementMasters'
  AND COLUMN_NAME IN ('TenantSignature', 'TenantSigned', 'TenantSignDate');
