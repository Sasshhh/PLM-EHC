SELECT q.Id, q.PropertyLeaseApplicationId, r.Name AS Responsibility, u.FirstName, u.LastName, q.StatusId, q.CreatedDateTime 
FROM dbo.RoundRobinQueues q 
JOIN dbo.ResponsibilityTypes r ON q.ResponsibilityTypeId = r.Id 
LEFT JOIN dbo.SystemUsers u ON q.ClerkId = u.Id 
WHERE q.PropertyLeaseApplicationId = 4197 AND q.EndTaskDateTime IS NULL;
