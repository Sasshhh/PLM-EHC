-- =============================================
-- QUICK TEST SETUP - Tenant Training
-- Run this in SSMS to create/update a test record
-- =============================================

USE [CRMPLMDEV_2025]
GO

-- Find or update an existing application to the correct status
DECLARE @ApplicationId INT
DECLARE @StatusId INT
DECLARE @CustomerId INT
DECLARE @SystemUserId INT

-- Get the correct status
SELECT @StatusId = Id FROM Status WHERE [Key] = 's_assessment_fee_payment_approved'

-- Get the customer with username 'sash38'
SELECT TOP 1 
    @CustomerId = c.Id,
    @SystemUserId = c.SystemUserId
FROM Customers c
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
WHERE su.UserName = 'sash38'

IF @CustomerId IS NULL
BEGIN
    PRINT 'ERROR: Customer with username "sash38" not found!'
    PRINT 'Please check the SystemUsers table.'
    RETURN
END

PRINT 'Found Customer ID: ' + CAST(@CustomerId AS VARCHAR)
PRINT 'System User ID: ' + CAST(@SystemUserId AS VARCHAR)

-- Option 1: Find an existing application for this customer
SELECT TOP 1 @ApplicationId = Id
FROM PropertyLeaseApplications
WHERE CustomerId = @CustomerId
    AND IsDeleted = 0 
    AND IsActive = 1
ORDER BY CreatedDateTime DESC

-- If no application exists, create a new one
IF @ApplicationId IS NULL
BEGIN
    PRINT 'No existing application found. Creating new test application...'
    
    DECLARE @NewRefNumber VARCHAR(50)
    SET @NewRefNumber = 'TEST-TRAIN-' + CONVERT(VARCHAR, GETDATE(), 112) + '-' + 
                       RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR), 3)
    
    DECLARE @PurchaserTypeId INT
    SELECT @PurchaserTypeId = Id FROM PurchaserType WHERE Name = 'Individual'
    IF @PurchaserTypeId IS NULL SELECT TOP 1 @PurchaserTypeId = Id FROM PurchaserType
    
    INSERT INTO PropertyLeaseApplications (
        ApplicationReferenceNumber,
        StatusId,
        CustomerId,
        SystemUserId,
        PurchaserTypeId,
        FirstName,
        LastName,
        IDNo,
        CellNo,
        PurEmail,
        Gender,
        MaritalStatus,
        GrossIncome,
        NetIncome,
        TotalCombinedIncome,
        HouseRequired,
        PrefArea,
        IsActive,
        IsDeleted,
        IsLocked,
        CreatedDateTime,
        ModifiedDateTime
    )
    VALUES (
        @NewRefNumber,
        @StatusId,
        @CustomerId,
        @SystemUserId,
        @PurchaserTypeId,
        'John',
        'TestTraining',
        '8001015009088',
        '0821234567',
        'testtraining@ehc.co.za',
        'Male',
        'Single',
        15000.00,
        12000.00,
        15000.00,
        '2 Bedroom',
        'Kempton Park',
        1, -- IsActive
        0, -- IsDeleted
        0, -- IsLocked
        GETDATE(),
        GETDATE()
    )
    
    SET @ApplicationId = SCOPE_IDENTITY()
    PRINT 'Created new application ID: ' + CAST(@ApplicationId AS VARCHAR)
END
ELSE
BEGIN
    PRINT 'Found existing application ID: ' + CAST(@ApplicationId AS VARCHAR)
END

-- Update to correct status
UPDATE PropertyLeaseApplications
SET StatusId = @StatusId,
    PurEmail = 'testtraining@ehc.co.za', -- Update email for testing
    ModifiedDateTime = GETDATE()
WHERE Id = @ApplicationId

-- Clean up any existing training records
DELETE FROM TenantExamAnswers WHERE PropertyLeaseApplicationId = @ApplicationId
DELETE FROM TenantTrainings WHERE PropertyLeaseApplicationId = @ApplicationId

PRINT ''
PRINT '? Cleanup complete - removed old training records'

-- Display result
SELECT 
    '? TEST APPLICATION READY!' AS Status,
    Id AS ApplicationId,
    ApplicationReferenceNumber AS RefNumber,
    FirstName + ' ' + LastName AS Applicant,
    PurEmail AS Email,
    CustomerId,
    SystemUserId,
    (SELECT Name FROM Status WHERE Id = StatusId) AS CurrentStatus
FROM PropertyLeaseApplications
WHERE Id = @ApplicationId

PRINT ''
PRINT '???????????????????????????????????????????????????????????'
PRINT '?? TESTING STEPS:'
PRINT '???????????????????????????????????????????????????????????'
PRINT ''
PRINT '1. Navigate to: /TenantTraining/Index'
PRINT '   (Logged in as user: sash38)'
PRINT ''
PRINT '2. Click "Invite to Training" for this application'
PRINT '   Application ID: ' + CAST(@ApplicationId AS VARCHAR)
PRINT ''
PRINT '3. System will create training record and send email'
PRINT ''
PRINT '4. Run this query to get the training link:'
PRINT '   SELECT InvitationToken FROM TenantTrainings'
PRINT '   WHERE PropertyLeaseApplicationId = ' + CAST(@ApplicationId AS VARCHAR)
PRINT ''
PRINT '5. Navigate to: /TenantTraining/StartTraining?token=YOUR_TOKEN'
PRINT ''
PRINT '???????????????????????????????????????????????????????????'

GO
