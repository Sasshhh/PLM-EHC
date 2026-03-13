SELECT a.Id, a.ApplicationReferenceNumber, a.StatusId, s.Name AS StatusName, s.[Key] AS StatusKey
FROM dbo.PropertyLeaseApplications a
JOIN dbo.Status s ON s.Id = a.StatusId
WHERE a.Id IN (5215, 5216);
