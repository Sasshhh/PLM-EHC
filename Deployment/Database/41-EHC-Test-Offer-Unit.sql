/* 41-EHC-Test-Offer-Unit.sql  -- TEST databases only.
   Offers one free unit to one waiting-list application, the same way Waiting List >
   Run Auto-Allocate / Manual Allocate does (minus the e-mail). Use it when the screen
   cannot be used. Set @Ref, select the PLM database, Execute.
   Afterwards the applicant sees the offer under PLM Applications > Inbox. */
SET NOCOUNT ON; SET XACT_ABORT ON;
DECLARE @Ref nvarchar(50) = N'EHC2026111600001';    -- application reference to offer a unit to
DECLARE @By  nvarchar(256) = N'richadt';            -- user recorded as allocating it

DECLARE @AppId int, @Complex int, @Complex2 int, @Option int, @WlId int, @UnitId int, @UnitNo nvarchar(100),
        @SU int = (SELECT SystemUserId FROM dbo.AspNetUsers WHERE UserName = @By),
        @Awaited int = (SELECT TOP (1) Id FROM dbo.Status WHERE [Key] = N's_rcs_awaited' ORDER BY Id);

SELECT @AppId = Id, @Complex = PreferredComplexAreaId, @Complex2 = PreferredComplexArea2Id, @Option = HumanEHCOptionsId
FROM dbo.PropertyLeaseApplications WHERE ApplicationReferenceNumber = @Ref;
SELECT TOP (1) @WlId = Id FROM dbo.PropertyLeaseWaitingLists
WHERE PropertyLeaseApplicationId = @AppId AND QueueStatus = N'Waiting' AND IsActive = 1 AND IsDeleted = 0 ORDER BY Id;

IF @AppId IS NULL BEGIN RAISERROR('Application %s not found.', 16, 1, @Ref); RETURN; END
IF @WlId IS NULL  BEGIN RAISERROR('Application %s is not Waiting on the waiting list (already offered?).', 16, 1, @Ref); RETURN; END
IF @Awaited IS NULL BEGIN RAISERROR('Status s_rcs_awaited is missing.', 16, 1); RETURN; END

SELECT TOP (1) @UnitId = Id, @UnitNo = SpaceUnitNumber
FROM dbo.ApplicationAllocatedProperties
WHERE IsTaken = 0 AND IsActive = 1 AND IsDeleted = 0 AND HumanEHCOptionId = @Option
  AND OfferedComplexId IN (@Complex, ISNULL(@Complex2, -1))
ORDER BY CASE WHEN OfferedComplexId = @Complex THEN 0 ELSE 1 END, Id;
IF @UnitId IS NULL BEGIN RAISERROR('No free unit of this type in the preferred complexes.', 16, 1); RETURN; END

BEGIN TRANSACTION;
UPDATE dbo.ApplicationAllocatedProperties
   SET IsTaken = 1, AllocatedByUserId = ISNULL(@SU, AllocatedByUserId), ModifiedDateTime = GETDATE()
 WHERE Id = @UnitId AND IsTaken = 0;

INSERT dbo.MatchedUnits (PropertyLeaseApplicationId, LeaseReferenceNo, ApplicationAllocatedPropertyId,
                         IsAccepted, RejectedProperty, IsActive, IsDeleted, CreatedBySystemUserId, CreatedDateTime)
VALUES (@AppId, @Ref, @UnitId, 0, 0, 1, 0, @SU, GETDATE());

UPDATE dbo.PropertyLeaseWaitingLists SET QueueStatus = N'Offered', OfferedUnitId = @UnitId, ModifiedDateTime = GETDATE() WHERE Id = @WlId;
UPDATE dbo.WaitingListQues SET IsMatched = 1, ModifiedDateTime = GETDATE() WHERE PropertyLeaseApplicationId = @AppId;
UPDATE dbo.PropertyLeaseApplications SET StatusId = @Awaited WHERE Id = @AppId;

INSERT dbo.PLMApplicationHistortyLogs (PropertyLeaseApplicationId, AuditAction, UserId, CreatedDateTime, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId)
SELECT @AppId, N'Unit ' + ISNULL(@UnitNo, CAST(@UnitId AS nvarchar(20))) + N' offered from the waiting list (manual script).',
       ISNULL((SELECT TOP (1) Id FROM dbo.Customers WHERE SystemUserId = @SU), (SELECT CustomerId FROM dbo.PropertyLeaseApplications WHERE Id = @AppId)),
       GETDATE(), 1, 0, 0, @SU;
COMMIT;

SELECT @Ref AS Application, @UnitId AS UnitId, @UnitNo AS UnitNumber, N'Offered' AS QueueStatus;
