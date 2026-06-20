-- Backfill MaintenanceManagerId with placeholder user
-- Note: PropertyFacilitiesManager is system-wide (AppSettings only), not per-complex
-- IMPORTANT: Replace placeholder Customer ID with actual user ID before running

DECLARE @MaintenanceManagerId INT = NULL;  -- REPLACE WITH ACTUAL CUSTOMER ID

-- Step 1: Find users with Maintenance Manager role
-- You need to create these users first using the script: create_maintenance_roles_and_users.sql

SELECT 
    'Available Maintenance Manager Users:' AS Info,
    c.Id AS CustomerId,
    c.FullName,
    su.UserName,
    r.Name AS RoleName
FROM Customers c
JOIN SystemUsers su ON su.Id = c.SystemUserId
JOIN AspNetUsers anu ON anu.Id = su.UserId
JOIN AspNetUserRoles aur ON aur.UserId = anu.Id
JOIN AspNetRoles r ON r.Id = aur.RoleId
WHERE r.Name = 'Maintenance Manager'
  AND c.IsActive = 1 
  AND c.IsDeleted = 0;

-- Step 2: Set the Customer ID you want to use
-- IMPORTANT: Uncomment and set this after reviewing the users above
-- SET @MaintenanceManagerId = <CUSTOMER_ID_HERE>;

-- Step 3: Validate the selected user exists
IF @MaintenanceManagerId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Customers WHERE Id = @MaintenanceManagerId AND IsActive = 1 AND IsDeleted = 0)
    BEGIN
        RAISERROR('Invalid Maintenance Manager Customer ID: %d does not exist or is not active', 16, 1, @MaintenanceManagerId);
        RETURN;
    END
    ELSE
    BEGIN
        SELECT 'Maintenance Manager to be assigned:' AS Info, * 
        FROM Customers WHERE Id = @MaintenanceManagerId;
    END
END

-- Step 4: Update all complexes with the placeholder user
-- IMPORTANT: Remove the 1=0 condition after setting the Customer ID above
IF @MaintenanceManagerId IS NOT NULL
BEGIN
    IF 1=0 -- SAFETY: Remove this condition after setting Customer ID
    BEGIN
        UPDATE PreferredComplexAreas
        SET 
            MaintenanceManagerId = @MaintenanceManagerId,
            ModifiedDateTime = GETDATE()
        WHERE IsActive = 1 AND IsDeleted = 0;

        SELECT 
            CONCAT('Updated ', @@ROWCOUNT, ' complex areas with Maintenance Manager') AS Result;
    END
    ELSE
    BEGIN
        PRINT 'SAFETY CHECK: Set Customer ID and remove the 1=0 condition to execute the UPDATE';
    END
END
ELSE
BEGIN
    PRINT 'No Customer ID set. Please set @MaintenanceManagerId';
END
GO

-- Step 5: Verify the update
SELECT 
    pca.Id AS ComplexId,
    pca.Name AS ComplexName,
    pca.MaintenanceManagerId,
    mm.FullName AS MaintenanceManagerName,
    pca.ModifiedDateTime AS LastUpdated
FROM PreferredComplexAreas pca
LEFT JOIN Customers mm ON mm.Id = pca.MaintenanceManagerId
WHERE pca.IsActive = 1 AND pca.IsDeleted = 0
ORDER BY pca.Name;
GO

