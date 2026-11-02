/*
  03-Review-Environment-Settings.sql
  A restored database still points at the OLD environment through rows in
  dbo.AppSettings (report server URL, upload/bill folders, API URLs, SharePoint,
  SMPP host, ...). This lists every active setting whose value looks like a URL,
  a UNC/drive path, an IP address or a host:port, so you can update them for the
  new server. Secret-looking keys are masked; the script changes nothing.

    sqlcmd -S <server\instance> -E -W -i 03-Review-Environment-Settings.sql -v DbName="PropertyLeaseManagementRealEstate"

  To change one:
    UPDATE dbo.AppSettings SET Value = N'<new value>' WHERE [Key] = N'<key>' AND IsDeleted = 0;
  The app caches nothing here between requests, so no restart is needed, but
  recycle the app pool if you are unsure.
*/
SET NOCOUNT ON;
USE [$(DbName)];

SELECT  s.Id,
        s.[Key],
        CASE WHEN s.[Key] LIKE N'%password%' OR s.[Key] LIKE N'%secret%' OR s.[Key] LIKE N'%api_key%'
                  OR s.[Key] LIKE N'%customer_key%' OR s.[Key] LIKE N'%token%'
             THEN N'*** (secret, not shown)'
             ELSE LEFT(s.Value, 300) END AS Value,
        CASE
            WHEN s.Value LIKE N'http%://%'                         THEN 'URL'
            WHEN s.Value LIKE N'\\%'                               THEN 'UNC path'
            WHEN s.Value LIKE N'[A-Za-z]:\%'                       THEN 'Drive path'
            WHEN s.Value LIKE N'%[0-9].%[0-9].%[0-9].%[0-9]%'      THEN 'IP address'
            WHEN s.Value LIKE N'~/%' OR s.Value LIKE N'/%'         THEN 'Site-relative path'
            ELSE 'Host/other'
        END AS Kind
FROM dbo.AppSettings s
WHERE s.IsActive = 1 AND s.IsDeleted = 0
  AND (   s.Value LIKE N'http%://%'
       OR s.Value LIKE N'\\%'
       OR s.Value LIKE N'[A-Za-z]:\%'
       OR s.Value LIKE N'%[0-9].%[0-9].%[0-9].%[0-9]%'
       OR s.Value LIKE N'~/%'
       OR s.[Key] LIKE N'%host%' OR s.[Key] LIKE N'%url%' OR s.[Key] LIKE N'%location%'
       OR s.[Key] LIKE N'%directory%' OR s.[Key] LIKE N'%path%' OR s.[Key] LIKE N'%domain%'
       OR s.[Key] LIKE N'%server%')
ORDER BY Kind, s.[Key];
