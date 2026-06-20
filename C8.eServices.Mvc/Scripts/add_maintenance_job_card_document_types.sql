-- =============================================
-- ADD DOCUMENT TYPES FOR MAINTENANCE JOB CARD ENHANCEMENT
-- Database: CRMPLMDEV_2025
-- =============================================

USE [CRMPLMDEV_2025]
GO

SET NOCOUNT ON
GO

PRINT '=========================================='
PRINT 'Adding Maintenance Job Card Document Types'
PRINT '=========================================='
PRINT ''

-- Check if document types already exist
DECLARE @TaskDocCount INT
DECLARE @BeforeImageCount INT
DECLARE @AfterImageCount INT

SELECT @TaskDocCount = COUNT(*) FROM DocumentTypes WHERE [Key] = 'dt_maintenance_task_document'
SELECT @BeforeImageCount = COUNT(*) FROM DocumentTypes WHERE [Key] = 'dt_maintenance_before_image'
SELECT @AfterImageCount = COUNT(*) FROM DocumentTypes WHERE [Key] = 'dt_maintenance_after_image'

-- Add Maintenance Task Document Type
IF @TaskDocCount = 0
BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime, CreatedBySystemUserId)
    VALUES (
        'dt_maintenance_task_document',
        'Maintenance Task Supporting Document',
        'Supporting document for maintenance job card task',
        1,
        0,
        GETDATE(),
        GETDATE(),
        1
    )
    PRINT '✓ Added: Maintenance Task Supporting Document (dt_maintenance_task_document)'
END
ELSE
BEGIN
    PRINT '⚠ Document type already exists: dt_maintenance_task_document'
END

-- Add Maintenance Before Image Type
IF @BeforeImageCount = 0
BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime, CreatedBySystemUserId)
    VALUES (
        'dt_maintenance_before_image',
        'Maintenance Before Image',
        'Image of unit condition before maintenance work',
        1,
        0,
        GETDATE(),
        GETDATE(),
        1
    )
    PRINT '✓ Added: Maintenance Before Image (dt_maintenance_before_image)'
END
ELSE
BEGIN
    PRINT '⚠ Document type already exists: dt_maintenance_before_image'
END

-- Add Maintenance After Image Type
IF @AfterImageCount = 0
BEGIN
    INSERT INTO DocumentTypes ([Key], [Name], [Description], IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime, CreatedBySystemUserId)
    VALUES (
        'dt_maintenance_after_image',
        'Maintenance After Image',
        'Image of unit condition after maintenance work',
        1,
        0,
        GETDATE(),
        GETDATE(),
        1
    )
    PRINT '✓ Added: Maintenance After Image (dt_maintenance_after_image)'
END
ELSE
BEGIN
    PRINT '⚠ Document type already exists: dt_maintenance_after_image'
END

PRINT ''
PRINT '=========================================='
PRINT 'VERIFICATION: Document Types Added'
PRINT '=========================================='
PRINT ''

SELECT 
    [Key],
    [Name],
    [Description],
    IsActive
FROM DocumentTypes
WHERE [Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')

PRINT ''
PRINT '=========================================='
PRINT '✓ DOCUMENT TYPES SETUP COMPLETED'
PRINT '=========================================='

GO

SET NOCOUNT OFF
GO
