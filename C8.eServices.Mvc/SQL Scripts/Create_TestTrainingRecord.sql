-- =============================================
-- TENANT TRAINING TEST DATA
-- Create or Update Test Application Record
-- For Customer: sash38
-- =============================================

USE [CRMPLMDEV_2025]
GO

-- =============================================
-- STEP 1: Find customer with username 'sash38'
-- =============================================

DECLARE @TestApplicationId INT
DECLARE @TestCustomerId INT
DECLARE @TestSystemUserId INT
DECLARE @AssessmentFeeApprovedStatusId INT
DECLARE @PurchaserTypeId INT

PRINT '????????????????????????????????????????????????????????????'
PRINT '   TENANT TRAINING SYSTEM - TEST DATA SETUP'
PRINT '   Customer: sash38'
PRINT '????????????????????????????????????????????????????????????'
PRINT ''

-- Get the customer with username 'sash38'
SELECT TOP 1 
    @TestCustomerId = c.Id,
    @TestSystemUserId = c.SystemUserId
FROM Customers c
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
WHERE su.UserName = 'sash38'

IF @TestCustomerId IS NULL
BEGIN
    PRINT '? ERROR: Customer with username "sash38" not found!'
    PRINT ''
    PRINT 'Available usernames:'
    SELECT TOP 10 
        su.UserName,
        c.Id AS CustomerId,
        c.FirstName + ' ' + c.LastName AS CustomerName
    FROM SystemUsers su
    INNER JOIN Customers c ON su.Id = c.SystemUserId
    WHERE c.IsActive = 1 AND c.IsDeleted = 0
    ORDER BY c.CreatedDateTime DESC
    RETURN
END

PRINT '? Found Customer:'
SELECT 
    c.Id AS CustomerId,
    su.UserName,
    c.FirstName + ' ' + c.LastName AS CustomerName,
    c.EmailAddress
FROM Customers c
INNER JOIN SystemUsers su ON c.SystemUserId = su.Id
WHERE c.Id = @TestCustomerId

PRINT ''

-- Get the status ID for "Assessment Fee Payment Approved"
SELECT @AssessmentFeeApprovedStatusId = Id 
FROM Status 
WHERE [Key] = 's_assessment_fee_payment_approved'

IF @AssessmentFeeApprovedStatusId IS NULL
BEGIN
    PRINT '? ERROR: Status key "s_assessment_fee_payment_approved" not found!'
    PRINT 'Please ensure status keys are set up correctly.'
END
ELSE
BEGIN
    PRINT '? Assessment Fee Approved Status ID: ' + CAST(@AssessmentFeeApprovedStatusId AS VARCHAR)
END

-- Get PurchaserType (Individual)
SELECT @PurchaserTypeId = Id 
FROM PurchaserType 
WHERE Name = 'Individual' OR [Key] = 'individual'
IF @PurchaserTypeId IS NULL
BEGIN
    SELECT TOP 1 @PurchaserTypeId = Id FROM PurchaserType
END

PRINT '? PurchaserType ID: ' + CAST(@PurchaserTypeId AS VARCHAR)
PRINT ''

-- =============================================
-- STEP 2: Find or Create Test Application
-- =============================================

-- Check if application already exists at correct status for this customer
SELECT TOP 1 @TestApplicationId = Id
FROM PropertyLeaseApplications
WHERE StatusId = @AssessmentFeeApprovedStatusId
    AND CustomerId = @TestCustomerId
    AND IsDeleted = 0
    AND IsActive = 1
ORDER BY CreatedDateTime DESC

IF @TestApplicationId IS NOT NULL
BEGIN
    PRINT '? Found existing application at correct status'
    PRINT '  Application ID: ' + CAST(@TestApplicationId AS VARCHAR)
END
ELSE
BEGIN
    -- Try to find any application for this customer and update it
    SELECT TOP 1 @TestApplicationId = Id
    FROM PropertyLeaseApplications
    WHERE IsDeleted = 0
        AND IsActive = 1
        AND CustomerId = @TestCustomerId
    ORDER BY CreatedDateTime DESC

    IF @TestApplicationId IS NOT NULL
    BEGIN
        PRINT '? Updating existing application to correct status...'
        
        UPDATE PropertyLeaseApplications
        SET StatusId = @AssessmentFeeApprovedStatusId,
            PurEmail = 'testtraining@ehc.co.za',
            ModifiedDateTime = GETDATE()
        WHERE Id = @TestApplicationId
        
        PRINT '? APPLICATION UPDATED!'
        PRINT '  Application ID: ' + CAST(@TestApplicationId AS VARCHAR)
    END
    ELSE
    BEGIN
        -- Create a new test application
        PRINT '? Creating new test application for sash38...'
        
        DECLARE @NewRefNumber VARCHAR(50)
        SET @NewRefNumber = 'TEST-TRAIN-' + CONVERT(VARCHAR, GETDATE(), 112) + '-' + 
                           RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR), 3)
        
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
            @AssessmentFeeApprovedStatusId,
            @TestCustomerId,
            @TestSystemUserId,
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
        
        SET @TestApplicationId = SCOPE_IDENTITY()
        
        PRINT '? NEW APPLICATION CREATED!'
        PRINT '  Application ID: ' + CAST(@TestApplicationId AS VARCHAR)
        PRINT '  Reference Number: ' + @NewRefNumber
    END
