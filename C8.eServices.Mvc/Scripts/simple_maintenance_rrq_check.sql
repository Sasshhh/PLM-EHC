-- Simple check for all recent maintenance RRQs
-- No joins to AspNetUsers, just raw data

-- Find all maintenance-related RRQs from the last 7 days
SELECT TOP 30
    rrq.Id as RRQ_Id,
    rrq.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    rrq.ClerkId,
    c.FirstName + ' ' + c.LastName as AssignedTo,
    c.EmailAddress,
    rt.Name as ResponsibilityName,
    rt.[Key] as ResponsibilityKey,
    s.Name as RRQ_Status,
    pla_status.Name as Application_Status,
    rrq.CreatedDateTime,
    rrq.IsDeleted
FROM RoundRobinQueues rrq
LEFT JOIN ResponsibilityTypes rt ON rrq.ResponsibilityTypeId = rt.Id
LEFT JOIN Customers c ON rrq.ClerkId = c.Id
LEFT JOIN Status s ON rrq.StatusId = s.Id
LEFT JOIN PropertyLeaseApplications pla ON rrq.PropertyLeaseApplicationId = pla.Id
LEFT JOIN Status pla_status ON pla.StatusId = pla_status.Id
WHERE rt.[Key] IN ('r_maintanance_job_sheet', 'r_unit_maintenance')
    AND rrq.IsDeleted = 0
    AND rrq.CreatedDateTime >= DATEADD(day, -7, GETDATE())
ORDER BY rrq.CreatedDateTime DESC;

-- Check what ResponsibilityTypes PropertyLeaseInspections filters for
SELECT 
    rt.Id,
    rt.Name,
    rt.[Key],
    rt.Description
FROM ResponsibilityTypes rt
WHERE rt.[Key] IN (
    'r_inspections',
    'r_shcedule_inspection_slots', 
    'r_maintanance_job_sheet',
    'r_unit_maintenance'
)
ORDER BY rt.Name;

-- Find any Customer with 'dev11' in name or email
SELECT 
    c.Id,
    c.FirstName + ' ' + c.LastName as FullName,
    c.EmailAddress,
    c.IsDeleted,
    c.IsActive
FROM Customers c
WHERE (c.FirstName LIKE '%dev11%' 
    OR c.LastName LIKE '%dev11%' 
    OR c.EmailAddress LIKE '%dev11%')
    AND c.IsDeleted = 0
ORDER BY c.Id;
