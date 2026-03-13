-- ============================================================
-- INSERT a new available 2-Bedroom unit for Airport Park
-- into dbo.ApplicationAllocatedProperties
--
-- WHY THIS TABLE:
--   ApplicationAllocatedProperties is the ONLY unit table
--   used by the PLM auto-allocation and manual letting officer
--   flows. MatchedUnitsController.UnitDetails(), AcceptMatchedUnit(),
--   and RejectMatchedUnit() ALL read from this table (via
--   matchedUnit.ApplicationAllocatedPropertyId). The legacy
--   dbo.Units table is empty for Airport Park (0 rows) and is
--   only referenced by the old HSD matching path via UnitsId.
--   dbo.ApplicationAllocatedProperties is the source of truth
--   for the EHC/PLM waiting list allocation.
--
-- KEY IDs verified from live DB:
--   OfferedComplexId  = 24   (Airport Park, key: p_airportpark)
--   HumanEHCOptionId  = 8    (2 Bedroom, key: ehc_2_bedroom_flat)
--   AllocatedByUserId = 2321 (Zanele Mngomezulu - COESolarDev02,
--                             matches all existing units in table)
--   CreatedBySystemUserId = 2321 (same pattern as existing rows)
-- ============================================================

-- Verify pre-conditions before inserting
SELECT
    'Airport Park Complex'        AS CheckItem,
    Id, Name, [Key]
FROM dbo.PreferredComplexAreas
WHERE Id = 24;

SELECT
    '2 Bedroom EHC Option'        AS CheckItem,
    Id, Name, [Key]
FROM dbo.HumanEHCOptions
WHERE Id = 8;

SELECT
    'Existing 2-Bed units (available)'  AS CheckItem,
    COUNT(*)                            AS AvailableCount
FROM dbo.ApplicationAllocatedProperties
WHERE OfferedComplexId = 24
  AND HumanEHCOptionId = 8
  AND IsTaken = 0
  AND IsDeleted = 0;

-- ============================================================
-- INSERT the new unit
-- Adjust SpaceUnitNumber, SolarReference, StreetName,
-- Township, Postal, MonthlyRentalAmount etc. as needed.
-- ============================================================
INSERT INTO dbo.ApplicationAllocatedProperties
(
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
    -- BaseModel audit columns
    IsActive,
    IsDeleted,
    IsLocked,
    CreatedBySystemUserId,
    CreatedDateTime,
    ModifiedBySystemUserId,
    ModifiedDateTime
)
VALUES
(
    65,                        -- SpaceUnitSize (m²) - typical 2-bed
    'AP2B-001',                -- SpaceUnitNumber  ? UPDATE to real unit number
    'SOLAR-AP-2B-001',         -- SolarReference   ? UPDATE to real Solar ref
    3500.00,                   -- MonthlyRentalAmount (R3 500 - adjust as needed)
    3500.00,                   -- RequiedDepositAmount (1 month deposit)
    2,                         -- NumOfBeds = 2
    'Airport Park Drive',      -- StreetName       ? UPDATE to real street
    'Airport Park',            -- Township
    '1619',                    -- Postal
    24,                        -- OfferedComplexId = Airport Park
    2321,                      -- AllocatedByUserId (Zanele Mngomezulu, matches existing rows)
    0,                         -- IsTaken = 0 (available)
    NULL,                      -- LettingRequirements
    1,                         -- Shower (bathrooms)
    NULL,                      -- ElectricityMeter
    NULL,                      -- KeyNumber
    0.00,                      -- Water
    0.00,                      -- Refuse
    0.00,                      -- Sewer
    8,                         -- HumanEHCOptionId = 2 Bedroom (ehc_2_bedroom_flat)
    3500.00,                   -- TotalCharges (rental + levies)
    0,                         -- Inspection = false (not yet inspected)
    'Airport Park Block A',    -- BuildingName     ? UPDATE to real building name
    -- BaseModel
    1,                         -- IsActive
    0,                         -- IsDeleted
    0,                         -- IsLocked
    2321,                      -- CreatedBySystemUserId
    GETDATE(),                 -- CreatedDateTime
    2321,                      -- ModifiedBySystemUserId
    GETDATE()                  -- ModifiedDateTime
);

-- Confirm the inserted row
SELECT
    Id,
    SpaceUnitNumber,
    SolarReference,
    NumOfBeds,
    OfferedComplexId,
    HumanEHCOptionId,
    IsTaken,
    MonthlyRentalAmount,
    BuildingName,
    StreetName,
    Township,
    Postal
FROM dbo.ApplicationAllocatedProperties
WHERE Id = SCOPE_IDENTITY();
