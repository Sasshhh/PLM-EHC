-- ============================================================
-- Backfill LeaseDetails for a PropertyLeaseApplication
-- that was created through the system but is missing its
-- LeaseDetails row (causes "Invalid Property Lease." exception).
--
-- This script copies fields directly from the application
-- and its matched unit, mirroring what the system writes
-- when ApplicationUpdateTenantLeaseDetails is submitted.
--
-- USAGE:
--   1. Set @AppId to the Id of the application that is missing
--      a LeaseDetails row (check the debugger / exception message).
--   2. Run against the eServices database.
--   3. Refresh / retry the failing action in the browser.
--
-- Safe to re-run: the script checks for an existing active
-- row first and only inserts if none exists.
-- ============================================================

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- --------------------------------------------------------
    -- CHANGE THIS to the application Id shown in the debugger
    -- --------------------------------------------------------
    DECLARE @AppId INT = 0   -- <-- replace 0 with the real Id

    IF @AppId = 0
    BEGIN
        RAISERROR('Set @AppId to the Id of the affected PropertyLeaseApplication before running.', 16, 1)
        RETURN
    END

    -- --------------------------------------------------------
    -- Guard: refuse to run if a valid LeaseDetails already exists
    -- --------------------------------------------------------
    IF EXISTS (
        SELECT 1 FROM LeaseDetails
        WHERE PropertyLeaseApplicationId = @AppId
          AND IsNew     = 1
          AND IsActive  = 1
          AND IsDeleted = 0
    )
    BEGIN
        PRINT 'A valid LeaseDetails row already exists for ApplicationId ' + CAST(@AppId AS VARCHAR) + '. Nothing to do.'
        ROLLBACK TRANSACTION
        RETURN
    END

    -- --------------------------------------------------------
    -- Pull the application record
    -- --------------------------------------------------------
    DECLARE
        @RefNum          NVARCHAR(50),
        @PurchaserTypeId INT,
        @FirstNames      NVARCHAR(200),
        @LastName        NVARCHAR(200),
        @IDNo            NVARCHAR(25),
        @CellNo          NVARCHAR(15),
        @PurEmail        NVARCHAR(50),
        @ResAddress      NVARCHAR(50),
        @ResSuburb       NVARCHAR(50),
        @ResPostal       NVARCHAR(10),
        @SystemUserId    INT,
        @CustomerId      INT,
        @StatusId        INT

    SELECT
        @RefNum          = ApplicationReferenceNumber,
        @PurchaserTypeId = PurchaserTypeId,
        @FirstNames      = FirstName,
        @LastName        = LastName,
        @IDNo            = IDNo,
        @CellNo          = CellNo,
        @PurEmail        = PurEmail,
        @ResAddress      = ResAddress,
        @ResSuburb       = ResSuburb,
        @ResPostal       = ResPostal,
        @SystemUserId    = SystemUserId,
        @CustomerId      = CustomerId,
        @StatusId        = StatusId
    FROM PropertyLeaseApplications
    WHERE Id = @AppId AND IsDeleted = 0

    IF @RefNum IS NULL
    BEGIN
        RAISERROR('PropertyLeaseApplication Id %d not found or is deleted.', 16, 1, @AppId)
        ROLLBACK TRANSACTION
        RETURN
    END

    PRINT 'Application found: Id=' + CAST(@AppId AS VARCHAR) + '  Ref=' + @RefNum

    -- --------------------------------------------------------
    -- Pull the matched / allocated unit details
    -- --------------------------------------------------------
    DECLARE
        @BuildingName    NVARCHAR(200),
        @SpaceUnitNo     NVARCHAR(100),
        @PropertyId      NVARCHAR(100),
        @RentalAmount    DECIMAL(18,2),
        @DepositAmount   DECIMAL(18,2)

    SELECT TOP 1
        @BuildingName   = ISNULL(aap.BuildingName,            ''),
        @SpaceUnitNo    = ISNULL(aap.SpaceUnitNumber,         ''),
        @PropertyId     = ISNULL(CAST(aap.Id AS NVARCHAR(20)), ''),
        @RentalAmount   = ISNULL(aap.MonthlyRentalAmount,     0),
        @DepositAmount  = ISNULL(aap.RequiedDepositAmount,    0)
    FROM MatchedUnits mu
    INNER JOIN ApplicationAllocatedProperties aap
           ON aap.Id = mu.ApplicationAllocatedPropertyId
    WHERE mu.PropertyLeaseApplicationId = @AppId
      AND mu.IsDeleted = 0
    ORDER BY mu.Id DESC

    -- Unit details may legitimately be blank for some property types; that is OK.
    PRINT 'Matched unit: Building=' + ISNULL(@BuildingName, 'n/a')
        + '  Unit=' + ISNULL(@SpaceUnitNo, 'n/a')
        + '  Rental=' + ISNULL(CAST(@RentalAmount AS VARCHAR), '0')

    -- --------------------------------------------------------
    -- Soft-delete any orphaned / non-active rows so we start
    -- clean (e.g. a row with IsNew=0 or IsActive=0)
    -- --------------------------------------------------------
    UPDATE LeaseDetails
    SET IsDeleted        = 1,
        ModifiedDateTime = GETDATE()
    WHERE PropertyLeaseApplicationId = @AppId
      AND IsDeleted = 0

    -- --------------------------------------------------------
    -- Insert the backfilled LeaseDetails row
    -- --------------------------------------------------------
    INSERT INTO LeaseDetails (
        -- identifiers
        leaseApplicationRef,
        LeaseReferenceNo,
        PurchaserTypeId,
        PropertyLeaseApplicationId,
        SystemUserId,

        -- tenant name / contact (copied from the application)
        FirstNames,
        LastName,
        IDNo,

        -- leasing address (copied from applicant residential address)
        LeaAddress,
        LeaSuburb,
        LeaPostal,

        -- unit / property info (from matched unit)
        buildingName,
        SpaceUnitNo,
        PropertyId,

        -- financial (from matched unit; zeros are safe defaults)
        RentalAmount,
        DepositeAmount,
        VATAmount,
        TotalIncludingVAT,

        -- lease state flags
        IsNew,
        IsRenewed,
        Completed,
        DetailsUpdated,
        MonthsOffered,

        -- notification preferences (default off; user will update via form)
        Email,
        SMS,
        Postal,

        -- status (carry over from the application)
        StatusId,

        -- required non-null value columns
        TerminationReminder,

        -- BaseModel columns
        IsActive, IsDeleted, IsLocked,
        CreatedBySystemUserId,  CreatedDateTime,
        ModifiedBySystemUserId, ModifiedDateTime
    )
    VALUES (
        @RefNum,                        -- leaseApplicationRef
        @RefNum,                        -- LeaseReferenceNo
        @PurchaserTypeId,
        @AppId,
        @SystemUserId,

        @FirstNames,
        @LastName,
        @IDNo,

        @ResAddress,
        @ResSuburb,
        @ResPostal,

        @BuildingName,
        @SpaceUnitNo,
        @PropertyId,

        @RentalAmount,
        @DepositAmount,
        0,                              -- VATAmount   (to be completed via form)
        @RentalAmount,                  -- TotalIncludingVAT (excl VAT placeholder)

        1,                              -- IsNew  = true  (required by the query)
        0,                              -- IsRenewed
        0,                              -- Completed
        0,                              -- DetailsUpdated
        0,                              -- MonthsOffered

        0,                              -- Email
        0,                              -- SMS
        0,                              -- Postal

        @StatusId,

        '1900-01-01',                   -- TerminationReminder (non-null sentinel)

        1, 0, 0,                        -- IsActive, IsDeleted, IsLocked
        @SystemUserId, GETDATE(),
        @SystemUserId, GETDATE()
    )

    DECLARE @NewLeaseId INT = SCOPE_IDENTITY()

    PRINT ''
    PRINT '============================================================'
    PRINT 'LeaseDetails backfilled successfully.'
    PRINT '  New LeaseDetails.Id          : ' + CAST(@NewLeaseId AS VARCHAR)
    PRINT '  PropertyLeaseApplicationId   : ' + CAST(@AppId AS VARCHAR)
    PRINT '  ApplicationReferenceNumber   : ' + @RefNum
    PRINT '============================================================'
    PRINT 'You can now retry the failing action in the browser.'
    PRINT 'Complete the lease details form to fill in the remaining fields.'
    PRINT '============================================================'

    -- Quick verification snapshot
    SELECT
        ld.Id                           AS LeaseDetailsId,
        ld.PropertyLeaseApplicationId,
        p.ApplicationReferenceNumber,
        ld.IsNew,
        ld.IsActive,
        ld.IsDeleted,
        ld.FirstNames,
        ld.LastName,
        ld.IDNo,
        ld.buildingName,
        ld.SpaceUnitNo,
        ld.RentalAmount,
        ld.DepositeAmount,
        ld.StatusId
    FROM LeaseDetails ld
    INNER JOIN PropertyLeaseApplications p ON p.Id = ld.PropertyLeaseApplicationId
    WHERE ld.Id = @NewLeaseId

    COMMIT TRANSACTION;
    PRINT 'Transaction committed.'

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR  : ' + ERROR_MESSAGE()
    PRINT 'Line   : ' + CAST(ERROR_LINE() AS VARCHAR)
END CATCH
