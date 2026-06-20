-- ==============================================================================
-- Script to generate 100 dummy units for Airport Park - 1 Bedroom (Auto-Matching)
-- With randomized/varying test data for size, rental, deposits, and requirements
-- ==============================================================================

DECLARE @ComplexName NVARCHAR(100) = 'Airport Park';
DECLARE @UnitTypeName NVARCHAR(100) = '1 Bedroom'; 
DECLARE @TotalUnitsToCreate INT = 100;
DECLARE @Counter INT = 1;

-- 1. Look up the ID for the Complex "Airport Park"
DECLARE @ComplexId INT;
SELECT TOP 1 @ComplexId = Id 
FROM PreferredComplexAreas 
WHERE Name LIKE '%' + @ComplexName + '%' AND IsActive = 1 AND IsDeleted = 0;

IF @ComplexId IS NULL
BEGIN
    PRINT 'Error: Could not find Complex Area matching: ' + @ComplexName;
    RETURN;
END

-- 2. Look up the ID for the Unit Type "1 Bedroom" in HumanEHCOptions
DECLARE @UnitTypeId INT;
SELECT TOP 1 @UnitTypeId = Id 
FROM HumanEHCOptions 
WHERE Name LIKE '%' + @UnitTypeName + '%' AND IsActive = 1 AND IsDeleted = 0;

IF @UnitTypeId IS NULL
BEGIN
    PRINT 'Error: Could not find Unit Type matching: ' + @UnitTypeName + ' in HumanEHCOptions';
    RETURN;
END

PRINT 'Found Complex ID: ' + CAST(@ComplexId AS VARCHAR) + ' and Unit Type ID: ' + CAST(@UnitTypeId AS VARCHAR);
PRINT 'Starting insertion of ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' units with varying data into ApplicationAllocatedProperties...';

-- 3. Loop to insert 100 dummy records
WHILE @Counter <= @TotalUnitsToCreate
BEGIN
    -- Generate random values for this row
    -- Size between 40 and 75
    DECLARE @RandomSize DECIMAL(10,2) = ROUND(RAND(CAST(NEWID() AS VARBINARY)) * 35 + 40, 2);
    -- Rental between 2800 and 4200
    DECLARE @RandomRental DECIMAL(10,2) = ROUND(RAND(CAST(NEWID() AS VARBINARY)) * 1400 + 2800, 2);
    -- Deposit is usually 1x or 1.5x rental
    DECLARE @RandomDeposit DECIMAL(10,2) = @RandomRental * (CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 1.0 ELSE 1.5 END);
    -- Random Requirements
    DECLARE @RandomRequirement NVARCHAR(50) = CASE ABS(CHECKSUM(NEWID())) % 3 
        WHEN 0 THEN 'Standard' 
        WHEN 1 THEN 'Strict - No Pets' 
        ELSE 'Flexible' END;
    
    INSERT INTO ApplicationAllocatedProperties (
        SpaceUnitSize,
        SpaceUnitNumber,
        SolarReference,
        MonthlyRentalAmount,
        RequiedDepositAmount,
        NumOfBeds,
        StreetName,
        Township,
        Postal,
        OfferedComplexId,
        AllocatedByUserId,
        IsTaken,
        LettingRequirements,
        Shower,
        ElectricityMeter,
        KeyNumber,
        Water,
        Refuse,
        Sewer,
        HumanEHCOptionId,
        TotalCharges,
        Inspection,
        BuildingName,
        IsActive,
        IsDeleted,
        CreatedDateTime,
        CreatedBySystemUserId
    )
    VALUES (
        @RandomSize,                                   -- SpaceUnitSize (varying)
        'AP-' + RIGHT('000' + CAST(@Counter AS VARCHAR), 3), -- SpaceUnitNumber
        'SOLAR-' + UPPER(SUBSTRING(CAST(NEWID() AS VARCHAR(36)), 1, 8)), -- SolarReference (random hash)
        @RandomRental,                                 -- MonthlyRentalAmount (varying)
        @RandomDeposit,                                -- RequiedDepositAmount (varying)
        1,                                             -- NumOfBeds (Fixed: 1 Bedroom)
        'Airport Park Complex',                        -- StreetName
        'Germiston',                                   -- Township
        '1401',                                        -- Postal
        @ComplexId,                                    -- OfferedComplexId (Fixed: Airport Park)
        1,                                             -- AllocatedByUserId
        0,                                             -- IsTaken = false
        @RandomRequirement,                            -- LettingRequirements (varying)
        1,                                             -- Shower
        'MTR-' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR), -- ElectricityMeter (randomized)
        'K-' + CAST(ABS(CHECKSUM(NEWID())) % 500 AS VARCHAR),     -- KeyNumber (randomized)
        0.00,                                          -- Water
        0.00,                                          -- Refuse
        0.00,                                          -- Sewer
        @UnitTypeId,                                   -- HumanEHCOptionId (Fixed: 1 Bedroom)
        @RandomRental,                                 -- TotalCharges (varying)
        0,                                             -- Inspection
        'Block A',                                     -- BuildingName
        1,                                             -- IsActive
        0,                                             -- IsDeleted
        GETDATE(),                                     -- CreatedDateTime
        1                                              -- CreatedBySystemUserId
    );

    SET @Counter = @Counter + 1;
END

PRINT 'Successfully generated ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' randomized dummy units for Auto-Matching testing.';
