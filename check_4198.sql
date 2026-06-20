SELECT q.Id, r.Name AS Responsibility, q.CreatedDateTime, q.EndTaskDateTime 
FROM dbo.RoundRobinQueues q 
JOIN dbo.ResponsibilityTypes r ON q.ResponsibilityTypeId = r.Id 
WHERE q.PropertyLeaseApplicationId = 4198 
ORDER BY q.Id DESC;
