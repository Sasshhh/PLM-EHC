/* Applied by the full install script right after the data is loaded (fresh database only).
   1. Keys added in this release that the reference database did not have yet.
   2. Workflow routing for the EHC test users, so every queue reaches a user who holds the right role
      (factory test D011 / D012 / D038: on the dev database some queues pointed at the wrong person).
   Before go-live, point these at the real staff: see EHC_Admin_Go_Live_SOP and 03-Review-Environment-Settings.sql. */

-- 1. D028: Revenue Manager supports a risk assessment -> waiting for the CEO
IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = N's_awaiting_ceo_risk_approval')
    INSERT dbo.Status ([Name], [Description], [Key], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    SELECT N'Awaiting CEO Approval', N'Risk assessment supported by the Revenue Manager, awaiting CEO approval',
           N's_awaiting_ceo_risk_approval', StatusTypeId, 1, 0, GETDATE()
    FROM dbo.Status WHERE [Key] = N's_incentive_policy_application_approved' AND Id = (SELECT MIN(Id) FROM dbo.Status WHERE [Key] = N's_incentive_policy_application_approved');

-- 2. Queue owners (AppSettings hold Customers.Id of the user who receives the work)
DECLARE @cust TABLE (UserName nvarchar(256), CustomerId int);
INSERT @cust SELECT u.UserName, c.Id FROM dbo.AspNetUsers u JOIN dbo.Customers c ON c.SystemUserId = u.SystemUserId;

UPDATE s SET [Value] = CAST(c.CustomerId AS nvarchar(20))
FROM dbo.AppSettings s
JOIN (VALUES (N'r_property_manager',            N'richadt'),
             (N'r_revenue_manager',             N'madimetjak'),
             (N'r_revenue_officer',             N'Dimakatsol'),
             (N'u_letting_officer',             N'zanelem'),      -- was a Property & Facilities Manager (D012)
             (N'u_housing_super_visor',         N'tshepangm'),    -- was a Property Manager (D011)
             (N'u_maintenance_manager',         N'SolarTest02'),
             (N'u_property_facilities_manager', N'SolarTest03'),
             (N's_ceo',                         N'SolarTest05')) m([Key], UserName) ON m.[Key] = s.[Key]
JOIN @cust c ON c.UserName = m.UserName;

-- Every complex needs a Letting Officer (new applications route there), a Housing Supervisor
-- (lease agreement "represented by") and a Maintenance Manager.
UPDATE dbo.PreferredComplexAreas SET LettingOfficerId = (SELECT CustomerId FROM @cust WHERE UserName = N'zanelem') WHERE LettingOfficerId IS NULL;
UPDATE dbo.PreferredComplexAreas SET HousingSuperId = (SELECT CustomerId FROM @cust WHERE UserName = N'tshepangm') WHERE HousingSuperId IS NULL;
UPDATE dbo.PreferredComplexAreas SET MaintenanceManagerId = (SELECT CustomerId FROM @cust WHERE UserName = N'SolarTest02') WHERE MaintenanceManagerId IS NULL;

-- Test accounts: no forced password change, nothing locked out.
UPDATE dbo.SystemUsers SET IsTemporaryPassword = 0 WHERE Id IN (SELECT SystemUserId FROM dbo.AspNetUsers);

-- D038: the refund authorisation menu needs the CEO role on the s_ceo user.
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'CEO')
    INSERT dbo.AspNetRoles (Id, Name) VALUES (CONVERT(nvarchar(128), NEWID()), N'CEO');
INSERT dbo.AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id FROM dbo.AspNetUsers u CROSS JOIN dbo.AspNetRoles r
WHERE u.UserName = N'SolarTest05' AND r.Name = N'CEO'
  AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = r.Id);

-- Same department tag as the status it replaces, where the Status table carries one.
IF COL_LENGTH('dbo.Status', 'DepartmentId') IS NOT NULL
    EXEC(N'UPDATE n SET DepartmentId = o.DepartmentId FROM dbo.Status n
           CROSS APPLY (SELECT TOP (1) DepartmentId FROM dbo.Status WHERE [Key] = N''s_incentive_policy_application_approved'' ORDER BY Id) o
           WHERE n.[Key] = N''s_awaiting_ceo_risk_approval'' AND n.DepartmentId IS NULL;');

-- BR01 switch: allow several applications per ID while testing. Set to 'true' for go-live.
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = N'ehc_enforce_one_application_per_id')
    INSERT dbo.AppSettings ([Name], [Description], [Key], [Value], IsActive, IsDeleted, CreatedDateTime)
    VALUES (N'EHC one application per ID (BR01)', N'true = refuse a second application while the ID is on the waiting list or holds an active lease; false = allow (testing)', N'ehc_enforce_one_application_per_id', N'false', 1, 0, GETDATE());
