-- ========================================
-- VERIFY TRAINING SETUP
-- For Application: EHC2023101800005
-- ========================================

USE [eServicesDbContext] -- Change to your database name
GO

-- Check Training Record
SELECT 
    'Training Record' AS Type,
    tt.Id AS TrainingId,
    tt.PropertyLeaseApplicationId AS AppId,
    tt.InvitationToken AS Token,
    tt.TokenExpiryDate AS Expiry,
    tt.IsTrainingCompleted AS Completed,
    tt.IsExamPassed AS Passed,
    tt.ExamAttempts AS Attempts
FROM TenantTrainings tt
WHERE tt.PropertyLeaseApplicationId = 1095
    AND tt.IsDeleted = 0;

-- Check Application Status
SELECT 
    'Application Status' AS Type,
    pla.Id AS AppId,
    pla.ApplicationReferenceNumber AS RefNum,
    pla.CustomerId AS CustomerId,
    s.Name AS StatusName,
    s.[Key] AS StatusKey
FROM PropertyLeaseApplications pla
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.Id = 1095;

-- Check Customer Details
SELECT 
    'Customer Details' AS Type,
    c.Id AS CustomerId,
    c.FirstName,
    c.LastName,
    c.EmailAddress,
    su.UserName AS Username
FROM Customers c
INNER JOIN AspNetUsers su ON c.SystemUserId = su.Id
WHERE c.Id = 45;

PRINT '? Verification complete!';
PRINT '';
PRINT '?? Direct Link (use this in your browser):';
PRINT 'http://localhost:YOURPORT/TenantTraining/StartTraining?token=203878b4-42e2-4643-a3da-3c0f71f7908b';
