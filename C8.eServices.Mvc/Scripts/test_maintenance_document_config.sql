-- =============================================
-- TEST MAINTENANCE DOCUMENT CONFIGURATION
-- Quick verification that DocumentTypes and DocumentCheckLists are properly set up
-- =============================================

USE [CRMPLMDEV_2025]
GO

PRINT '=========================================='
PRINT 'Testing Maintenance Document Configuration'
PRINT '=========================================='
PRINT ''

-- Test the same lookup that's failing in the code
DECLARE @DocumentCheckListId INT

SELECT @DocumentCheckListId = dcl.Id
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_task_document'
AND dcl.IsActive = 1 
AND dcl.IsDeleted = 0

IF @DocumentCheckListId IS NOT NULL
BEGIN
    PRINT '✓ SUCCESS: MaintenanceTaskDocument DocumentCheckList found (ID: ' + CAST(@DocumentCheckListId AS VARCHAR(10)) + ')'
END
ELSE
BEGIN
    PRINT '✗ FAILED: MaintenanceTaskDocument DocumentCheckList not found'
    PRINT ''
    PRINT 'Available DocumentTypes:'
    SELECT [Key], [Name], IsActive FROM DocumentTypes WHERE [Key] LIKE '%maintenance%'
    
    PRINT ''
    PRINT 'Available DocumentCheckLists:'
    SELECT 
        dcl.Id,
        dt.[Key],
        dt.[Name],
        dcl.IsActive as CheckListActive,
        dcl.IsDeleted as CheckListDeleted
    FROM DocumentCheckLists dcl
    INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
    WHERE dt.[Key] LIKE '%maintenance%'
END

-- Test Before Image
SELECT @DocumentCheckListId = dcl.Id
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_before_image'
AND dcl.IsActive = 1 
AND dcl.IsDeleted = 0

IF @DocumentCheckListId IS NOT NULL
BEGIN
    PRINT '✓ SUCCESS: MaintenanceBeforeImage DocumentCheckList found (ID: ' + CAST(@DocumentCheckListId AS VARCHAR(10)) + ')'
END
ELSE
BEGIN
    PRINT '✗ FAILED: MaintenanceBeforeImage DocumentCheckList not found'
END

-- Test After Image
SELECT @DocumentCheckListId = dcl.Id
FROM DocumentCheckLists dcl
INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
WHERE dt.[Key] = 'dt_maintenance_after_image'
AND dcl.IsActive = 1 
AND dcl.IsDeleted = 0

IF @DocumentCheckListId IS NOT NULL
BEGIN
    PRINT '✓ SUCCESS: MaintenanceAfterImage DocumentCheckList found (ID: ' + CAST(@DocumentCheckListId AS VARCHAR(10)) + ')'
END
ELSE
BEGIN
    PRINT '✗ FAILED: MaintenanceAfterImage DocumentCheckList not found'
END

PRINT ''
PRINT '=========================================='
PRINT 'Configuration Test Complete'
PRINT '=========================================='

GO