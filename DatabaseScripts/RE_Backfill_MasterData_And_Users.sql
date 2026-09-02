USE [PropertyLeaseManagementRealEstate]
GO

-- 1. Insert Tokoza and other missing CCCs
SET IDENTITY_INSERT [dbo].[CCCs] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 10)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (10, 1, 'Tokoza', '90', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 11)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (11, 1, 'Tsakane', '91', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 12)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (12, 1, 'Kwa - Thema', '92', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 13)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (13, 1, 'Daveyton', '93', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 14)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (14, 1, 'Etwatwa', '94', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 15)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (15, 1, 'Thembisa', '95', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CCCs] WHERE Id = 16)
    INSERT INTO [dbo].[CCCs] (Id, CCCTypeId, CCCName, Prefix, IsActive, IsDeleted)
    VALUES (16, 1, 'Katlehong', '96', 1, 0);
SET IDENTITY_INSERT [dbo].[CCCs] OFF;
GO

-- 2. Insert Real Estate Facility Categories
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_industrial_parks')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_industrial_parks', 'Township Industrial Parks', 57.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_business_hubs')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_business_hubs', 'Township Business Hubs & Skills Centre', 57.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_automotive_hubs')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('township_automotive_hubs', 'Township Automotive Hubs.', 67.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'fablab_facilities')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('fablab_facilities', 'FabLab Facilities', 36.00, 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'incubation_farms')
    INSERT INTO [dbo].[RE_FacilityCategories] ([Key], [Name], [TariffPerSqm], [IsActive], [IsDeleted], [IsLocked])
    VALUES ('incubation_farms', 'Incubation Farms and/or Agri-parks', 0.70, 1, 0, 0);
GO

-- 3. Insert Real Estate Facilities
SET IDENTITY_INSERT [dbo].[RE_Facilities] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 1)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (1, 'Fannie Malape Co-operatives Industrial Hive Centre', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 2)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (2, 'Tokoza Traders Market', 10, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 3)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (3, 'Tsakane Business Park', 11, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 4)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (4, 'KwaThema Business Park', 12, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 5)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (5, 'Springs Traders Market', 4, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 6)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (6, 'Brakpan Civic Centre Kiosk', 6, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 7)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (7, 'Oscar Mabika Co-operatives Industrial Hive Centre', 13, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 8)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (8, 'Barcelona Traders Market', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 9)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (9, 'Etwatwa Business Hive', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 10)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (10, 'Etwatwa Unserviced Portions', 14, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 11)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (11, 'Bomba Sibiya Co-ops Industrial Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 12)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (12, 'Tembisa Business Park', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 13)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (13, 'Motsu Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 14)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (14, 'Sethokga Buy Back Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 15)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (15, 'Sethokga Traders Market', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 16)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (16, 'Sedibeng Hive Centre', 15, '', 1, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_Facilities] WHERE Id = 17)
    INSERT INTO [dbo].[RE_Facilities] (Id, Name, CCCId, Address, IsActive, IsDeleted)
    VALUES (17, 'Katlehong Automotive manufacturing hub', 16, '', 1, 0);
SET IDENTITY_INSERT [dbo].[RE_Facilities] OFF;
GO

-- 4. Insert Real Estate Facility Units
DECLARE @CatParks INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_industrial_parks');
DECLARE @CatHubs INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_business_hubs');
DECLARE @CatAuto INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'township_automotive_hubs');
DECLARE @CatFab INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'fablab_facilities');
DECLARE @CatFarms INT = (SELECT Id FROM [dbo].[RE_FacilityCategories] WHERE [Key] = 'incubation_farms');

SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] ON;

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 1)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (1, 1, @CatParks, 'Indoor Unit', 65.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 2)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (2, 2, @CatHubs, 'Indoor Unit', 65.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 3)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (3, 3, @CatHubs, 'Offices', 10.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 4)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (4, 3, @CatHubs, 'Single Garage Size Units', 18.00, 10, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 5)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (5, 3, @CatHubs, 'Double Garage Size Units', 36.00, 10, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 6)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (6, 3, @CatHubs, 'Kiosk', 17.00, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 7)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (7, 4, @CatHubs, 'Indoor Unit (Block A)', 21.00, 32, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 8)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (8, 4, @CatHubs, 'Indoor Unit (Block B)', 42.50, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 9)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (9, 5, @CatHubs, 'Small Shop', 20.00, 21, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 10)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (10, 6, @CatHubs, 'Kiosk', 17.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 11)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (11, 7, @CatHubs, 'Offices', 24.00, 8, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 12)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (12, 7, @CatHubs, 'Indoor Unit', 80.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 13)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (13, 7, @CatHubs, 'Outdoor Roof Covered Units', 70.00, 4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 14)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (14, 7, @CatHubs, 'Boardroom', 26.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 15)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (15, 8, @CatHubs, 'Small Shop', 20.00, 21, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 16)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (16, 9, @CatHubs, 'Indoor Unit (35m²)', 35.00, 12, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 17)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (17, 9, @CatHubs, 'Kiosk', 17.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 18)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (18, 9, @CatHubs, 'Indoor Unit (34m²)', 34.00, 27, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 19)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (19, 9, @CatHubs, 'Indoor Unit (57m²)', 57.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 20)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (20, 9, @CatHubs, 'Admin Block with Boardroom', 173.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 21)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (21, 10, @CatFarms, 'Outdoor Plot (4300m²)', 4300.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 22)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (22, 10, @CatFarms, 'Outdoor Plot (4840m²)', 4840.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 23)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (23, 10, @CatFarms, 'Outdoor Plot (4100m²)', 4100.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 24)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (24, 10, @CatFarms, 'Outdoor Plot (3100m²)', 3100.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 25)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (25, 11, @CatHubs, 'Indoor Unit', 40.00, 7, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 26)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (26, 11, @CatHubs, 'Offices', 13.00, 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 27)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (27, 12, @CatHubs, 'Offices', 13.00, 18, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 28)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (28, 13, @CatAuto, 'Workshop', 322.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 29)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (29, 14, @CatHubs, 'Offices', 14.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 30)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (30, 14, @CatAuto, 'Workshop', 50.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 31)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (31, 14, @CatFab, 'Roof Covered Sorting Area', 11.00, 5, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 32)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (32, 15, @CatHubs, 'Open Stalls', 6.00, 22, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 33)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (33, 15, @CatHubs, 'Lockable Stalls', 12.00, 18, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 34)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (34, 16, @CatAuto, 'Workshop', 85.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 35)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (35, 16, @CatHubs, 'Kitchen', 7.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 36)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (36, 16, @CatHubs, 'Offices', 24.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 37)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (37, 17, @CatAuto, 'Unit A01 Workshop', 128.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 38)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (38, 17, @CatAuto, 'Unit A02 Workshop', 128.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 39)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (39, 17, @CatAuto, 'Unit A03 Workshop', 117.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 40)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (40, 17, @CatAuto, 'Unit A04 Workshop', 117.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 41)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (41, 17, @CatAuto, 'Unit A05 Workshop', 150.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 42)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (42, 17, @CatAuto, 'Unit A06 Workshop', 30.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 43)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (43, 17, @CatHubs, 'Offices', 30.00, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM [dbo].[RE_FacilityUnits] WHERE Id = 44)
    INSERT INTO [dbo].[RE_FacilityUnits] (Id, FacilityId, FacilityCategoryId, UnitType, UnitSize, MaxUnits, IsActive, IsDeleted)
    VALUES (44, 17, @CatHubs, 'Canteen', 30.00, 1, 1, 0);

SET IDENTITY_INSERT [dbo].[RE_FacilityUnits] OFF;
GO

-- 5. Insert missing Caretaker role
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Caretaker')
    INSERT INTO AspNetRoles (Id, Name) VALUES ('e141a06a-68a8-4e76-a06b-c0da7682dc06', 'Caretaker');
