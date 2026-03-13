-- Assign AshKay to 'Letting Officer' role so the round-robin and area assignment works
-- (LettingOfficerId on PreferredComplexAreas uses Letting Officer role lookups)
-- Also assign to 'Client Services Officer' for nav/UI access

DECLARE @UserId   NVARCHAR(128) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'AshKay')
DECLARE @LORole   NVARCHAR(128) = (SELECT Id FROM AspNetRoles WHERE Name = 'Letting Officer')
DECLARE @CSORole  NVARCHAR(128) = (SELECT Id FROM AspNetRoles WHERE Name = 'Client Services Officer')

-- Letting Officer (required for area assignment + round-robin)
IF @UserId IS NOT NULL AND @LORole IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @LORole)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @LORole)
        PRINT 'AshKay assigned to Letting Officer.'
    END
    ELSE
        PRINT 'AshKay already has Letting Officer.'
END

-- Client Services Officer (for nav visibility)
IF @UserId IS NOT NULL AND @CSORole IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @CSORole)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @CSORole)
        PRINT 'AshKay assigned to Client Services Officer.'
    END
    ELSE
        PRINT 'AshKay already has Client Services Officer.'
END

-- Verify
SELECT r.Name, u.UserName
FROM AspNetUserRoles ur
JOIN AspNetRoles r ON r.Id = ur.RoleId
JOIN AspNetUsers u ON u.Id = ur.UserId
WHERE u.UserName = 'AshKay'
