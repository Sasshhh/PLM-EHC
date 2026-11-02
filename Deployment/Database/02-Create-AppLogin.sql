/*
  02-Create-AppLogin.sql
  Creates (or re-links) the login the web app connects with, and gives it the
  rights the app needs in one database. Run once per database
  (main PLM DB and the Cesar email/SMS queue DB).

  SQL authentication (matches the "siyakhokha" style connection strings):
    sqlcmd -S <server\instance> -E -b -i 02-Create-AppLogin.sql ^
      -v DbName="PropertyLeaseManagementRealEstate" LoginName="siyakhokha" LoginPassword="<strong password>"

  Windows authentication (app pool identity or a domain service account):
    sqlcmd -S <server\instance> -E -b -i 02-Create-AppLogin.sql ^
      -v DbName="PropertyLeaseManagementRealEstate" LoginName="DOMAIN\svc_plm" LoginPassword="WINDOWS"
    For the IIS app pool identity on the SAME machine use LoginName="IIS APPPOOL\PLM".
    For SQL on a DIFFERENT machine use the web server's computer account: DOMAIN\WEBSERVER$

  What it does
    - Creates the server login if missing (SQL logins get CHECK_POLICY ON, CHECK_EXPIRATION OFF).
      If the SQL login already exists the password is NOT changed.
    - If the restored DB already has a user with that name (orphaned after the
      restore), re-links it to the login instead of creating a new one.
    - The password must not contain a single quote ('); sqlcmd pastes it in as text.
    - Grants db_datareader, db_datawriter and EXECUTE (ELMAH logging and the
      reporting stored procedures need EXECUTE). It does NOT grant db_owner.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DbName    sysname       = N'$(DbName)';
DECLARE @Login     sysname       = N'$(LoginName)';
DECLARE @Password  nvarchar(256) = N'$(LoginPassword)';
DECLARE @IsWindows bit           = CASE WHEN @Password = N'WINDOWS' THEN 1 ELSE 0 END;
DECLARE @sql nvarchar(max);

IF DB_ID(@DbName) IS NULL
    THROW 50010, 'Database does not exist. Run 01-Restore-Database.sql first.', 1;

-- 1. Server login
IF SUSER_ID(@Login) IS NULL
BEGIN
    IF @IsWindows = 1
        SET @sql = N'CREATE LOGIN ' + QUOTENAME(@Login) + N' FROM WINDOWS WITH DEFAULT_DATABASE = ' + QUOTENAME(@DbName) + N';';
    ELSE
        SET @sql = N'CREATE LOGIN ' + QUOTENAME(@Login) + N' WITH PASSWORD = N''' + REPLACE(@Password, '''', '''''')
                 + N''', DEFAULT_DATABASE = ' + QUOTENAME(@DbName) + N', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;';
    EXEC (@sql);
    PRINT 'Created login ' + @Login;
END
ELSE
    PRINT 'Login ' + @Login + ' already exists (password left unchanged).';

-- 2. Database user + rights (runs inside the target database)
SET @sql = N'USE ' + QUOTENAME(@DbName) + N';
DECLARE @l sysname = @pLogin;
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @l)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals dp JOIN sys.server_principals sp ON sp.sid = dp.sid WHERE dp.name = @l)
    BEGIN
        EXEC (N''ALTER USER '' + @qLogin + N'' WITH LOGIN = '' + @qLogin + N'';'');
        PRINT ''Re-linked orphaned user '' + @l + '' to its login.'';
    END
    ELSE PRINT ''User '' + @l + '' already linked.'';
END
ELSE
BEGIN
    EXEC (N''CREATE USER '' + @qLogin + N'' FOR LOGIN '' + @qLogin + N'' WITH DEFAULT_SCHEMA = dbo;'');
    PRINT ''Created user '' + @l;
END
EXEC (N''ALTER ROLE db_datareader ADD MEMBER '' + @qLogin + N'';'');
EXEC (N''ALTER ROLE db_datawriter ADD MEMBER '' + @qLogin + N'';'');
EXEC (N''GRANT EXECUTE TO '' + @qLogin + N'';'');
PRINT ''Granted db_datareader, db_datawriter, EXECUTE to '' + @l + '' in '' + DB_NAME();';

DECLARE @q nvarchar(300) = QUOTENAME(@Login);
EXEC sys.sp_executesql @sql, N'@pLogin sysname, @qLogin nvarchar(300)', @pLogin = @Login, @qLogin = @q;

-- 3. Report any other orphaned users left over from the old server.
SET @sql = N'SELECT dp.name AS StillOrphanedUser FROM ' + QUOTENAME(@DbName) + N'.sys.database_principals dp
  LEFT JOIN sys.server_principals sp ON sp.sid = dp.sid
  WHERE dp.type IN (''S'',''U'',''G'') AND dp.principal_id > 4 AND sp.sid IS NULL AND dp.authentication_type <> 0;';
EXEC (@sql);
