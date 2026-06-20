SELECT p.Id, p.ApplicationReferenceNumber, s.Name AS StatusName, s.[Key] AS StatusKey, p.IsDeleted 
FROM dbo.PropertyLeaseApplications p 
JOIN dbo.Status s ON p.StatusId = s.Id 
WHERE p.ApplicationReferenceNumber = 'EHC2025103100001';
