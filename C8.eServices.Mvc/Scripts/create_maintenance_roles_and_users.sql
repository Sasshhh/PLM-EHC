-- Create Maintenance Manager and Property & Facilities Manager roles and placeholder users
-- This script creates the roles if they don't exist and provides a template for creating users

-- ==========================================
-- STEP 1: Create Roles
-- ==========================================

-- Create Maintenance Manager role if it doesn't exist
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Maintenance Manager')
BEGIN
    DECLARE @MaintenanceManagerRoleId NVARCHAR(128) = NEWID();
    
    INSERT INTO AspNetRoles (Id, Name)
    VALUES (@MaintenanceManagerRoleId, 'Maintenance Manager');
    
    PRINT 'Maintenance Manager role created with ID: ' + @MaintenanceManagerRoleId;
END
ELSE
BEGIN
    PRINT 'Maintenance Manager role already exists';
END
GO

-- Create Property & Facilities Manager role if it doesn't exist
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Property & Facilities Manager')
BEGIN
    DECLARE @PropertyFacilitiesManagerRoleId NVARCHAR(128) = NEWID();
    
    INSERT INTO AspNetRoles (Id, Name)
    VALUES (@PropertyFacilitiesManagerRoleId, 'Property & Facilities Manager');
    
    PRINT 'Property & Facilities Manager role created with ID: ' + @PropertyFacilitiesManagerRoleId;
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager role already exists';
END
GO

-- ==========================================
-- STEP 2: View existing roles
-- ==========================================

SELECT 
    Id AS RoleId,
    Name AS RoleName
FROM AspNetRoles
WHERE Name IN ('Maintenance Manager', 'Property & Facilities Manager')
ORDER BY Name;
GO

-- ==========================================
-- STEP 3: Create placeholder users (TEMPLATE)
-- ==========================================

-- IMPORTANT: This is a TEMPLATE. You need to:
-- 1. Replace '<USERNAME>' with actual username (e.g., 'maintenance01', 'facilities01')
-- 2. Replace '<EMAIL>' with actual email
-- 3. Replace '<FIRSTNAME>' and '<LASTNAME>' with actual names
-- 4. Set proper password hash (or use the Account/InternalRegister page in the application)

-- Template for Maintenance Manager User
/*
DECLARE @MaintenanceUserName NVARCHAR(256) = '<USERNAME>';  -- e.g., 'maintenance01'
DECLARE @MaintenanceEmail NVARCHAR(256) = '<EMAIL>';  -- e.g., 'maintenance@ekurhuleni.gov.za'
DECLARE @MaintenanceFirstName NVARCHAR(100) = '<FIRSTNAME>';  -- e.g., 'Maintenance'
DECLARE @MaintenanceLastName NVARCHAR(100) = '<LASTNAME>';  -- e.g., 'Manager'

-- Get the Maintenance Manager role ID
DECLARE @MaintenanceRoleId NVARCHAR(128);
SELECT @MaintenanceRoleId = Id FROM AspNetRoles WHERE Name = 'Maintenance Manager';

-- Check if user already exists
IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE UserName = @MaintenanceUserName)
BEGIN
    -- Create AspNetUser
    DECLARE @MaintenanceAspNetUserId NVARCHAR(128) = NEWID();
    
    INSERT INTO AspNetUsers (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
    VALUES (
        @MaintenanceAspNetUserId,
        @MaintenanceEmail,
        1,  -- EmailConfirmed
        'PLACEHOLDER_HASH',  -- YOU MUST SET A PROPER PASSWORD HASH OR USE INTERNAL REGISTRATION
        NEWID(),
        0,  -- PhoneNumberConfirmed
        0,  -- TwoFactorEnabled
        1,  -- LockoutEnabled
        0,  -- AccessFailedCount
        @MaintenanceUserName
    );
    
    -- Create SystemUser
    INSERT INTO SystemUsers (UserId, UserName, IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        @MaintenanceAspNetUserId,
        @MaintenanceUserName,
        1,  -- IsActive
        0,  -- IsDeleted
        GETDATE()
    );
    
    -- Get the SystemUser ID
    DECLARE @MaintenanceSystemUserId INT;
    SELECT @MaintenanceSystemUserId = Id FROM SystemUsers WHERE UserId = @MaintenanceAspNetUserId;
    
    -- Create Customer
    INSERT INTO Customers (
        SystemUserId, 
        FirstNames, 
        LastName, 
        FullName, 
        IsActive, 
        IsDeleted, 
        CreatedDateTime,
        ApplicationEntityId
    )
    VALUES (
        @MaintenanceSystemUserId,
        @MaintenanceFirstName,
        @MaintenanceLastName,
        @MaintenanceFirstName + ' ' + @MaintenanceLastName,
        1,  -- IsActive
        0,  -- IsDeleted
        GETDATE(),
        (SELECT Id FROM ApplicationEntities WHERE [Key] = 'ekurhuleni_housing_company')  -- EHC
    );
    
    -- Assign role to user
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@MaintenanceAspNetUserId, @MaintenanceRoleId);
    
    PRINT 'Maintenance Manager user created: ' + @MaintenanceUserName;
    
    -- Display the created Customer ID
    SELECT 
        'Maintenance Manager Customer ID' AS Info,
        c.Id AS CustomerId,
        c.FullName,
        su.UserName
    FROM Customers c
    JOIN SystemUsers su ON su.Id = c.SystemUserId
    WHERE su.UserId = @MaintenanceAspNetUserId;
END
ELSE
BEGIN
    PRINT 'User already exists: ' + @MaintenanceUserName;
END
*/

