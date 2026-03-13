-- Check signature status for application EHC2026020600001
DECLARE @AppId INT;

SELECT @AppId = Id 
FROM PropertyLeaseApplications 
WHERE ApplicationReferenceNumber = 'EHC2026020600001';

PRINT 'Application ID: ' + CAST(@AppId AS VARCHAR);

-- Check PropertyLeaseAgreementMaster signature fields
SELECT 
    Id,
    PropertyLeaseApplicationId,
    PropertyManagerSigned,
    PropertyManagerSignatureDate,
    PropertyManagerId,
    RevenueManagerSigned,
    RevenueManagerSignatureDate,
    RevenueManagerId,
    TenantSigned,
    MainLesseeSigned,
    CASE 
        WHEN PropertyManagersSignature IS NULL THEN 'NULL'
        WHEN LEN(PropertyManagersSignature) > 50 THEN 'HAS DATA (' + CAST(LEN(PropertyManagersSignature) AS VARCHAR) + ' chars)'
        ELSE PropertyManagersSignature
    END AS PropertyManagersSignature_Status,
    CASE 
        WHEN RevenueManagersSignature IS NULL THEN 'NULL'
        WHEN LEN(RevenueManagersSignature) > 50 THEN 'HAS DATA (' + CAST(LEN(RevenueManagersSignature) AS VARCHAR) + ' chars)'
        ELSE RevenueManagersSignature
    END AS RevenueManagersSignature_Status,
    CASE 
        WHEN TenantSignature IS NULL THEN 'NULL'
        WHEN LEN(TenantSignature) > 50 THEN 'HAS DATA (' + CAST(LEN(TenantSignature) AS VARCHAR) + ' chars)'
        ELSE TenantSignature
    END AS TenantSignature_Status,
    CASE 
        WHEN Witness1Signature IS NULL THEN 'NULL'
        WHEN LEN(Witness1Signature) > 50 THEN 'HAS DATA (' + CAST(LEN(Witness1Signature) AS VARCHAR) + ' chars)'
        ELSE Witness1Signature
    END AS Witness1Signature_Status,
    Witness1Name,
    Witness1SignatureDate
FROM PropertyLeaseAgreementMasters
WHERE PropertyLeaseApplicationId = @AppId
ORDER BY Id DESC;
