/*
  PLM-CEO-Setup.sql: run AFTER PLM-Upgrade-ALL.sql, against the same PLM database.

  The CEO's deposit refund authorisation (Termination > Refund Authorisations) needs:
    - a user holding the AspNet role 'CEO' (the role did not exist on the restored copy), and
    - AppSettings s_ceo = that user's Customers.Id (PLM-Upgrade-ALL.sql adds s_ceo as 0 = nobody).
  (The local test copy used the test account SolarTest05 on 2026-11-03; do not use it on a real server.)

  FILL IN @CeoUserName below with the real CEO's username before running. Safe to run twice.
  The user must sign out and in again before the Refund Authorisations menu shows.
*/
SET NOCOUNT ON; SET XACT_ABORT ON;
DECLARE @CeoUserName nvarchar(256) = N'';   -- <<< the real CEO's username (AspNetUsers.UserName)

IF LTRIM(RTRIM(@CeoUserName)) = N''
BEGIN RAISERROR('Set @CeoUserName to the real CEO''s username at the top of this script. Nothing changed.', 16, 1); RETURN; END
IF OBJECT_ID(N'dbo.AppSettings', 'U') IS NULL OR OBJECT_ID(N'dbo.AspNetRoles', 'U') IS NULL
BEGIN RAISERROR('Not a PLM database: pick the PLM database and run again.', 16, 1); RETURN; END

DECLARE @uid nvarchar(128), @cid int;
SELECT TOP (1) @uid = u.Id, @cid = c.Id
FROM dbo.AspNetUsers u JOIN dbo.Customers c ON c.SystemUserId = u.SystemUserId
WHERE u.UserName = @CeoUserName;
IF @uid IS NULL
BEGIN RAISERROR('User %s not found (or has no Customers row). Nothing changed.', 16, 1, @CeoUserName); RETURN; END

BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'CEO')
    INSERT dbo.AspNetRoles (Id, Name) VALUES (CONVERT(nvarchar(128), NEWID()), N'CEO');
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId WHERE ur.UserId = @uid AND r.Name = N'CEO')
    INSERT dbo.AspNetUserRoles (UserId, RoleId) SELECT @uid, Id FROM dbo.AspNetRoles WHERE Name = N'CEO';
IF EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = N's_ceo')
    UPDATE dbo.AppSettings SET Value = CAST(@cid AS nvarchar(20)), ModifiedDateTime = GETDATE() WHERE [Key] = N's_ceo';
ELSE
    INSERT dbo.AppSettings ([Name], [Description], [Key], [Value], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'CEO', N'Customer Id of the CEO who authorises deposit refunds (0 = not set)', N's_ceo', CAST(@cid AS nvarchar(20)), 1, 0, GETDATE());
COMMIT TRANSACTION;

SELECT u.UserName, r.Name AS Role FROM dbo.AspNetUsers u
JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
WHERE u.Id = @uid;
SELECT [Key], Value FROM dbo.AppSettings WHERE [Key] = N's_ceo';