-- ==========================================
-- Template for Property & Facilities Manager User
-- ==========================================

/*
DECLARE @FacilitiesUserName NVARCHAR(256) = '<USERNAME>';  -- e.g., 'facilities01'
DECLARE @FacilitiesEmail NVARCHAR(256) = '<EMAIL>';  -- e.g., 'facilities@ekurhuleni.gov.za'
DECLARE @FacilitiesFirstName NVARCHAR(100) = '<FIRSTNAME>';  -- e.g., 'Facilities'
DECLARE @FacilitiesLastName NVARCHAR(100) = '<LASTNAME>';  -- e.g., 'Manager'

-- Get the Property & Facilities Manager role ID
DECLARE @FacilitiesRoleId NVARCHAR(128);
SELECT @FacilitiesRoleId = Id FROM AspNetRoles WHERE Name = 'Property & Facilities Manager';

-- Check if user already exists
IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE UserName = @FacilitiesUserName)
BEGIN
    -- Create AspNetUser
    DECLARE @FacilitiesAspNetUserId NVARCHAR(128) = NEWID();
    
    INSERT INTO AspNetUsers (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
    VALUES (
        @FacilitiesAspNetUserId,
        @FacilitiesEmail,
        1,  -- EmailConfirmed
        'PLACEHOLDER_HASH',  -- YOU MUST SET A PROPER PASSWORD HASH OR USE INTERNAL REGISTRATION
        NEWID(),
        0,  -- PhoneNumberConfirmed
        0,  -- TwoFactorEnabled
        1,  -- LockoutEnabled
        0,  -- AccessFailedCount
        @FacilitiesUserName
    );
    
    -- Create SystemUser
    INSERT INTO SystemUsers (UserId, UserName, IsActive, IsDeleted, CreatedDateTime)
    VALUES (
        @FacilitiesAspNetUserId,
        @FacilitiesUserName,
        1,  -- IsActive
        0,  -- IsDeleted
        GETDATE()
    );
    
    -- Get the SystemUser ID
    DECLARE @FacilitiesSystemUserId INT;
    SELECT @FacilitiesSystemUserId = Id FROM SystemUsers WHERE UserId = @FacilitiesAspNetUserId;
    
    -- Create Customer
    INSERT INTO Customers (
        SystemUserId, 
        FirstNames, 
        LastName, 
        FullName, 
        IsActive, 
        IsDeleted, 
        CreatedDateTime,
        ApplicationEntityId
    )
    VALUES (
        @FacilitiesSystemUserId,
        @FacilitiesFirstName,
        @FacilitiesLastName,
        @FacilitiesFirstName + ' ' + @FacilitiesLastName,
        1,  -- IsActive
        0,  -- IsDeleted
        GETDATE(),
        (SELECT Id FROM ApplicationEntities WHERE [Key] = 'ekurhuleni_housing_company')  -- EHC
    );
    
    -- Assign role to user
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@FacilitiesAspNetUserId, @FacilitiesRoleId);
    
    PRINT 'Property & Facilities Manager user created: ' + @FacilitiesUserName;
    
    -- Display the created Customer ID
    SELECT 
        'Property & Facilities Manager Customer ID' AS Info,
        c.Id AS CustomerId,
        c.FullName,
        su.UserName
    FROM Customers c
    JOIN SystemUsers su ON su.Id = c.SystemUserId
    WHERE su.UserId = @FacilitiesAspNetUserId;
END
ELSE
BEGIN
    PRINT 'User already exists: ' + @FacilitiesUserName;
END
*/

-- ==========================================
-- RECOMMENDED APPROACH
-- ==========================================

PRINT '';
PRINT '=========================================='
PRINT 'RECOMMENDED APPROACH:'
PRINT '=========================================='
PRINT '';
PRINT '1. Use the application UI to create users:';
PRINT '   - Navigate to: Account/InternalRegister';
PRINT '   - Create a user for Maintenance Manager role';
PRINT '   - Create a user for Property & Facilities Manager role';
PRINT '';
PRINT '2. Then assign the roles:';
PRINT '   - Use the templates above to assign roles via SQL';
PRINT '   - OR use the Area Manager UI to assign roles';
PRINT '';
PRINT '3. Get the Customer IDs:';
PRINT '   - Run the query below to find the Customer IDs';
PRINT '   - Use these IDs in the backfill_maintenance_managers.sql script';
PRINT '';
PRINT '=========================================='
GO

-- Query to find existing users and their Customer IDs
SELECT 
    'Existing Back Office Users' AS Info,
    c.Id AS CustomerId,
    c.FullName,
    su.UserName,
    r.Name AS RoleName,
    ae.Name AS Department
FROM Customers c
JOIN SystemUsers su ON su.Id = c.SystemUserId
JOIN AspNetUsers anu ON anu.Id = su.UserId
JOIN AspNetUserRoles aur ON aur.UserId = anu.Id
JOIN AspNetRoles r ON r.Id = aur.RoleId
LEFT JOIN ApplicationEntities ae ON ae.Id = c.ApplicationEntityId
WHERE c.IsActive = 1 
  AND c.IsDeleted = 0
  AND r.Name IN (
      'Maintenance Manager', 
      'Property & Facilities Manager',
      'Housing Supervisor',
      'Letting Officer',
      'Client Services Officer'
  )
ORDER BY r.Name, c.FullName;
GO
