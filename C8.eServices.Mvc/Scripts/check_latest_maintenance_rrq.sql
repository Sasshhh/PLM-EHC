-- Check the most recent RRQ records for maintenance workflows
-- This will help identify what ResponsibilityType was actually created

-- First, find coesolardev11's Customer ID
SELECT TOP 1 
    c.Id as CustomerId,
    c.FirstName + ' ' + c.LastName as FullName,
    c.SystemUserId
FROM Customers c
WHERE c.FirstName + ' ' + c.LastName LIKE '%coesolardev11%' 
   OR c.EmailAddress LIKE '%coesolardev11%'
ORDER BY c.Id;

-- Check latest RRQs for this user
DECLARE @CustomerId INT = (
    SELECT TOP 1 c.Id 
    FROM Customers c
    WHERE c.FirstName + ' ' + c.LastName LIKE '%coesolardev11%'
       OR c.EmailAddress LIKE '%coesolardev11%'
);

SELECT TOP 10
    rrq.Id as RRQ_Id,
    rrq.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    rrq.ClerkId,
    c.FirstName + ' ' + c.LastName as AssignedTo,
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
WHERE rrq.ClerkId = @CustomerId
    AND rrq.IsDeleted = 0
ORDER BY rrq.CreatedDateTime DESC;

-- Check all recent maintenance-related RRQs (regardless of user)
SELECT TOP 20
    rrq.Id as RRQ_Id,
    rrq.PropertyLeaseApplicationId,
    pla.ApplicationReferenceNumber,
    rrq.ClerkId,
    c.FirstName + ' ' + c.LastName as AssignedTo,
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
ORDER BY rrq.CreatedDateTime DESC;

-- Check PropertyLeaseInspections queue filter ResponsibilityTypes
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
