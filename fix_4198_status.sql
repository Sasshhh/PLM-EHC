DECLARE @AppId INT = 4198;
DECLARE @TargetStatusId INT;

SELECT @TargetStatusId = Id FROM dbo.Status WHERE [Key] = 's_awaiting_managers_signature';

UPDATE dbo.PropertyLeaseApplications
SET StatusId = @TargetStatusId
WHERE Id = @AppId;
