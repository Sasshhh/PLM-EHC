-- DPRE (Real Estate) roles, 27 Nov 2026.
-- Adds the DPRE Head of Department and DPRE Evaluation Committee roles.
--   * Final authorisation and PTO authorisation go to "DPRE Head of Department" once someone has that role.
--     Until then they keep going to Back Office System Administrator.
--   * Committee reviews go to "DPRE Evaluation Committee" once someone has that role.
--     Until then they keep going to Area Manager. New committee members created in Manage Workflow get the new role.
-- Property Officer (RE_PropertyOfficer_Role.sql) now also does all Real Estate Property Manager work.
-- Safe to run more than once.

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'DPRE Head of Department')
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES ('r_dpre_head_of_department', 'DPRE Head of Department');

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'DPRE Evaluation Committee')
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES ('r_dpre_evaluation_committee', 'DPRE Evaluation Committee');

-- To give a user a role (replace the user name):
-- INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
-- SELECT u.Id, r.Id FROM dbo.AspNetUsers u, dbo.AspNetRoles r
-- WHERE u.UserName = 'someone' AND r.Name = 'DPRE Head of Department'
--   AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = r.Id);

SELECT Name, (SELECT COUNT(*) FROM dbo.AspNetUserRoles ur WHERE ur.RoleId = r.Id) AS Members
FROM dbo.AspNetRoles r
WHERE Name IN ('Property Officer', 'DPRE Head of Department', 'DPRE Evaluation Committee');
GO
