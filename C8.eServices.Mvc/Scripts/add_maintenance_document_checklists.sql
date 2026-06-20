-- =============================================
-- ADD DOCUMENT CHECKLISTS FOR MAINTENANCE JOB CARD ENHANCEMENT
-- Database: CRMPLMDEV_2025
-- This script ensures both DocumentTypes and DocumentCheckLists are properly configured
-- =============================================

USE [CRMPLMDEV_2025]
GO

SET NOCOUNT ON
GO

PRINT '=========================================='
PRINT 'Setting up Maintenance Document Configuration'
PRINT '=========================================='
PRINT ''

-- First ensure DocumentTypes exist
DECLARE @TaskDocTypeId INT
DECLARE @BeforeImageTypeId INT  
DECLARE @AfterImageTypeId INT
DECLARE @ApplicationId INT
DECLARE @ReferenceTypeId INT

-- Get Application and Reference Type IDs
SELECT @ApplicationId = Id FROM Applications WHERE [Key] = 'a_rates_clearance_system'
SELECT @ReferenceTypeId = Id FROM ReferenceTypes WHERE [Key] = 'rt_upload_rcs'

PRINT 'Application ID: ' + CAST(ISNULL(@ApplicationId, 0) AS VARCHAR(10))
PRINT 'Reference Type ID: ' + CAST(ISNULL(@ReferenceTypeId, 0) AS VARCHAR(10))
PRINT ''

-- Check/Add Document Types
-- 1. Maintenance Task Document
SELECT @TaskDocTypeId = Id FROM DocumentTypes WHERE [Key] = 'dt_maintenance_task_document'
IF @TaskDocTypeId IS NULL
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
    SELECT @TaskDocTypeId = SCOPE_IDENTITY()
    PRINT '✓ Added DocumentType: Maintenance Task Supporting Document (dt_maintenance_task_document)'
END
ELSE
BEGIN
    PRINT '⚠ DocumentType already exists: dt_maintenance_task_document (ID: ' + CAST(@TaskDocTypeId AS VARCHAR(10)) + ')'
END

-- 2. Maintenance Before Image
SELECT @BeforeImageTypeId = Id FROM DocumentTypes WHERE [Key] = 'dt_maintenance_before_image'
IF @BeforeImageTypeId IS NULL
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
    SELECT @BeforeImageTypeId = SCOPE_IDENTITY()
    PRINT '✓ Added DocumentType: Maintenance Before Image (dt_maintenance_before_image)'
END
ELSE
BEGIN
    PRINT '⚠ DocumentType already exists: dt_maintenance_before_image (ID: ' + CAST(@BeforeImageTypeId AS VARCHAR(10)) + ')'
END

-- 3. Maintenance After Image
SELECT @AfterImageTypeId = Id FROM DocumentTypes WHERE [Key] = 'dt_maintenance_after_image'
IF @AfterImageTypeId IS NULL
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
    SELECT @AfterImageTypeId = SCOPE_IDENTITY()
    PRINT '✓ Added DocumentType: Maintenance After Image (dt_maintenance_after_image)'
END
ELSE
BEGIN
    PRINT '⚠ DocumentType already exists: dt_maintenance_after_image (ID: ' + CAST(@AfterImageTypeId AS VARCHAR(10)) + ')'
END

PRINT ''
PRINT '=========================================='
PRINT 'Adding DocumentCheckList Entries'
PRINT '=========================================='
PRINT ''

-- Now add DocumentCheckList entries if they don't exist
DECLARE @TaskCheckListCount INT
DECLARE @BeforeImageCheckListCount INT
DECLARE @AfterImageCheckListCount INT

-- Check existing entries
SELECT @TaskCheckListCount = COUNT(*) 
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_task_document'

SELECT @BeforeImageCheckListCount = COUNT(*) 
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_before_image'

SELECT @AfterImageCheckListCount = COUNT(*) 
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_after_image'

-- Add DocumentCheckList for Maintenance Task Document
IF @TaskCheckListCount = 0 AND @TaskDocTypeId IS NOT NULL AND @ApplicationId IS NOT NULL AND @ReferenceTypeId IS NOT NULL
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId, ApplicationId, ReferenceTypeId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    VALUES (
        @TaskDocTypeId,
        @ApplicationId,
        @ReferenceTypeId,
        1,
        0,
        0,
        1,
        GETDATE(),
        1,
        GETDATE()
    )
    PRINT '✓ Added DocumentCheckList: Maintenance Task Document'
END
ELSE
BEGIN
    PRINT '⚠ DocumentCheckList already exists or missing dependencies: dt_maintenance_task_document'
END

-- Add DocumentCheckList for Maintenance Before Image
IF @BeforeImageCheckListCount = 0 AND @BeforeImageTypeId IS NOT NULL AND @ApplicationId IS NOT NULL AND @ReferenceTypeId IS NOT NULL
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId, ApplicationId, ReferenceTypeId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    VALUES (
        @BeforeImageTypeId,
        @ApplicationId,
        @ReferenceTypeId,
        1,
        0,
        0,
        1,
        GETDATE(),
        1,
        GETDATE()
    )
    PRINT '✓ Added DocumentCheckList: Maintenance Before Image'
END
ELSE
BEGIN
    PRINT '⚠ DocumentCheckList already exists or missing dependencies: dt_maintenance_before_image'
END

-- Add DocumentCheckList for Maintenance After Image
IF @AfterImageCheckListCount = 0 AND @AfterImageTypeId IS NOT NULL AND @ApplicationId IS NOT NULL AND @ReferenceTypeId IS NOT NULL
BEGIN
    INSERT INTO DocumentCheckLists (DocumentTypeId, ApplicationId, ReferenceTypeId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime)
    VALUES (
        @AfterImageTypeId,
        @ApplicationId,
        @ReferenceTypeId,
        1,
        0,
        0,
        1,
        GETDATE(),
        1,
        GETDATE()
    )
    PRINT '✓ Added DocumentCheckList: Maintenance After Image'
END
ELSE
BEGIN
    PRINT '⚠ DocumentCheckList already exists or missing dependencies: dt_maintenance_after_image'
END

PRINT ''
PRINT '=========================================='
PRINT 'VERIFICATION: Configuration Complete'
PRINT '=========================================='
PRINT ''

-- Verify DocumentTypes
PRINT 'DocumentTypes:'
SELECT 
    dt.Id,
    dt.[Key],
    dt.[Name],
    dt.IsActive
FROM DocumentTypes dt
WHERE dt.[Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')

PRINT ''

-- Verify DocumentCheckLists
PRINT 'DocumentCheckLists:'
SELECT 
    dcl.Id,
    dt.[Key] as DocumentTypeKey,
    dt.[Name] as DocumentTypeName,
    a.[Key] as ApplicationKey,
    rt.[Key] as ReferenceTypeKey,
    dcl.IsActive
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
INNER JOIN Applications a ON dcl.ApplicationId = a.Id
INNER JOIN ReferenceTypes rt ON dcl.ReferenceTypeId = rt.Id
WHERE dt.[Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')

PRINT ''
PRINT '=========================================='
PRINT '✓ MAINTENANCE DOCUMENT CONFIGURATION COMPLETED'
PRINT '=========================================='

GO

SET NOCOUNT OFF
GO