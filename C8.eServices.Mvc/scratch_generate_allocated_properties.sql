-- ==============================================================================
-- Script to generate 100 dummy units for Airport Park - 1 Bedroom (Auto-Matching)
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
-- Auto-matching strictly checks x.HumanEHCOptionId == findPropertyLease.HumanEHCOptionsId
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
PRINT 'Starting insertion of ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' units into ApplicationAllocatedProperties...';

-- 3. Loop to insert 100 dummy records
WHILE @Counter <= @TotalUnitsToCreate
BEGIN
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
        50.00,                                         -- SpaceUnitSize
        'AP-' + RIGHT('000' + CAST(@Counter AS VARCHAR), 3), -- SpaceUnitNumber (AP-001, etc.)
        'SOLAR-TEST-' + CAST(@Counter AS VARCHAR),     -- SolarReference
        3500.00,                                       -- MonthlyRentalAmount
        3500.00,                                       -- RequiedDepositAmount
        1,                                             -- NumOfBeds
        'Airport Park Complex',                        -- StreetName
        'Germiston',                                   -- Township
        '1401',                                        -- Postal
        @ComplexId,                                    -- OfferedComplexId
        1,                                             -- AllocatedByUserId (System Admin)
        0,                                             -- IsTaken = false
        'Standard',                                    -- LettingRequirements
        1,                                             -- Shower (Bathroom count)
        '',                                            -- ElectricityMeter
        '',                                            -- KeyNumber
        0.00,                                          -- Water
        0.00,                                          -- Refuse
        0.00,                                          -- Sewer
        @UnitTypeId,                                   -- HumanEHCOptionId
        3500.00,                                       -- TotalCharges
        0,                                             -- Inspection
        'Block A',                                     -- BuildingName
        1,                                             -- IsActive
        0,                                             -- IsDeleted
        GETDATE(),                                     -- CreatedDateTime
        1                                              -- CreatedBySystemUserId
    );

    SET @Counter = @Counter + 1;
END

PRINT 'Successfully generated ' + CAST(@TotalUnitsToCreate AS VARCHAR) + ' dummy units for Auto-Matching testing.';
