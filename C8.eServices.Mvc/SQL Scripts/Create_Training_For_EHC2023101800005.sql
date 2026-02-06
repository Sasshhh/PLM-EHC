-- ========================================
-- CREATE TENANT TRAINING RECORD
-- For Application: EHC2023101800005
-- ========================================
-- This script creates a TenantTraining record so the customer can access training immediately
-- Run this in SQL Server Management Studio or your preferred SQL tool

USE [YourDatabaseName] -- CHANGE THIS TO YOUR DATABASE NAME!
GO

DECLARE @ApplicationRefNum NVARCHAR(50) = 'EHC2023101800005';
DECLARE @ApplicationId INT;
DECLARE @CustomerId INT;
DECLARE @StatusId INT;
DECLARE @Token NVARCHAR(450);
DECLARE @ExpiryDate DATETIME;

-- ========================================
-- STEP 1: Get Application Details
-- ========================================
SELECT 
    @ApplicationId = Id,
    @CustomerId = CustomerId,
    @StatusId = StatusId
FROM PropertyLeaseApplications
WHERE ApplicationReferenceNumber = @ApplicationRefNum
    AND IsDeleted = 0;

-- Check if application exists
IF @ApplicationId IS NULL
BEGIN
    PRINT '? ERROR: Application not found with reference number: ' + @ApplicationRefNum;
    PRINT 'Please verify the application reference number and try again.';
    RETURN;
END

PRINT '? Application found:';
PRINT '   Application ID: ' + CAST(@ApplicationId AS NVARCHAR(10));
PRINT '   Customer ID: ' + CAST(@CustomerId AS NVARCHAR(10));
PRINT '';

-- ========================================
-- STEP 2: Check if Training Record Already Exists
-- ========================================
IF EXISTS (
    SELECT 1 
    FROM TenantTrainings 
    WHERE PropertyLeaseApplicationId = @ApplicationId 
        AND IsDeleted = 0
)
BEGIN
    PRINT '??  WARNING: Training record already exists for this application!';
    PRINT '   Skipping TenantTraining creation.';
    PRINT '';
    
    -- Show existing training details
    SELECT 
        'Existing Training Record' AS [Status],
        Id AS TrainingId,
        InvitationToken AS Token,
        TokenExpiryDate AS ExpiryDate,
        InvitationSentDate AS SentDate,
        TrainingStartedDate AS StartedDate,
        IsTrainingCompleted AS Completed,
        IsExamPassed AS ExamPassed,
        ExamAttempts AS Attempts,
        ExamScore AS Score
    FROM TenantTrainings
    WHERE PropertyLeaseApplicationId = @ApplicationId 
        AND IsDeleted = 0;
END
ELSE
BEGIN
    -- ========================================
    -- STEP 3: Create TenantTraining Record
    -- ========================================
    PRINT '?? Creating TenantTraining record...';
    
    -- Generate unique token
    SET @Token = LOWER(NEWID());
    SET @ExpiryDate = DATEADD(DAY, 30, GETDATE()); -- 30 days from now
    
    INSERT INTO TenantTrainings (
        PropertyLeaseApplicationId,
        InvitationToken,
        TokenExpiryDate,
        InvitationSentDate,
        TrainingStartedDate,
        TrainingCompletedDate,
        ExamPassedDate,
        CurrentSlideNumber,
        IsTrainingCompleted,
        IsExamPassed,
        ExamAttempts,
        ExamScore,
        CreatedDateTime,
        ModifiedDateTime,
        IsActive,
        IsDeleted,
        IsLocked
    )
    VALUES (
        @ApplicationId,                 -- PropertyLeaseApplicationId
        @Token,                         -- InvitationToken (unique GUID)
        @ExpiryDate,                    -- TokenExpiryDate (30 days)
        GETDATE(),                      -- InvitationSentDate
        NULL,                           -- TrainingStartedDate (NULL until started)
        NULL,                           -- TrainingCompletedDate (NULL until completed)
        NULL,                           -- ExamPassedDate (NULL until passed)
        0,                              -- CurrentSlideNumber (starting at 0)
        0,                              -- IsTrainingCompleted (FALSE)
        0,                              -- IsExamPassed (FALSE)
        0,                              -- ExamAttempts (0 attempts)
        NULL,                           -- ExamScore (NULL until attempted)
        GETDATE(),                      -- CreatedDateTime
        GETDATE(),                      -- ModifiedDateTime
        1,                              -- IsActive (TRUE)
        0,                              -- IsDeleted (FALSE)
        0                               -- IsLocked (FALSE)
    );
    
    PRINT '? TenantTraining record created successfully!';
    PRINT '';
END

-- ========================================
-- STEP 4: Get Status Key
-- ========================================
DECLARE @CurrentStatusKey NVARCHAR(100);

SELECT @CurrentStatusKey = s.[Key]
FROM Status s
WHERE s.Id = @StatusId;

PRINT '?? Current Application Status: ' + ISNULL(@CurrentStatusKey, 'UNKNOWN');
PRINT '';

-- ========================================
-- STEP 5: Update Application Status (if needed)
-- ========================================
DECLARE @AwaitingOnlineTrainingStatusId INT;

SELECT @AwaitingOnlineTrainingStatusId = Id
FROM Status
WHERE [Key] = 's_awaiting_online_training';

IF @AwaitingOnlineTrainingStatusId IS NULL
BEGIN
    PRINT '??  WARNING: Status "s_awaiting_online_training" not found in database!';
    PRINT '   Please ensure the status keys have been added to the Status table.';
    PRINT '';
END
ELSE IF @CurrentStatusKey != 's_awaiting_online_training'
BEGIN
    PRINT '?? Updating application status to "Awaiting Online Training"...';
    
    UPDATE PropertyLeaseApplications
    SET StatusId = @AwaitingOnlineTrainingStatusId,
        ModifiedDateTime = GETDATE()
    WHERE Id = @ApplicationId;
    
    PRINT '? Application status updated successfully!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? Application status is already "Awaiting Online Training"';
    PRINT '';
END

-- ========================================
-- STEP 6: Display Final Results
-- ========================================
PRINT '========================================';
PRINT '?? TRAINING RECORD READY!';
PRINT '========================================';
PRINT '';

-- Show training details
SELECT 
    'Training Record Details' AS [Info],
    tt.Id AS TrainingId,
    pla.ApplicationReferenceNumber AS ApplicationRef,
    c.FirstName + ' ' + c.LastName AS CustomerName,
    c.EmailAddress AS Email,
    tt.InvitationToken AS Token,
    tt.TokenExpiryDate AS TokenExpiry,
    s.Name AS CurrentStatus,
    s.[Key] AS StatusKey
FROM TenantTrainings tt
INNER JOIN PropertyLeaseApplications pla ON tt.PropertyLeaseApplicationId = pla.Id
INNER JOIN Customers c ON pla.CustomerId = c.Id
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.ApplicationReferenceNumber = @ApplicationRefNum
    AND tt.IsDeleted = 0;

PRINT '';
PRINT '========================================';
PRINT '?? NEXT STEPS:';
PRINT '========================================';
PRINT '1. Customer can now login to the system';
PRINT '2. Click "My Training" from the left menu';
PRINT '3. Click "Start Training" button';
PRINT '4. Complete all training slides';
PRINT '5. Take the examination (15 questions, 100% required to pass)';
PRINT '';
PRINT '?? Direct Training Link:';
PRINT '   /TenantTraining/StartTraining?token=' + @Token;
PRINT '';
PRINT '? Script completed successfully!';

GO
