USE [CRMPLMDEV_2025];
GO

DECLARE @AppRef NVARCHAR(MAX) = 'EHC2026032600001';
DECLARE @AppId INT;
DECLARE @LeaseGeneratedStatusId INT;

SELECT @AppId = Id FROM PropertyLeaseApplications WHERE ApplicationReferenceNumber = @AppRef;
SELECT @LeaseGeneratedStatusId = Id FROM Status WHERE [Key] = 'a_lease_generated';

IF @AppId IS NOT NULL
BEGIN
    -- 1. Reset Application Status to 'LeaseAgreementGenerated'
    UPDATE PropertyLeaseApplications
    SET StatusId = @LeaseGeneratedStatusId
    WHERE Id = @AppId;

    PRINT 'Application ' + @AppRef + ' status reset to LeaseAgreementGenerated.';

    -- 2. Delete all Round Robin Queue entries for this application 
    -- relating to the subsequent steps (so they don't linger in RM/CEO inboxes)
    DELETE FROM RoundRobinQueues
    WHERE PropertyLeaseApplicationId = @AppId;

    PRINT 'Deleted active RoundRobinQueue entries for the application.';

    -- 3. Clear any existing signatures in PropertyLeaseAgreementMaster
    UPDATE PropertyLeaseAgreementMasters
    SET PropertyManagerSigned = 0,
        RevenueManagerSigned = 0
    WHERE PropertyLeaseApplicationId = @AppId;

    PRINT 'Cleared PropertyManagerSigned and RevenueManagerSigned flags.';
END
ELSE
BEGIN
    PRINT 'Application ' + @AppRef + ' not found.';
END
GO
