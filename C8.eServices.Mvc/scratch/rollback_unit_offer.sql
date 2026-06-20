DECLARE @AppRef VARCHAR(50) = 'EHC2025103100002';
DECLARE @AppId INT = (SELECT Id FROM PropertyLeaseApplications WHERE ApplicationReferenceNumber = @AppRef);

IF @AppId IS NOT NULL
BEGIN
    -- 1. Restore MatchedUnit
    DECLARE @MatchedUnitId INT = (SELECT TOP 1 Id FROM MatchedUnits WHERE PropertyLeaseApplicationId = @AppId ORDER BY Id DESC);
    UPDATE MatchedUnits SET IsDeleted = 0, RejectedProperty = 0, IsAccepted = 0 WHERE Id = @MatchedUnitId;

    -- 2. Restore Unit Availability (lock it for the applicant again)
    DECLARE @UnitId INT = (SELECT UnitsId FROM MatchedUnits WHERE Id = @MatchedUnitId);
    UPDATE Units SET IsTaken = 1 WHERE Id = @UnitId;

    -- 3. Restore App Status to "Risk Assessment Approved" (this allows unit offers to be viewed)
    DECLARE @StatusId INT = (SELECT Id FROM Status WHERE [Key] = 's_rcs_approved');
    UPDATE PropertyLeaseApplications SET StatusId = @StatusId WHERE Id = @AppId;

    -- 4. Restore WaitingListQue (mark as matched so it doesn't get picked up by another applicant)
    UPDATE WaitingListQues SET IsMatched = 1 WHERE PropertyLeaseApplicationId = @AppId;

    PRINT 'Application ' + @AppRef + ' successfully rolled back to Unit Offer phase!';
END
ELSE
BEGIN
    PRINT 'Error: Application not found!';
END