GO

-- 6. Insert Users and assign them to roles
DECLARE @Role_Customer NVARCHAR(128) = '9524ba14-6dea-44af-b2e4-c94f8980a412';
DECLARE @Role_Finance NVARCHAR(128) = '95660a1d-c242-49cc-a549-1097a5a419f9';
DECLARE @Role_Property NVARCHAR(128) = 'c23949ee-e73d-4d87-a466-6a47b28bd0d3';
DECLARE @Role_Area NVARCHAR(128) = 'cc8e81f0-8440-4740-a746-e51a9f49d68d';
DECLARE @Role_BackOffice NVARCHAR(128) = '90103409-5e37-493f-b80a-20e216178cba';
DECLARE @Role_FacManager NVARCHAR(128) = 'ADDBE943-6C29-47C6-A69A-CFDE77F6FE4D';
DECLARE @Role_Technician NVARCHAR(128) = 'e141a06a-68a8-4e76-a06b-c0da7682dc06';

DECLARE @PwdHash NVARCHAR(MAX) = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA==';
DECLARE @SecurityStamp NVARCHAR(MAX) = '669cc746-a1ef-46c6-afe1-2ffa3053b5e0';

DECLARE @AspNetUserId NVARCHAR(128);
DECLARE @SystemUserId INT;

-- 1. RealEstateCustomer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'RealEstateCustomer')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c1';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 'RealEstateCustomer', @PwdHash, @SecurityStamp, 'realestate@test.com', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Real', 'Estate', 'RealEstateCustomer', 'realestate@test.com', 1, 0, 1, 0, 0);
    SET @SystemUserId = SCOPE_IDENTITY();

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Customer);

    DECLARE @StatusId INT = (SELECT Id FROM Status WHERE [Key] = 's_customer_active');
    DECLARE @CustomerTypeId INT = (SELECT Id FROM CustomerTypes WHERE [Key] = 'ct_individual');
    DECLARE @IdTypeId INT = (SELECT Id FROM IdentificationTypes WHERE [Key] = 'id_south_african');

    INSERT INTO Customers (CustomerTypeId, IdentificationTypeId, IdentificationNumber, TitleTypeId, FirstName, LastName, EmailAddress, SystemUserId, StatusId, IsActive, IsDeleted)
    VALUES (@CustomerTypeId, @IdTypeId, '9001015000088', 1, 'Real', 'Estate', 'realestate@test.com', @SystemUserId, @StatusId, 1, 0);
END

-- 2. re_finance_officer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_finance_officer')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c2';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_finance_officer', @PwdHash, @SecurityStamp, 'finance_officer@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Finance', 'Officer', 're_finance_officer', 'finance_officer@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Finance);
END

-- 3. re_property_officer
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_property_officer')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c3';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_property_officer', @PwdHash, @SecurityStamp, 'property_officer@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Property', 'Officer', 're_property_officer', 'property_officer@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Property);
END

-- 4. re_committee_member
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_committee_member')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c4';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_committee_member', @PwdHash, @SecurityStamp, 'committee_member@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Committee', 'Member', 're_committee_member', 'committee_member@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Area);
END

-- 5. re_hod
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_hod')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c5';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_hod', @PwdHash, @SecurityStamp, 'hod_realestate@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('HOD', 'RealEstate', 're_hod', 'hod_realestate@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_BackOffice);
END

-- 6. re_facilities_manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_facilities_manager')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c6';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_facilities_manager', @PwdHash, @SecurityStamp, 'fac_manager@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Facilities', 'Manager', 're_facilities_manager', 'fac_manager@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_FacManager);
END

