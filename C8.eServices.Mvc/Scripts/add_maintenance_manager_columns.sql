-- Add MaintenanceManagerId to PreferredComplexArea table
-- Note: PropertyFacilitiesManager is system-wide (AppSettings only), not per-complex

-- Step 1: Check if column already exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PreferredComplexAreas]') AND name = 'MaintenanceManagerId')
BEGIN
    ALTER TABLE [dbo].[PreferredComplexAreas]
    ADD [MaintenanceManagerId] INT NULL;

    PRINT 'Column MaintenanceManagerId added to PreferredComplexAreas';
END
ELSE
BEGIN
    PRINT 'Column MaintenanceManagerId already exists in PreferredComplexAreas';
END
GO

-- Step 2: Add foreign key constraint (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PreferredComplexAreas_Customers_MaintenanceManagerId')
BEGIN
    ALTER TABLE [dbo].[PreferredComplexAreas]
    ADD CONSTRAINT [FK_PreferredComplexAreas_Customers_MaintenanceManagerId]
    FOREIGN KEY ([MaintenanceManagerId]) REFERENCES [dbo].[Customers]([Id]);

    PRINT 'Foreign key FK_PreferredComplexAreas_Customers_MaintenanceManagerId added';
END
GO

-- Step 3: View current complex configuration
SELECT 
    pca.Id AS ComplexId,
    pca.Name AS ComplexName,
    pca.LettingOfficerId,
    lo.FullName AS LettingOfficerName,
    pca.HousingSuperId,
    hs.FullName AS HousingSupervisorName,
    pca.MaintenanceManagerId,
    mm.FullName AS MaintenanceManagerName
FROM PreferredComplexAreas pca
LEFT JOIN Customers lo ON lo.Id = pca.LettingOfficerId
LEFT JOIN Customers hs ON hs.Id = pca.HousingSuperId
LEFT JOIN Customers mm ON mm.Id = pca.MaintenanceManagerId
WHERE pca.IsActive = 1 AND pca.IsDeleted = 0
ORDER BY pca.Name;
GO

PRINT 'Script completed successfully';

