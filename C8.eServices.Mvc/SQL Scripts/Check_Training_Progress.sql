-- ========================================
-- CHECK CURRENT TRAINING STATUS
-- For Application: EHC2023101800005
-- ========================================

USE [eServicesDbContext] -- Change to your database name
GO

DECLARE @AppRefNum NVARCHAR(50) = 'EHC2023101800005';

-- Get current training state
SELECT 
    'Current Training Status' AS Info,
    tt.Id AS TrainingId,
    pla.ApplicationReferenceNumber AS AppRef,
    tt.CurrentSlideNumber AS CurrentSlide,
    tt.TrainingStartedDate AS Started,
    tt.TrainingCompletedDate AS Completed,
    tt.IsTrainingCompleted AS TrainingComplete,
    tt.IsExamPassed AS ExamPassed,
    tt.ExamAttempts AS Attempts,
    tt.ExamScore AS Score,
    s.Name AS Status,
    s.[Key] AS StatusKey
FROM TenantTrainings tt
INNER JOIN PropertyLeaseApplications pla ON tt.PropertyLeaseApplicationId = pla.Id
INNER JOIN Status s ON pla.StatusId = s.Id
WHERE pla.ApplicationReferenceNumber = @AppRefNum
    AND tt.IsDeleted = 0;

-- Count total slides
SELECT 
    'Total Slides Available' AS Info,
    COUNT(*) AS TotalSlides
FROM TrainingSlides
WHERE IsActive = 1 AND IsDeleted = 0;

PRINT '? Check that CurrentSlideNumber matches your progress';
PRINT '? Total slides should be 24';