-- 7. re_technician
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 're_technician')
BEGIN
    SET @AspNetUserId = 'a001a001-beef-4c7b-a2fc-b04afb57b3c7';
    INSERT INTO AspNetUsers (Id, UserName, PasswordHash, SecurityStamp, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (@AspNetUserId, 're_technician', @PwdHash, @SecurityStamp, 'technician@siyakhokha-test.gov.za', 1, 0, 0, 1, 0);

    INSERT INTO SystemUsers (FirstName, LastName, UserName, EmailAddress, IsActive, IsDeleted, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
    VALUES ('Technician', 'Electrician', 're_technician', 'technician@siyakhokha-test.gov.za', 1, 0, 1, 0, 1);

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AspNetUserId, @Role_Technician);
END
GO

-- ====================================================================================
-- Additional Setup: Vetting Departments, Roles, Representative Users and Mappings
-- ====================================================================================

-- A. Ensure Real Estate Roles exist in AspNetRoles
PRINT 'Checking and inserting AspNetRoles...';
DECLARE @roles TABLE (Id NVARCHAR(128), Name NVARCHAR(256))
INSERT INTO @roles (Id, Name) VALUES
('r_finance_admin', 'Finance Administrator'),
('r_property_manager', 'Property Manager'),
('r_area_manager', 'Area Manager'),
('r_bo_sys_admin', 'Back Office System Administrator'),
('r_prop_fac_manager', 'Property & Facilities Manager'),
('r_caretaker', 'Caretaker'),
('r_dept_rep', 'Departmental Representative')

INSERT INTO dbo.AspNetRoles (Id, Name)
SELECT r.Id, r.Name
FROM @roles r
WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = r.Name);

-- B. Define Representative Users variables
DECLARE @PasswordHash NVARCHAR(MAX) = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA=='; -- Arsenal5@
DECLARE @SecurityStamp NVARCHAR(MAX) = 'd3a4b64b-b0b3-46d5-86f7-c57388df2cb1';

-- Table to hold user definitions
DECLARE @users TABLE (
    UserName NVARCHAR(256),
    Email NVARCHAR(256),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    RoleName NVARCHAR(256)
)

-- Seed individual department representative users (Assigned Departmental Representative role)
INSERT INTO @users (UserName, Email, FirstName, LastName, RoleName) VALUES
('re_city_planning', 're_planning@ekurhuleni.gov.za', 'City Planning', 'Representative', 'Departmental Representative'),
('re_legal', 're_legal@ekurhuleni.gov.za', 'Legal Services', 'Representative', 'Departmental Representative'),
('re_disaster', 're_disaster@ekurhuleni.gov.za', 'Disaster Management', 'Representative', 'Departmental Representative'),
('re_economic', 're_economic@ekurhuleni.gov.za', 'Economic Dev', 'Representative', 'Departmental Representative'),
('re_empd', 're_empd@ekurhuleni.gov.za', 'EMPD', 'Representative', 'Departmental Representative'),
('re_energy', 're_energy@ekurhuleni.gov.za', 'Energy', 'Representative', 'Departmental Representative'),
('re_environmental', 're_environmental@ekurhuleni.gov.za', 'Environmental', 'Representative', 'Departmental Representative'),
('re_health', 're_health@ekurhuleni.gov.za', 'Health Dev', 'Representative', 'Departmental Representative'),
('re_human_settlements', 're_human_settlements@ekurhuleni.gov.za', 'Human Settlements', 'Representative', 'Departmental Representative'),
('re_ict', 're_ict@ekurhuleni.gov.za', 'ICT', 'Representative', 'Departmental Representative'),
('re_roads', 're_roads@ekurhuleni.gov.za', 'Roads & Stormwater', 'Representative', 'Departmental Representative'),
('re_sports', 're_sports@ekurhuleni.gov.za', 'Sports & Rec', 'Representative', 'Departmental Representative'),
('re_transport', 're_transport@ekurhuleni.gov.za', 'Transport Planning', 'Representative', 'Departmental Representative');

-- Loop through and setup each user
DECLARE @usrName NVARCHAR(256), @email NVARCHAR(256), @fName NVARCHAR(100), @lName NVARCHAR(100), @roleName NVARCHAR(256)
DECLARE @aspNetId NVARCHAR(128), @systemUserId INT, @customerId INT, @roleId NVARCHAR(128)

