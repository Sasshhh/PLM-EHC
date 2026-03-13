-- Check if Witness1Signature columns exist in PropertyLeaseAgreementMaster table
USE CRMPLMDEV_2025;
GO

-- Check column existence
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMaster')
  AND c.name IN ('Witness1Signature', 'Witness1Name', 'Witness1SignatureDate')
ORDER BY c.name;

-- If columns don't exist, this will show all columns for reference
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMaster') 
      AND name = 'Witness1Signature'
)
BEGIN
    PRINT '❌ Witness1Signature column NOT FOUND';
    PRINT 'All columns in PropertyLeaseAgreementMaster:';
    
    SELECT 
        c.column_id,
        c.name AS ColumnName,
        t.name AS DataType,
        c.max_length AS MaxLength,
        c.is_nullable AS IsNullable
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.PropertyLeaseAgreementMaster')
    ORDER BY c.column_id;
END
ELSE
BEGIN
    PRINT '✅ Witness1Signature columns FOUND';
    
    -- Show sample data
    SELECT TOP 5
        Id,
        PropertyLeaseApplicationId,
        TenantSignature,
        Witness1Signature,
        Witness1Name,
        Witness1SignatureDate
    FROM PropertyLeaseAgreementMaster
    ORDER BY Id DESC;
END
