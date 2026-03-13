-- Leave AshKay with only Client Services Officer (remove Letting Officer and Customers)
DECLARE @UserId NVARCHAR(128) = (SELECT Id FROM AspNetUsers WHERE UserName = 'AshKay')

DELETE FROM AspNetUserRoles
WHERE UserId = @UserId
AND RoleId IN (
    SELECT Id FROM AspNetRoles WHERE Name IN ('Letting Officer', 'Customers')
)

-- Verify
SELECT u.UserName, r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'AshKay'
