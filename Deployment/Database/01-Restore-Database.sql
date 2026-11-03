/*
  01-Restore-Database.sql
  Restores a PLM database from a .bak file, moving every data/log file into the
  server's data and log folders (logical file names are read from the backup,
  so it works for any .bak without editing).

  Run with sqlcmd (SQLCMD mode in SSMS also works: Query > SQLCMD Mode):

    sqlcmd -S <server\instance> -E -b -i 01-Restore-Database.sql ^
      -v DbName="PropertyLeaseManagementRealEstate" ^
         BakPath="D:\Backups\PLM.bak" ^
         DataDir="DEFAULT" LogDir="DEFAULT" Replace="0"

  Variables
    DbName   Name of the database to create.
    BakPath  Full path to the .bak AS SEEN BY THE SQL SERVER SERVICE (local disk
             of the SQL box or a UNC path the service account can read).
    DataDir  Folder for data files. DEFAULT = server default data folder.
    LogDir   Folder for log files.  DEFAULT = server default log folder.
    Replace  1 = overwrite an existing database of the same name (it is put in
             SINGLE_USER first, which disconnects everyone). 0 = stop if it exists.

  Supports SQL Server 2012 and later. A backup can only be restored on the SAME
  or a NEWER major version than the one it was taken on.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DbName  sysname        = N'$(DbName)';
DECLARE @BakPath nvarchar(4000) = N'$(BakPath)';
DECLARE @DataDir nvarchar(4000) = N'$(DataDir)';
DECLARE @LogDir  nvarchar(4000) = N'$(LogDir)';
DECLARE @Replace bit            = $(Replace);

IF @DataDir IN (N'', N'DEFAULT') SET @DataDir = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath'));
IF @LogDir  IN (N'', N'DEFAULT') SET @LogDir  = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultLogPath'));
IF @DataDir IS NULL OR @LogDir IS NULL
    THROW 50001, 'Could not work out the default data/log folders (SQL 2012?). Pass DataDir and LogDir explicitly.', 1;

-- Normalise trailing separator (works for Windows and Linux paths).
DECLARE @sep nchar(1) = CASE WHEN CHARINDEX(N'/', @DataDir) > 0 THEN N'/' ELSE N'\' END;
IF RIGHT(@DataDir, 1) NOT IN (N'\', N'/') SET @DataDir += @sep;
IF RIGHT(@LogDir, 1)  NOT IN (N'\', N'/') SET @LogDir  += @sep;

PRINT 'Server : ' + @@SERVERNAME + '  (' + CONVERT(varchar(30), SERVERPROPERTY('ProductVersion')) + ')';
PRINT 'Restore: ' + @BakPath + '  ->  [' + @DbName + ']';
PRINT 'Data   : ' + @DataDir;
PRINT 'Log    : ' + @LogDir;

------------------------------------------------------------------------------
-- 1. Read the file list from the backup. The column set grew over versions:
--    2012-2014 = 20 columns, 2016+ adds SnapshotURL.
------------------------------------------------------------------------------
CREATE TABLE #files (
    LogicalName nvarchar(128), PhysicalName nvarchar(260), [Type] char(1),
    FileGroupName nvarchar(128), Size numeric(20,0), MaxSize numeric(20,0),
    FileId bigint, CreateLSN numeric(25,0), DropLSN numeric(25,0),
    UniqueId uniqueidentifier, ReadOnlyLSN numeric(25,0), ReadWriteLSN numeric(25,0),
    BackupSizeInBytes bigint, SourceBlockSize int, FileGroupId int,
    LogGroupGUID uniqueidentifier, DifferentialBaseLSN numeric(25,0),
    DifferentialBaseGUID uniqueidentifier, IsReadOnly bit, IsPresent bit,
    TDEThumbprint varbinary(32)
);
IF CONVERT(int, PARSENAME(CONVERT(varchar(30), SERVERPROPERTY('ProductVersion')), 4)) >= 13
    ALTER TABLE #files ADD SnapshotURL nvarchar(360);

DECLARE @sql nvarchar(max) = N'RESTORE FILELISTONLY FROM DISK = @p';
INSERT #files EXEC sys.sp_executesql @sql, N'@p nvarchar(4000)', @p = @BakPath;

IF NOT EXISTS (SELECT 1 FROM #files)
    THROW 50002, 'The backup file list is empty or unreadable. Check BakPath and that the SQL service account can read it.', 1;

------------------------------------------------------------------------------
-- 2. Existing database handling.
------------------------------------------------------------------------------
IF DB_ID(@DbName) IS NOT NULL
BEGIN
    IF @Replace = 0
        THROW 50003, 'Database already exists. Re-run with Replace=1 to overwrite it, or pick another DbName.', 1;
    PRINT 'Database exists, Replace=1: switching to SINGLE_USER and overwriting.';
    SET @sql = N'ALTER DATABASE ' + QUOTENAME(@DbName) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;';
    EXEC (@sql);
END

------------------------------------------------------------------------------
-- 3. Build RESTORE ... WITH MOVE for every file in the backup.
--    Files are named <DbName><suffix> so two restores never collide.
------------------------------------------------------------------------------
DECLARE @moves nvarchar(max) = N'';
SELECT @moves += N', MOVE N''' + REPLACE(LogicalName, '''', '''''') + N''' TO N'''
     + REPLACE(CASE WHEN [Type] = 'L' THEN @LogDir ELSE @DataDir END, '''', '''''')
     + REPLACE(@DbName, '''', '''''')
     + CASE
         WHEN [Type] = 'L' THEN CASE WHEN FileId = MIN(FileId) OVER (PARTITION BY [Type]) THEN N'_log.ldf'
                                     ELSE N'_log' + CONVERT(nvarchar(10), FileId) + N'.ldf' END
         WHEN [Type] = 'S' THEN N'_' + CONVERT(nvarchar(10), FileId)   -- FILESTREAM / in-memory folder
         WHEN FileId = 1    THEN N'.mdf'
         ELSE N'_' + CONVERT(nvarchar(10), FileId) + N'.ndf'
       END
     + N''''
FROM #files
ORDER BY FileId;

SET @sql = N'RESTORE DATABASE ' + QUOTENAME(@DbName) + N' FROM DISK = N''' + REPLACE(@BakPath, '''', '''''') + N''''
         + N' WITH RECOVERY, STATS = 10' + CASE WHEN @Replace = 1 THEN N', REPLACE' ELSE N'' END
         + @moves + N';';
PRINT @sql;
EXEC (@sql);

------------------------------------------------------------------------------
-- 4. Bring it up to this server's level and back to normal use.
------------------------------------------------------------------------------
DECLARE @compat int = CONVERT(int, PARSENAME(CONVERT(varchar(30), SERVERPROPERTY('ProductVersion')), 4)) * 10;
IF @compat > 160 SET @compat = 160;   -- cap at SQL 2022 level unless you raise it deliberately
SET @sql = N'ALTER DATABASE ' + QUOTENAME(@DbName) + N' SET MULTI_USER;'
         + N'ALTER DATABASE ' + QUOTENAME(@DbName) + N' SET COMPATIBILITY_LEVEL = ' + CONVERT(nvarchar(5), @compat) + N';'
         + N'ALTER AUTHORIZATION ON DATABASE::' + QUOTENAME(@DbName) + N' TO [sa];';
EXEC (@sql);

-- A restored DB keeps the old server's user SIDs; list orphans for 02-Create-AppLogin.sql to fix.
SET @sql = N'SELECT dp.name AS OrphanedUser FROM ' + QUOTENAME(@DbName) + N'.sys.database_principals dp
  LEFT JOIN sys.server_principals sp ON sp.sid = dp.sid
  WHERE dp.type IN (''S'',''U'',''G'') AND dp.principal_id > 4 AND sp.sid IS NULL AND dp.authentication_type <> 0;';
EXEC (@sql);

PRINT 'Restore of [' + @DbName + '] complete. Compatibility level set to ' + CONVERT(varchar(5), @compat) + '.';
DROP TABLE #files;