END

PRINT ''

-- Display application details
SELECT 
    'APPLICATION DETAILS' AS [Info],
    Id AS ApplicationId,
    ApplicationReferenceNumber AS RefNumber,
    FirstName + ' ' + LastName AS ApplicantName,
    PurEmail AS Email,
    CustomerId,
    SystemUserId,
    s.Name AS CurrentStatus
FROM PropertyLeaseApplications
CROSS APPLY (SELECT Name FROM Status WHERE Id = StatusId) s
WHERE Id = @TestApplicationId

-- =============================================
-- STEP 3: Clean up any existing training records
-- =============================================

PRINT ''
PRINT '? Cleaning up existing training records...'



DELETE FROM TenantExamAnswers 
WHERE PropertyLeaseApplicationId = @TestApplicationId

DELETE FROM TenantTrainings 
WHERE PropertyLeaseApplicationId = @TestApplicationId

PRINT '? Cleaned up existing training records'
PRINT ''

-- =============================================
-- STEP 4: Display Testing Instructions
-- =============================================

PRINT '???????????????????????????????????????????????????????????'
PRINT '        TENANT TRAINING SYSTEM - READY TO TEST!'
PRINT '???????????????????????????????????????????????????????????'
PRINT ''
PRINT '? Setup Complete!'
PRINT ''
PRINT '?? TESTING STEPS:'
PRINT ''
PRINT '1. Log in as: sash38'
PRINT ''
PRINT '2. Navigate to: /TenantTraining/Index'
PRINT ''
PRINT '3. Find the application in the table:'
PRINT '   Reference: ' + (SELECT ApplicationReferenceNumber FROM PropertyLeaseApplications WHERE Id = @TestApplicationId)
PRINT '   Applicant: ' + (SELECT FirstName + ' ' + LastName FROM PropertyLeaseApplications WHERE Id = @TestApplicationId)
PRINT '   Email: ' + (SELECT PurEmail FROM PropertyLeaseApplications WHERE Id = @TestApplicationId)
PRINT ''
PRINT '4. Click "Invite to Training" button'
PRINT ''
PRINT '5. After invitation sent, run this query to get token:'
PRINT '   SELECT InvitationToken FROM TenantTrainings'
PRINT '   WHERE PropertyLeaseApplicationId = ' + CAST(@TestApplicationId AS VARCHAR)
PRINT ''
PRINT '6. Navigate to: /TenantTraining/StartTraining?token=YOUR_TOKEN'
PRINT ''
PRINT '7. Complete training and take exam'
PRINT ''
PRINT '???????????????????????????????????????????????????????????'
PRINT ''

-- =============================================
-- STEP 5: Monitoring Queries
-- =============================================

PRINT '?? MONITORING QUERIES:'
PRINT ''
PRINT '-- Check training progress:'
PRINT 'SELECT * FROM TenantTrainings'
PRINT 'WHERE PropertyLeaseApplicationId = ' + CAST(@TestApplicationId AS VARCHAR)
PRINT ''
PRINT '-- Check exam answers:'
PRINT 'SELECT ea.*, q.QuestionText, q.CorrectAnswer'
PRINT 'FROM TenantExamAnswers ea'
PRINT 'INNER JOIN ExaminationQuestions q ON ea.ExaminationQuestionId = q.Id'
PRINT 'WHERE ea.PropertyLeaseApplicationId = ' + CAST(@TestApplicationId AS VARCHAR)
PRINT 'ORDER BY ea.AttemptNumber, q.QuestionOrder'
PRINT ''
PRINT '-- Check status changes:'
PRINT 'SELECT a.ApplicationReferenceNumber, s.Name AS Status, a.ModifiedDateTime'
PRINT 'FROM PropertyLeaseApplications a'
PRINT 'INNER JOIN Status s ON a.StatusId = s.Id'
PRINT 'WHERE a.Id = ' + CAST(@TestApplicationId AS VARCHAR)
PRINT ''

-- =============================================
-- EXAM ANSWER KEY
-- =============================================

PRINT ''
PRINT '?? EXAM ANSWER KEY (100% Required to Pass):'
PRINT '??????????????????????????????????????????'
PRINT ''

SELECT 
    QuestionOrder AS [Q#],
    LEFT(QuestionText, 60) + '...' AS Question,
    CorrectAnswer AS [Ans]
FROM ExaminationQuestions
WHERE IsExampleQuestion = 0
    AND IsActive = 1
    AND IsDeleted = 0
ORDER BY QuestionOrder

PRINT ''
PRINT '? TEST DATA SETUP COMPLETE!'
PRINT 'Application ID: ' + CAST(@TestApplicationId AS VARCHAR)
PRINT ''

GO
