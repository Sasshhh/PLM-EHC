-- ==============================================================================
-- REAL ESTATE DEVELOPMENT - MASTER DATA SCHEMA & SEED MIGRATION
-- ==============================================================================
PRINT '--- STARTING REAL ESTATE SCHEMA MIGRATION ---';

-- 1. Create RE_FacilityCategories table (Tariff Grades)
IF OBJECT_ID('dbo.RE_FacilityCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_FacilityCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [Key] NVARCHAR(100) NOT NULL UNIQUE,
        Name NVARCHAR(250) NOT NULL,
        TariffPerSqm DECIMAL(18,2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL
    );
    PRINT 'Created Table: RE_FacilityCategories';
END;

-- 2. Create RE_Facilities table (Physical Properties)
IF OBJECT_ID('dbo.RE_Facilities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_Facilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(250) NOT NULL,
        CCCId INT NOT NULL,
        Address NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL,
        CONSTRAINT FK_RE_Facilities_CCCs FOREIGN KEY (CCCId) REFERENCES dbo.CCCs (Id)
    );
    PRINT 'Created Table: RE_Facilities';
END;

-- 3. Create RE_FacilityUnits table (Granular Unit Spaces)
IF OBJECT_ID('dbo.RE_FacilityUnits', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RE_FacilityUnits (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FacilityId INT NOT NULL,
        FacilityCategoryId INT NOT NULL,
        UnitType NVARCHAR(150) NOT NULL,
        UnitSize DECIMAL(18,2) NOT NULL,
        MaxUnits INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedBySystemUserId INT NULL,
        CreatedDateTime DATETIME NULL,
        ModifiedBySystemUserId INT NULL,
        ModifiedDateTime DATETIME NULL,
        CONSTRAINT FK_RE_FacilityUnits_Facilities FOREIGN KEY (FacilityId) REFERENCES dbo.RE_Facilities (Id),
        CONSTRAINT FK_RE_FacilityUnits_FacilityCategories FOREIGN KEY (FacilityCategoryId) REFERENCES dbo.RE_FacilityCategories (Id)
    );
    PRINT 'Created Table: RE_FacilityUnits';
END;

-- 4. Alter RE_Applications table to store selected units and dynamic cost calculations
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RE_Applications') AND name = 'SelectedFacilityId')
BEGIN
    ALTER TABLE dbo.RE_Applications ADD SelectedFacilityId INT NULL;
    ALTER TABLE dbo.RE_Applications ADD CONSTRAINT FK_RE_Applications_Facilities FOREIGN KEY (SelectedFacilityId) REFERENCES dbo.RE_Facilities (Id);
    PRINT 'Altered RE_Applications: Added SelectedFacilityId';
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RE_Applications') AND name = 'SelectedFacilityUnitId')
BEGIN
    ALTER TABLE dbo.RE_Applications ADD SelectedFacilityUnitId INT NULL;
    ALTER TABLE dbo.RE_Applications ADD CONSTRAINT FK_RE_Applications_FacilityUnits FOREIGN KEY (SelectedFacilityUnitId) REFERENCES dbo.RE_FacilityUnits (Id);
    PRINT 'Altered RE_Applications: Added SelectedFacilityUnitId';
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RE_Applications') AND name = 'SelectedUnitCount')
BEGIN
    ALTER TABLE dbo.RE_Applications ADD SelectedUnitCount INT NULL;
    PRINT 'Altered RE_Applications: Added SelectedUnitCount';
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RE_Applications') AND name = 'CalculatedMonthlyRental')
BEGIN
    ALTER TABLE dbo.RE_Applications ADD CalculatedMonthlyRental DECIMAL(18,2) NULL;
    PRINT 'Altered RE_Applications: Added CalculatedMonthlyRental';
END;


-- ==============================================================================
-- SEED DATA MIGRATION
-- ==============================================================================
PRINT '--- SEEDING MASTER DATA ---';

-- A. Seed missing Customer Care Centres (CCCs) from the spreadsheet
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Tokoza')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Tokoza', '90', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Tsakane')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Tsakane', '91', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Kwa - Thema')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Kwa - Thema', '92', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Daveyton')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Daveyton', '93', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Etwatwa')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Etwatwa', '94', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Thembisa')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Thembisa', '95', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.CCCs WHERE CCCName = 'Katlehong')
    INSERT INTO dbo.CCCs (CCCTypeId, CCCName, Prefix, IsActive, IsDeleted) VALUES (1, 'Katlehong', '96', 1, 0);

PRINT 'Seeded CCCs.';

-- B. Seed base Category Tariffs (Schedule 21 Table 3)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_industrial_parks')
    INSERT INTO dbo.RE_FacilityCategories ([Key], Name, TariffPerSqm) VALUES ('township_industrial_parks', 'Township Industrial Parks', 57.00);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_business_hubs')
    INSERT INTO dbo.RE_FacilityCategories ([Key], Name, TariffPerSqm) VALUES ('township_business_hubs', 'Township Business Hubs & Skills Centre', 57.00);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_automotive_hubs')
    INSERT INTO dbo.RE_FacilityCategories ([Key], Name, TariffPerSqm) VALUES ('township_automotive_hubs', 'Township Automotive Hubs.', 67.00);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityCategories WHERE [Key] = 'fablab_facilities')
    INSERT INTO dbo.RE_FacilityCategories ([Key], Name, TariffPerSqm) VALUES ('fablab_facilities', 'FabLab Facilities', 36.00);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityCategories WHERE [Key] = 'incubation_farms')
    INSERT INTO dbo.RE_FacilityCategories ([Key], Name, TariffPerSqm) VALUES ('incubation_farms', 'Incubation Farms and/or Agri-parks', 0.70);

