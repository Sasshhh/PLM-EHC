/* 40-EHC-Test-Routing.sql  -- OPTIONAL, for a TEST copy of an existing PLM database only.
   Run AFTER PLM-Upgrade-ALL.sql, with the PLM database selected (not master).

   Points every EHC work queue at the factory-test users, so each step reaches someone who
   holds the right role (factory test D011 / D012 / D038). Unlike PLM-Upgrade-ALL.sql this
   CHANGES existing rows, so do not run it on production: there, set the real staff with
   03-Review-Environment-Settings.sql (see EHC_Admin_Go_Live_SOP).
   A test user missing from the database is skipped. Safe to run more than once. */
SET NOCOUNT ON;
IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb') OR OBJECT_ID(N'dbo.AppSettings', 'U') IS NULL
BEGIN
    RAISERROR('STOPPED: select the PLM database first.', 16, 1);
    SET NOEXEC ON;
END
GO
DECLARE @cust TABLE (UserName nvarchar(256), CustomerId int);
INSERT @cust SELECT u.UserName, MIN(c.Id) FROM dbo.AspNetUsers u JOIN dbo.Customers c ON c.SystemUserId = u.SystemUserId GROUP BY u.UserName;

-- Queue owners (AppSettings hold Customers.Id of the user who receives the work)
UPDATE s SET [Value] = CAST(c.CustomerId AS nvarchar(20)), ModifiedDateTime = GETDATE()
FROM dbo.AppSettings s
JOIN (VALUES (N'r_property_manager',            N'richadt'),
             (N'r_revenue_manager',             N'madimetjak'),
             (N'r_revenue_officer',             N'Dimakatsol'),
             (N'u_letting_officer',             N'zanelem'),
             (N'u_housing_super_visor',         N'tshepangm'),
             (N'u_maintenance_manager',         N'SolarTest02'),
             (N'u_property_facilities_manager', N'SolarTest03'),
             (N's_ceo',                         N'SolarTest05')) m([Key], UserName) ON m.[Key] = s.[Key]
JOIN @cust c ON c.UserName = m.UserName;
PRINT CONCAT('Queue owners set: ', @@ROWCOUNT);

-- Complexes with no Letting Officer / Housing Supervisor / Maintenance Manager get the test user.
UPDATE dbo.PreferredComplexAreas SET LettingOfficerId = (SELECT CustomerId FROM @cust WHERE UserName = N'zanelem')
 WHERE LettingOfficerId IS NULL AND EXISTS (SELECT 1 FROM @cust WHERE UserName = N'zanelem');
UPDATE dbo.PreferredComplexAreas SET HousingSuperId = (SELECT CustomerId FROM @cust WHERE UserName = N'tshepangm')
 WHERE HousingSuperId IS NULL AND EXISTS (SELECT 1 FROM @cust WHERE UserName = N'tshepangm');
UPDATE dbo.PreferredComplexAreas SET MaintenanceManagerId = (SELECT CustomerId FROM @cust WHERE UserName = N'SolarTest02')
 WHERE MaintenanceManagerId IS NULL AND EXISTS (SELECT 1 FROM @cust WHERE UserName = N'SolarTest02');

-- D038: the refund authorisation menu needs the CEO role on the s_ceo user.
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'CEO')
    INSERT dbo.AspNetRoles (Id, Name) VALUES (CONVERT(nvarchar(128), NEWID()), N'CEO');
INSERT dbo.AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id FROM dbo.AspNetUsers u CROSS JOIN dbo.AspNetRoles r
WHERE u.UserName = N'SolarTest05' AND r.Name = N'CEO'
  AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = r.Id);

-- Test logins: no forced password change on first login.
UPDATE su SET IsTemporaryPassword = 0
FROM dbo.SystemUsers su JOIN dbo.AspNetUsers u ON u.SystemUserId = su.Id
WHERE u.UserName IN (N'Sash38', N'zanelem', N'Dimakatsol', N'madimetjak', N'richadt', N'SolarTest05',
                     N'tshepangm', N'susans', N'SolarTest02', N'SolarTest03');
UPDATE dbo.AspNetUsers SET LockoutEndDateUtc = NULL, AccessFailedCount = 0
WHERE UserName IN (N'Sash38', N'zanelem', N'Dimakatsol', N'madimetjak', N'richadt', N'SolarTest05',
                   N'tshepangm', N'susans', N'SolarTest02', N'SolarTest03');

-- Uploads: with EDRMSACTIVE = 'on' every uploaded file is also pushed to the city's EDRMS (WSO2).
-- A test server that cannot reach it made each upload wait minutes. Files are still stored in PLM.
UPDATE dbo.AppSettings SET [Value] = N'off' WHERE [Key] = N'EDRMSACTIVE' AND [Value] <> N'off';
PRINT CONCAT('EDRMS upload switched off: ', @@ROWCOUNT, ' row(s)');

-- BR01 stays off while testing (several applications per ID allowed).
UPDATE dbo.AppSettings SET [Value] = N'false' WHERE [Key] = N'ehc_enforce_one_application_per_id';

-- What each queue now points at
SELECT s.[Key], s.[Value] AS CustomerId, u.UserName
FROM dbo.AppSettings s
LEFT JOIN dbo.Customers c ON CAST(c.Id AS nvarchar(20)) = s.[Value]
LEFT JOIN dbo.AspNetUsers u ON u.SystemUserId = c.SystemUserId
WHERE s.[Key] IN (N'r_property_manager', N'r_revenue_manager', N'r_revenue_officer', N'u_letting_officer',
                  N'u_housing_super_visor', N'u_maintenance_manager', N'u_property_facilities_manager', N's_ceo')
ORDER BY s.[Key];
GO
SET NOEXEC OFF;
GO
