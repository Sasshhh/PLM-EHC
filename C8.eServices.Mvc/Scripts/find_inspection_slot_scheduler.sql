-- Find which back office user is assigned to schedule inspection slots for an application
-- Usage: Replace 'YOUR_APP_REF_NUMBER' with the actual application reference number

DECLARE @AppRefNum VARCHAR(50) = 'EHC2026020600001'; -- Change this to your application reference number

-- Get application details and assigned user
SELECT 
    pla.ApplicationReferenceNumber AS 'Application Ref',
    s.Description AS 'Current Status',
    rt.Name AS 'Responsibility Type',
    c.FirstName + ' ' + c.LastName AS 'Assigned User Name',
    c.Email AS 'User Email',
    c.PhoneNumber AS 'User Phone',
    u.UserName AS 'Login Username',
    rrq.CreatedDateTime AS 'Task Assigned Date',
    rrq.CurrentTaskDateTime AS 'Current Task Date',
    rrq.IsActive AS 'Task Is Active',
    rrq.Id AS 'RoundRobinQueue ID'
FROM PropertyLeaseApplications pla
LEFT JOIN Status s ON pla.StatusId = s.Id
LEFT JOIN RoundRobinQueues rrq ON rrq.PropertyLeaseApplicationId = pla.Id
LEFT JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
LEFT JOIN AspNetUsers u ON c.SystemUserId = u.Id
WHERE pla.ApplicationReferenceNumber = @AppRefNum
    AND rt.Key = 'r_shcedule_inspection_slots'  -- ScheduleInspectionSlots responsibility
    AND rrq.IsActive = 1
ORDER BY rrq.CreatedDateTime DESC;

-- If no user is assigned, show the fallback Letting Officer from AppSettings
IF NOT EXISTS (
    SELECT 1 
    FROM PropertyLeaseApplications pla
    JOIN RoundRobinQueues rrq ON rrq.PropertyLeaseApplicationId = pla.Id
    JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
    WHERE pla.ApplicationReferenceNumber = @AppRefNum
        AND rt.Key = 'r_shcedule_inspection_slots'
        AND rrq.IsActive = 1
)
BEGIN
    PRINT '⚠️ No specific user assigned. Showing default Letting Officer from settings:';
    
    SELECT 
        'DEFAULT LETTING OFFICER' AS 'Assignment Type',
        c.FirstName + ' ' + c.LastName AS 'Assigned User Name',
        c.Email AS 'User Email',
        c.PhoneNumber AS 'User Phone',
        u.UserName AS 'Login Username',
        'From AppSettings.LettingOfficer' AS 'Source'
    FROM AppSettings aps
    JOIN Customers c ON c.Id = CAST(aps.Value AS INT)
    JOIN AspNetUsers u ON c.SystemUserId = u.Id
    WHERE aps.Key = 'LettingOfficer';
END

-- Show complex area assignment (helps understand why this user was assigned)
SELECT 
    pca.ComplexName AS 'Complex/Area Name',
    lo.FirstName + ' ' + lo.LastName AS 'Complex Letting Officer',
    lo.Email AS 'Officer Email'
FROM PropertyLeaseApplications pla
JOIN ApplicantUnits au ON au.PropertyLeaseApplicationId = pla.Id
JOIN MatchedUnits mu ON mu.Id = au.MatchedID
JOIN ApplicationAllocatedProperty aap ON aap.Id = mu.ApplicationAllocatedPropertyId
JOIN PreferredComplexAreas pca ON pca.Id = aap.OfferedComplexId
LEFT JOIN Customers lo ON lo.Id = pca.LettingOfficerId
WHERE pla.ApplicationReferenceNumber = @AppRefNum;

-- Show all active round robin tasks for this application (for context)
SELECT 
    rt.Name AS 'Responsibility Type',
    rt.Key AS 'Responsibility Key',
    c.FirstName + ' ' + c.LastName AS 'Assigned To',
    rrq.CreatedDateTime AS 'Assigned Date',
    rrq.IsActive AS 'Is Active'
FROM PropertyLeaseApplications pla
JOIN RoundRobinQueues rrq ON rrq.PropertyLeaseApplicationId = pla.Id
JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
WHERE pla.ApplicationReferenceNumber = @AppRefNum
    AND rrq.IsActive = 1
ORDER BY rrq.CreatedDateTime DESC;
