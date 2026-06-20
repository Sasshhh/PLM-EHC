-- =============================================
-- Query Current Complex Assignments
-- Shows current officer assignments for all complexes
-- =============================================

USE [CRMPLMDEV_2025]
GO

SELECT 
    pca.Id AS ComplexId,
    pca.ComplexOrAreaName,
    
    -- Letting Officer
    lo.Id AS LettingOfficerId,
    lo.FullName AS LettingOfficerName,
    lo_user.EmailAddress AS LettingOfficerEmail,
    
    -- Housing Supervisor
    hs.Id AS HousingSupervisorId,
    hs.FullName AS HousingSupervisorName,
    hs_user.EmailAddress AS HousingSupervisorEmail,
    
    -- Maintenance Manager (new)
    mm.Id AS MaintenanceManagerId,
    mm.FullName AS MaintenanceManagerName,
    mm_user.EmailAddress AS MaintenanceManagerEmail,
    
    pca.IsActive,
    pca.IsDeleted
FROM 
    dbo.PreferredComplexAreas pca
    
    -- Letting Officer join
    LEFT JOIN dbo.Customers lo ON pca.LettingOfficerId = lo.Id
    LEFT JOIN dbo.AspNetUsers lo_user ON lo.SystemUserId = lo_user.Id
    
    -- Housing Supervisor join
    LEFT JOIN dbo.Customers hs ON pca.HousingSuperId = hs.Id
    LEFT JOIN dbo.AspNetUsers hs_user ON hs.SystemUserId = hs_user.Id
    
    -- Maintenance Manager join
    LEFT JOIN dbo.Customers mm ON pca.MaintenanceManagerId = mm.Id
    LEFT JOIN dbo.AspNetUsers mm_user ON mm.SystemUserId = mm_user.Id
    
WHERE 
    pca.IsDeleted = 0
ORDER BY 
    pca.ComplexOrAreaName
GO

-- Show count of complexes with/without maintenance managers
SELECT 
    CASE 
        WHEN MaintenanceManagerId IS NULL THEN 'No Maintenance Manager'
        ELSE 'Has Maintenance Manager'
    END AS Assignment_Status,
    COUNT(*) AS Complex_Count
FROM 
    dbo.PreferredComplexAreas
WHERE 
    IsDeleted = 0
GROUP BY 
    CASE 
        WHEN MaintenanceManagerId IS NULL THEN 'No Maintenance Manager'
        ELSE 'Has Maintenance Manager'
    END
GO