PRINT 'Seeded Facility Categories.';

-- C. Seeding Facilities & Dynamic Units
DECLARE @CatIndustrial INT = (SELECT Id FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_industrial_parks');
DECLARE @CatBusiness INT = (SELECT Id FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_business_hubs');
DECLARE @CatAutomotive INT = (SELECT Id FROM dbo.RE_FacilityCategories WHERE [Key] = 'township_automotive_hubs');
DECLARE @CatFabLab INT = (SELECT Id FROM dbo.RE_FacilityCategories WHERE [Key] = 'fablab_facilities');
DECLARE @CatAgri INT = (SELECT Id FROM dbo.RE_FacilityCategories WHERE [Key] = 'incubation_farms');

DECLARE @FacId INT;
DECLARE @CCCId INT;

-- 1. Fannie Malape (Tokoza)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Tokoza');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Fannie Malape Co-operatives Industrial Hive Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Fannie Malape Co-operatives Industrial Hive Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Fannie Malape Co-operatives Industrial Hive Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit' AND UnitSize = 65.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatIndustrial, 'Indoor Unit', 65.0, 4);

-- 2. Tokoza Traders Market (Tokoza)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Tokoza Traders Market' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Tokoza Traders Market', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Tokoza Traders Market' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit' AND UnitSize = 65.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit', 65.0, 4);

-- 3. Tsakane Business Park (Tsakane)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Tsakane');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Tsakane Business Park' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Tsakane Business Park', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Tsakane Business Park' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 10.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 10.0, 5);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Single Garage Size Units' AND UnitSize = 18.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Single Garage Size Units', 18.0, 10);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Double Garage Size Units' AND UnitSize = 36.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Double Garage Size Units', 36.0, 10);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Kiosk' AND UnitSize = 17.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Kiosk', 17.0, 12);

-- 4. KwaThema Business Park (Kwa - Thema)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Kwa - Thema');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'KwaThema Business Park' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('KwaThema Business Park', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'KwaThema Business Park' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit (Block A)' AND UnitSize = 21.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit (Block A)', 21.0, 32);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit (Block B)' AND UnitSize = 42.5)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit (Block B)', 42.5, 12);

-- 5. Springs Traders Market (Springs)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Springs');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Springs Traders Market' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Springs Traders Market', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Springs Traders Market' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Small Shop' AND UnitSize = 20.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Small Shop', 20.0, 21);

-- 6. Brakpan Civic Centre Kiosk (Brakpan)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Brakpan');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Brakpan Civic Centre Kiosk' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Brakpan Civic Centre Kiosk', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Brakpan Civic Centre Kiosk' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Kiosk' AND UnitSize = 17.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Kiosk', 17.0, 1);

-- 7. Oscar Mabika Co-operatives Industrial Hive Centre (Daveyton)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Daveyton');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Oscar Mabika Co-operatives Industrial Hive Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Oscar Mabika Co-operatives Industrial Hive Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Oscar Mabika Co-operatives Industrial Hive Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 24.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 24.0, 8);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit' AND UnitSize = 80.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit', 80.0, 3);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Outdoor Roof Covered Units' AND UnitSize = 70.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Outdoor Roof Covered Units', 70.0, 4);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Boardroom' AND UnitSize = 26.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Boardroom', 26.0, 1);

-- 8. Barcelona Traders Market (Etwatwa)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Etwatwa');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Barcelona Traders Market' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Barcelona Traders Market', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Barcelona Traders Market' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Small Shop' AND UnitSize = 20.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Small Shop', 20.0, 21);

