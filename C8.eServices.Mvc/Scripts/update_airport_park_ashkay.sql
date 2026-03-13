-- Assign AshKay (Customer 190) as the officer for Airport Park areas
UPDATE PreferredComplexAreas
SET LettingOfficerId = 190
WHERE Name LIKE '%Airport Park%'

-- Update the fallback LettingOfficer AppSetting to AshKay
UPDATE AppSettings
SET Value = '190'
WHERE [Key] = 'u_letting_officer'

-- Verify PreferredComplexAreas
SELECT pca.Id, pca.Name, pca.LettingOfficerId,
       su.FirstName + ' ' + su.LastName AS AssignedTo
FROM PreferredComplexAreas pca
LEFT JOIN Customers c ON c.Id = pca.LettingOfficerId
LEFT JOIN SystemUsers su ON su.Id = c.SystemUserId
WHERE pca.Name LIKE '%Airport Park%'

-- Verify AppSettings
SELECT [Key], Name, Value FROM AppSettings WHERE [Key] = 'u_letting_officer'
