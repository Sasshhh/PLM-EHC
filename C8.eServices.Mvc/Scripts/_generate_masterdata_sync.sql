-- ============================================================
-- GENERATOR SCRIPT
-- Run this on localhost/CRMPLMDEV_2025 to PRODUCE the sync script.
-- Redirect output to sync_masterdata_to_target.sql
--   sqlcmd -S localhost -d CRMPLMDEV_2025 -E -i _generate_masterdata_sync.sql -o sync_masterdata_to_target.sql -y 0 -h -1
-- ============================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @NL  NVARCHAR(4)  = CHAR(13) + CHAR(10);
DECLARE @TAB NVARCHAR(4)  = CHAR(9);
DECLARE @sql NVARCHAR(MAX);

-- ============================================================
PRINT '-- ============================================================';
PRINT '-- MASTER DATA SYNC  —  Source: localhost / CRMPLMDEV_2025';
PRINT '-- Generated : ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '-- Target    : run on any environment database.';
PRINT '-- Strategy  : MERGE keyed on natural [Key] column.';
PRINT '--              DocumentCheckLists keyed on (DocumentTypeId, ReferenceTypeId).';
PRINT '-- ============================================================';
PRINT 'SET NOCOUNT ON;';
PRINT 'SET XACT_ABORT ON;';
PRINT 'BEGIN TRANSACTION;';
PRINT 'BEGIN TRY';
PRINT '';

-- ============================================================
-- 1. StatusTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 1. StatusTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.StatusTypes AS tgt'
    + ' USING (SELECT ' + QUOTENAME(CAST(Id AS VARCHAR(10)),'''')     + ' AS Id'
    + ', '             + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')   + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')         + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)  + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)  + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.StatusTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 2. Status
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 2. Status  (depends on StatusTypes — run StatusTypes first)';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.Status AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(s.Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(s.Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME(s.[Key],''''),'NULL')          + ' AS [Key]'
    + ', (SELECT Id FROM dbo.StatusTypes WHERE [Key] = ' + ISNULL(QUOTENAME(st.[Key],''''),'NULL') + ') AS StatusTypeId'
    + ', '             + CAST(CAST(s.IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(s.IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.StatusTypeId=src.StatusTypeId, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],StatusTypeId,IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.StatusTypeId,src.IsActive,src.IsDeleted);'
FROM dbo.Status s
LEFT JOIN dbo.StatusTypes st ON st.Id = s.StatusTypeId
WHERE s.IsDeleted = 0;

PRINT '';

-- ============================================================
-- 3. ReferenceTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 3. ReferenceTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.ReferenceTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.ReferenceTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 4. LocationTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 4. LocationTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.LocationTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.LocationTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 5. RCSActionTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 5. RCSActionTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.RCSActionTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.RCSActionTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 6. ResponsibilityTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 6. ResponsibilityTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.ResponsibilityTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.ResponsibilityTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 7. ApplicantTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 7. ApplicantTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.ApplicantTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.ApplicantTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 8. ActivityTrackerMessages
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 8. ActivityTrackerMessages';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.ActivityTrackerMessages AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.ActivityTrackerMessages
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 9. AppSettings
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 9. AppSettings  (Value is synced but NOT overwritten if';
PRINT '--    the key already exists — use WHEN NOT MATCHED only)';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.AppSettings AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + ISNULL(QUOTENAME(Value,''''),'NULL')          + ' AS Value'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],Value,IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.Value,src.IsActive,src.IsDeleted);'
FROM dbo.AppSettings
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 10. EmailContentTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 10. EmailContentTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.EmailContentTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.EmailContentTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 11. DocumentTypes
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 11. DocumentTypes';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.DocumentTypes AS tgt'
    + ' USING (SELECT ' + ISNULL(QUOTENAME(Name,''''),'NULL')          + ' AS Name'
    + ', '             + ISNULL(QUOTENAME(Description,''''),'NULL')    + ' AS Description'
    + ', '             + ISNULL(QUOTENAME([Key],''''),'NULL')          + ' AS [Key]'
    + ', '             + CAST(CAST(IsActive  AS TINYINT) AS VARCHAR)   + ' AS IsActive'
    + ', '             + CAST(CAST(IsDeleted AS TINYINT) AS VARCHAR)   + ' AS IsDeleted'
    + ') AS src ON tgt.[Key] = src.[Key]'
    + ' WHEN MATCHED THEN UPDATE SET tgt.Name=src.Name, tgt.Description=src.Description, tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (Name,Description,[Key],IsActive,IsDeleted)'
    + '   VALUES (src.Name,src.Description,src.[Key],src.IsActive,src.IsDeleted);'
FROM dbo.DocumentTypes
WHERE IsDeleted = 0;

PRINT '';

-- ============================================================
-- 12. DocumentCheckLists  (keyed on DocumentTypeKey + ReferenceTypeKey)
-- ============================================================
PRINT '-- ------------------------------------------------------------';
PRINT '-- 12. DocumentCheckLists';
PRINT '--    Depends on DocumentTypes and ReferenceTypes being in sync.';
PRINT '-- ------------------------------------------------------------';

SELECT
    'MERGE INTO dbo.DocumentCheckLists AS tgt'
    + ' USING (SELECT'
    + '   (SELECT Id FROM dbo.DocumentTypes   WHERE [Key] = ' + ISNULL(QUOTENAME(dt.[Key],''''),'NULL') + ') AS DocumentTypeId'
    + ',  (SELECT Id FROM dbo.ReferenceTypes  WHERE [Key] = ' + ISNULL(QUOTENAME(rt.[Key],''''),'NULL') + ') AS ReferenceTypeId'
    + ', '             + CAST(CAST(dcl.IsActive  AS TINYINT) AS VARCHAR) + ' AS IsActive'
    + ', '             + CAST(CAST(dcl.IsDeleted AS TINYINT) AS VARCHAR) + ' AS IsDeleted'
    + ') AS src ON tgt.DocumentTypeId = src.DocumentTypeId AND tgt.ReferenceTypeId = src.ReferenceTypeId'
    + ' WHEN MATCHED THEN UPDATE SET tgt.IsActive=src.IsActive, tgt.IsDeleted=src.IsDeleted'
    + ' WHEN NOT MATCHED BY TARGET THEN INSERT (DocumentTypeId,ReferenceTypeId,IsActive,IsDeleted)'
    + '   VALUES (src.DocumentTypeId,src.ReferenceTypeId,src.IsActive,src.IsDeleted);'
FROM dbo.DocumentCheckLists dcl
JOIN dbo.DocumentTypes  dt ON dt.Id  = dcl.DocumentTypeId
JOIN dbo.ReferenceTypes rt ON rt.Id  = dcl.ReferenceTypeId
WHERE dcl.IsDeleted = 0;

PRINT '';

-- ============================================================
-- Footer
-- ============================================================
PRINT '    COMMIT TRANSACTION;';
PRINT '    PRINT ''Master data sync completed successfully.'';';
PRINT 'END TRY';
PRINT 'BEGIN CATCH';
PRINT '    ROLLBACK TRANSACTION;';
PRINT '    PRINT ''ERROR: '' + ERROR_MESSAGE();';
PRINT '    PRINT ''Line : '' + CAST(ERROR_LINE() AS VARCHAR);';
PRINT 'END CATCH';