DECLARE user_cursor CURSOR FOR 
SELECT UserName, Email, FirstName, LastName, RoleName FROM @users

OPEN user_cursor
FETCH NEXT FROM user_cursor INTO @usrName, @email, @fName, @lName, @roleName

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Setting up user: ' + @usrName;

    -- 1. Insert SystemUsers first if not exists (Set DepartmentId = 3 for Real Estate Development)
    IF NOT EXISTS (SELECT 1 FROM dbo.SystemUsers WHERE UserName = @usrName)
    BEGIN
        INSERT INTO dbo.SystemUsers (UserName, FirstName, LastName, EmailAddress, MobileNumber, DepartmentId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, isInternalUser, isActiveDirectoryUser, IsPasswordReset, IsTemporaryPassword, IsIAMRegistered)
        VALUES (@usrName, @fName, @lName, @email, '0119990000', 3, 1, 0, 0, 1, GETDATE(), 1, 0, 1, 0, 0);
        SET @systemUserId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT TOP 1 @systemUserId = Id FROM dbo.SystemUsers WHERE UserName = @usrName ORDER BY Id DESC;
        UPDATE dbo.SystemUsers SET DepartmentId = 3, isInternalUser = 1 WHERE Id = @systemUserId;
    END

    -- 2. Insert AspNetUsers if not exists, referencing @systemUserId
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE UserName = @usrName)
    BEGIN
        SET @aspNetId = NEWID();
        INSERT INTO dbo.AspNetUsers (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, SystemUserId, isInternalUser, isActiveDirectoryUser, isDeleted)
        VALUES (@aspNetId, @email, 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, @usrName, @systemUserId, 1, 0, 0);
    END
    ELSE
    BEGIN
        SELECT TOP 1 @aspNetId = Id FROM dbo.AspNetUsers WHERE UserName = @usrName ORDER BY Id DESC;
    END

    -- 3. Insert Customers (Back Office Clerk Profile) if not exists
    IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE SystemUserId = @systemUserId)
    BEGIN
        INSERT INTO dbo.Customers (SystemUserId, FirstName, LastName, EmailAddress, CellPhoneNumber, DepartmentId, IsActive, IsDeleted, IsLocked, CustomerTypeId, PhysicalAddressCode, PostalAddressCode, StatusId, CreatedBySystemUserId, CreatedDateTime)
        VALUES (@systemUserId, @fName, @lName, @email, '0119990000', 3, 1, 0, 0, 5, '0000', '0000', 1, @systemUserId, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE dbo.Customers SET DepartmentId = 3 WHERE SystemUserId = @systemUserId;
    END

    -- 4. Assign Role in AspNetUserRoles
    SELECT TOP 1 @roleId = Id FROM dbo.AspNetRoles WHERE Name = @roleName ORDER BY Id DESC;
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @aspNetId AND RoleId = @roleId)
    BEGIN
        INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@aspNetId, @roleId);
    END

    FETCH NEXT FROM user_cursor INTO @usrName, @email, @fName, @lName, @roleName
END

CLOSE user_cursor
DEALLOCATE user_cursor;

-- C. Assign Departmental Representative role as secondary role to key representatives who act for departments
PRINT 'Assigning secondary Departmental Representative roles...';
DECLARE @deptRepRoleId NVARCHAR(128)
SELECT @deptRepRoleId = Id FROM dbo.AspNetRoles WHERE Name = 'Departmental Representative';

-- finance officer
DECLARE @finUserId NVARCHAR(128)
SELECT TOP 1 @finUserId = Id FROM dbo.AspNetUsers WHERE UserName = 're_finance_officer' ORDER BY Id DESC;
IF @finUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @finUserId AND RoleId = @deptRepRoleId)
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@finUserId, @deptRepRoleId);

