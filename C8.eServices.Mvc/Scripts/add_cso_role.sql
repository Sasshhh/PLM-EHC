-- Add Client Services Officer role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Client Services Officer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name) VALUES (NEWID(), 'Client Services Officer')
    PRINT 'Role created: Client Services Officer'
END
ELSE
    PRINT 'Role already exists.'

-- Verify
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Client Services Officer'
