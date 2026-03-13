-- Reset manager signatures for application EHC2026020600001 to test workflow
DECLARE @AppId INT;

SELECT @AppId = Id 
FROM PropertyLeaseApplications 
WHERE ApplicationReferenceNumber = 'EHC2026020600001';

PRINT 'Application ID: ' + CAST(@AppId AS VARCHAR);

-- Show current state
SELECT 
    'BEFORE RESET' AS Status,
    PropertyManagerSigned,
    PropertyManagerSignatureDate,
    RevenueManagerSigned,
    RevenueManagerSignatureDate
FROM PropertyLeaseAgreementMasters
WHERE PropertyLeaseApplicationId = @AppId;

-- Reset manager signatures
UPDATE PropertyLeaseAgreementMasters
SET 
    PropertyManagersSignature = NULL,
    PropertyManagerSigned = 0,
    PropertyManagerSignatureDate = NULL,
    PropertyManagerId = NULL,
    RevenueManagersSignature = NULL,
    RevenueManagerSigned = 0,
    RevenueManagerSignatureDate = NULL,
    RevenueManagerId = NULL
WHERE PropertyLeaseApplicationId = @AppId;

-- Show after state
SELECT 
    'AFTER RESET' AS Status,
    PropertyManagerSigned,
    PropertyManagerSignatureDate,
    RevenueManagerSigned,
    RevenueManagerSignatureDate
FROM PropertyLeaseAgreementMasters
WHERE PropertyLeaseApplicationId = @AppId;

PRINT 'Manager signatures reset successfully. Ready for testing.';