-- property officer
DECLARE @propUserId NVARCHAR(128)
SELECT TOP 1 @propUserId = Id FROM dbo.AspNetUsers WHERE UserName = 're_property_officer' ORDER BY Id DESC;
IF @propUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @propUserId AND RoleId = @deptRepRoleId)
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@propUserId, @deptRepRoleId);

-- committee member
DECLARE @commUserId NVARCHAR(128)
SELECT TOP 1 @commUserId = Id FROM dbo.AspNetUsers WHERE UserName = 're_committee_member' ORDER BY Id DESC;
IF @commUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @commUserId AND RoleId = @deptRepRoleId)
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@commUserId, @deptRepRoleId);

-- D. Cleanup any incorrect/old roles for pure department representative users
PRINT 'Cleaning up old/incorrect roles for pure department representative users...';
DELETE ur
FROM dbo.AspNetUserRoles ur
JOIN dbo.AspNetUsers u ON ur.UserId = u.Id
JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName IN ('re_city_planning', 're_legal', 're_disaster', 're_economic', 're_empd', 're_energy', 're_environmental', 're_health', 're_human_settlements', 're_ict', 're_roads', 're_sports', 're_transport')
  AND r.Name = 'Property Manager';

-- E. Configure Departments CoEs vetting departments and map to the representative users
PRINT 'Configuring Vetting Departments mapping...';

-- Add columns to DepartmentsCoEs if not exists
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DepartmentsCoEs') AND name = 'RepresentativeSystemUserId')
BEGIN
    ALTER TABLE dbo.DepartmentsCoEs ADD RepresentativeSystemUserId INT NULL;
    PRINT 'Altered Table: DepartmentsCoEs - Added RepresentativeSystemUserId';
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DepartmentsCoEs') AND name = 'RepresentedBy')
BEGIN
    ALTER TABLE dbo.DepartmentsCoEs ADD RepresentedBy NVARCHAR(250) NULL;
    PRINT 'Altered Table: DepartmentsCoEs - Added RepresentedBy';
END;
GO

-- Map vetting departments to their specific representative users
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_city_planning' ORDER BY Id DESC), RepresentedBy = 'City Planning Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'City Planning';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_legal' ORDER BY Id DESC), RepresentedBy = 'Legal Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Corporate Legal Services';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_disaster' ORDER BY Id DESC), RepresentedBy = 'Disaster Management Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Disaster and Emergency Management Services';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_economic' ORDER BY Id DESC), RepresentedBy = 'Economic Dev Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Economic Development';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_empd' ORDER BY Id DESC), RepresentedBy = 'EMPD Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Ekurhuleni Metro Police Department';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_energy' ORDER BY Id DESC), RepresentedBy = 'Energy Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Energy';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_environmental' ORDER BY Id DESC), RepresentedBy = 'Environmental Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Environmental Resource and Waste Management';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_health' ORDER BY Id DESC), RepresentedBy = 'Health Dev Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Health and Social Development';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_human_settlements' ORDER BY Id DESC), RepresentedBy = 'Human Settlements Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Human Settlements';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_ict' ORDER BY Id DESC), RepresentedBy = 'ICT Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Information and Communication Technology';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_roads' ORDER BY Id DESC), RepresentedBy = 'Roads Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Roads and Stormwater';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_sports' ORDER BY Id DESC), RepresentedBy = 'Sports Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Sports, Recreation Arts and Culture';
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_transport' ORDER BY Id DESC), RepresentedBy = 'Transport Representative', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Transport Planning and Provision';

-- Finance Department mapped to Finance Officer
UPDATE dbo.DepartmentsCoEs SET RepresentativeSystemUserId = (SELECT TOP 1 Id FROM dbo.SystemUsers WHERE UserName = 're_finance_officer' ORDER BY Id DESC), RepresentedBy = 'Finance Officer', IsActive = 1, IsDeleted = 0 WHERE DepartmentName = 'Finance';

PRINT 'Real Estate Workflow Setup Mappings successfully completed!';
GO
