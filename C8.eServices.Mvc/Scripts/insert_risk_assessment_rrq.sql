INSERT INTO dbo.RoundRobinQueues
(ClerkId, RCSApplicationStatusId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked,
 CreatedBySystemUserId, CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime,
 PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
SELECT TOP 1
    190, NULL, 11, 99, 1, 0, 0,
    1214, GETDATE(), 1214, GETDATE(),
    5216, LeaseDetailsId, DepartmentId
FROM dbo.RoundRobinQueues
WHERE PropertyLeaseApplicationId = 5216
ORDER BY Id DESC;

SELECT rq.Id, rt.Name AS Responsibility, st.Name AS QueueStatus, rq.ClerkId
FROM dbo.RoundRobinQueues rq
LEFT JOIN dbo.ResponsibilityTypes rt ON rt.Id = rq.ResponsibilityTypeId
LEFT JOIN dbo.Status st ON st.Id = rq.StatusId
WHERE rq.PropertyLeaseApplicationId = 5216
ORDER BY rq.Id DESC;