-- 9. Etwatwa Business Hive (Etwatwa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Etwatwa Business Hive' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Etwatwa Business Hive', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Etwatwa Business Hive' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit (35m²)' AND UnitSize = 35.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit (35m²)', 35.0, 12);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Kiosk' AND UnitSize = 17.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Kiosk', 17.0, 3);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit (34m²)' AND UnitSize = 34.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit (34m²)', 34.0, 27);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit (57m²)' AND UnitSize = 57.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit (57m²)', 57.0, 5);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Admin Block with Boardroom' AND UnitSize = 173.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Admin Block with Boardroom', 173.0, 1);

-- 10. Etwatwa Unserviced Portions (Etwatwa - Agri)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Etwatwa Unserviced Portions' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Etwatwa Unserviced Portions', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Etwatwa Unserviced Portions' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Outdoor Plot (4300m²)' AND UnitSize = 4300.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAgri, 'Outdoor Plot (4300m²)', 4300.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Outdoor Plot (4840m²)' AND UnitSize = 4840.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAgri, 'Outdoor Plot (4840m²)', 4840.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Outdoor Plot (4100m²)' AND UnitSize = 4100.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAgri, 'Outdoor Plot (4100m²)', 4100.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Outdoor Plot (3100m²)' AND UnitSize = 3100.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAgri, 'Outdoor Plot (3100m²)', 3100.0, 1);

-- 11. Bomba Sibiya Co-ops Industrial Hive Centre (Thembisa)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Thembisa');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Bomba Sibiya Co-ops Industrial Hive Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Bomba Sibiya Co-ops Industrial Hive Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Bomba Sibiya Co-ops Industrial Hive Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Indoor Unit' AND UnitSize = 40.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Indoor Unit', 40.0, 7);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 13.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 13.0, 3);

-- 12. Tembisa Business Park (Thembisa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Tembisa Business Park' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Tembisa Business Park', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Tembisa Business Park' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 13.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 13.0, 18);

-- 13. Motsu Buy Back Centre (Thembisa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Motsu Buy Back Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Motsu Buy Back Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Motsu Buy Back Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Workshop' AND UnitSize = 322.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Workshop', 322.0, 1);

-- 14. Sethokga Buy Back Centre (Thembisa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Sethokga Buy Back Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Sethokga Buy Back Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Sethokga Buy Back Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 14.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 14.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Workshop' AND UnitSize = 50.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Workshop', 50.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Roof Covered Sorting Area' AND UnitSize = 11.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatFabLab, 'Roof Covered Sorting Area', 11.0, 5);

-- 15. Sethokga Traders Market (Thembisa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Sethokga Traders Market' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Sethokga Traders Market', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Sethokga Traders Market' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Open Stalls' AND UnitSize = 6.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Open Stalls', 6.0, 22);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Lockable Stalls' AND UnitSize = 12.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Lockable Stalls', 12.0, 18);

-- 16. Sedibeng Hive Centre (Thembisa)
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Sedibeng Hive Centre' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Sedibeng Hive Centre', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Sedibeng Hive Centre' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Workshop' AND UnitSize = 85.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Workshop', 85.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Kitchen' AND UnitSize = 7.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Kitchen', 7.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 24.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 24.0, 1);

-- 17. Katlehong Automotive manufacturing hub (Katlehong)
SET @CCCId = (SELECT Id FROM dbo.CCCs WHERE CCCName = 'Katlehong');
IF NOT EXISTS (SELECT 1 FROM dbo.RE_Facilities WHERE Name = 'Katlehong Automotive manufacturing hub' AND CCCId = @CCCId)
    INSERT INTO dbo.RE_Facilities (Name, CCCId) VALUES ('Katlehong Automotive manufacturing hub', @CCCId);
SET @FacId = (SELECT Id FROM dbo.RE_Facilities WHERE Name = 'Katlehong Automotive manufacturing hub' AND CCCId = @CCCId);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A01 Workshop' AND UnitSize = 128.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A01 Workshop', 128.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A02 Workshop' AND UnitSize = 128.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A02 Workshop', 128.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A03 Workshop' AND UnitSize = 117.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A03 Workshop', 117.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A04 Workshop' AND UnitSize = 117.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A04 Workshop', 117.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A05 Workshop' AND UnitSize = 150.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A05 Workshop', 150.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Unit A06 Workshop' AND UnitSize = 30.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatAutomotive, 'Unit A06 Workshop', 30.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Offices' AND UnitSize = 30.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Offices', 30.0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.RE_FacilityUnits WHERE FacilityId = @FacId AND UnitType = 'Canteen' AND UnitSize = 30.0)
    INSERT INTO dbo.RE_FacilityUnits (FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits) VALUES (@FacId, @CatBusiness, 'Canteen', 30.0, 1);

PRINT 'Seeded Facilities & Facility Units.';

PRINT '--- REAL ESTATE SCHEMA MIGRATION COMPLETE ---';
