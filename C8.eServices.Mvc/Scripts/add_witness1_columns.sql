-- Add Witness1 signature columns to PropertyLeaseAgreementMaster table
USE CRMPLMDEV_2025;
GO

-- Check if columns already exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PropertyLeaseAgreementMaster') AND name = 'Witness1Signature')
BEGIN
    PRINT 'Adding Witness1Signature columns...';
    
    ALTER TABLE [dbo].[PropertyLeaseAgreementMaster]
    ADD 
        [Witness1Signature] NVARCHAR(MAX) NULL,
        [Witness1Name] NVARCHAR(200) NULL,
        [Witness1SignatureDate] DATETIME NULL;
    
    PRINT '✅ Witness1 columns added successfully!';
END
ELSE
BEGIN
    PRINT '⚠️ Witness1Signature column already exists';
END
GO

-- Verify columns were added
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR) END AS DisplayLength,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS Nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMaster')
  AND c.name IN ('TenantSignature', 'Witness1Signature', 'Witness1Name', 'Witness1SignatureDate')
ORDER BY c.name;
GO
