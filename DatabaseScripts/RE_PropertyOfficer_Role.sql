-- DPRE UCXX actor confirmed with the BA (26 Nov 2026): "Property Officer" (was "Property/LED Officer").
-- Creates the role and gives it to the existing test user re_property_officer.
-- Property Officers can use Lease Administration (Lease Dashboard, Lease Register, Capture Lease Details).
-- Safe to run more than once.

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Property Officer')
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES ('r_property_officer', 'Property Officer');

DECLARE @roleId NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = 'Property Officer');
DECLARE @userId NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetUsers WHERE UserName = 're_property_officer' ORDER BY Id DESC);
IF @userId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @userId AND RoleId = @roleId)
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@userId, @roleId);
GO
