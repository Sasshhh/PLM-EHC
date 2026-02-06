-- ========================================
-- VERIFY TRAINING SLIDES COUNT
-- ========================================

USE [eServicesDbContext] -- Change to your database name
GO

-- Check total active slides
SELECT 
    COUNT(*) AS TotalActiveSlides,
    MIN(DisplayOrder) AS FirstSlide,
    MAX(DisplayOrder) AS LastSlide
FROM TrainingSlides
WHERE IsActive = 1 AND IsDeleted = 0;

-- List all slides
SELECT 
    Id,
    SlideNumber,
    Title,
    DisplayOrder,
    IsActive,
    IsDeleted
FROM TrainingSlides
ORDER BY DisplayOrder;

-- Check for gaps in DisplayOrder
SELECT 
    'Checking for gaps in DisplayOrder' AS CheckType,
    CASE 
        WHEN COUNT(DISTINCT DisplayOrder) = MAX(DisplayOrder) + 1 THEN 'No gaps found'
        ELSE 'WARNING: Gaps found in DisplayOrder!'
    END AS Result
FROM TrainingSlides
WHERE IsActive = 1 AND IsDeleted = 0;

PRINT '? If you see 24 total active slides, you are good to go!';
