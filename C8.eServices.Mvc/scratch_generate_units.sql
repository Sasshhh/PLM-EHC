-- ==============================================================================
-- Script to generate 100 dummy units for Airport Park - 1 Bedroom
-- ==============================================================================

DECLARE @ComplexName NVARCHAR(100) = 'Airport Park';
DECLARE @UnitTypeName NVARCHAR(100) = '1 Bedroom'; -- Adjust this to match the exact string in your DB if different
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

-- 2. Look up the ID for the Unit Type "1 Bedroom"
DECLARE @UnitTypeId INT;
-- First try to find it in HumanEHCOptions (which is linked to OccupationTypeId in the model)
SELECT TOP 1 @UnitTypeId = Id 
FROM HumanEHCOptions 
WHERE Name LIKE '%' + @UnitTypeName + '%' AND IsActive = 1 AND IsDeleted = 0;

-- If not found there, it might be in OccupationTypes depending on your DB schema mapping
IF @UnitTypeId IS NULL
BEGIN
    SELECT TOP 1 @UnitTypeId = Id 
    FROM OccupationTypes 
    WHERE Propertytype LIKE '%' + @UnitTypeName + '%' AND IsActive = 1 AND IsDeleted = 0;
END

IF @UnitTypeId IS NULL
BEGIN
    PRINT 'Error: Could not find Unit Type matching: ' + @UnitTypeName;
    RETURN;
END

PRINT 'Found Complex ID: ' + CAST(@ComplexId AS VARCHAR) + ' and Unit Type ID: ' + CAST(@UnitTypeId AS VARCHAR);
PRINT 'Starting insertion of ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' units...';

-- 3. Loop to insert 100 dummy records
WHILE @Counter <= @TotalUnitsToCreate
BEGIN
    INSERT INTO Units (
        OccupationID, 
        SettlementID, 
        UnitBuildingName, 
        OccupationTypeId, 
        PreferredComplexAreaId, 
        IsTaken, 
        Address, 
        BedroomCount, 
        BathroomCount, 
        PropertyPrice, 
        PropertyDeposit, 
        SpaceUnitNo, 
        Rental, 
        DepositRequired, 
        IsActive, 
        IsDeleted, 
        CreatedDateTime,
        CreatedBySystemUserId
    )
    VALUES (
        0, 
        0, 
        'Block A',                                     -- UnitBuildingName
        @UnitTypeId,                                   -- Unit Type (1 Bedroom)
        @ComplexId,                                    -- Complex (Airport Park)
        0,                                             -- IsTaken = false
        'Airport Park Complex, Germiston',             -- Address
        1,                                             -- BedroomCount
        1,                                             -- BathroomCount
        3500.00,                                       -- PropertyPrice
        3500.00,                                       -- PropertyDeposit
        'AP-' + RIGHT('000' + CAST(@Counter AS VARCHAR), 3), -- SpaceUnitNo (AP-001, AP-002, etc.)
        3500.00,                                       -- Rental
        3500.00,                                       -- DepositRequired
        1,                                             -- IsActive
        0,                                             -- IsDeleted
        GETDATE(),                                     -- CreatedDateTime
        1                                              -- CreatedBySystemUserId (System Admin)
    );

    SET @Counter = @Counter + 1;
END

PRINT 'Successfully generated ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' dummy units for Auto-Matching testing.';
