-- Copy Sash38's password hash to AshKay
UPDATE AspNetUsers
SET PasswordHash = (SELECT PasswordHash FROM AspNetUsers WHERE UserName = 'Sash38')
WHERE UserName = 'AshKay'

-- Verify
SELECT UserName, Email FROM AspNetUsers WHERE UserName = 'AshKay'
