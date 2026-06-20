-- =============================================
-- Add Property & Facilities Manager Review ResponsibilityType
-- This responsibility type is used when major defects require
-- Property & Facilities Manager approval before re-inspection
-- =============================================

USE [eServicesDb]
GO

-- Check if the responsibility type already exists
IF NOT EXISTS (SELECT 1 FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review')
BEGIN
    INSERT INTO dbo.ResponsibilityTypes ([Key], [Name], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'r_property_facilities_manager_review',
        'Property & Facilities Manager Review',
        'Property & Facilities Manager reviews and approves major defect maintenance completion before unit re-inspection',
        1, -- IsActive
        0, -- IsDeleted
        GETDATE(),
        GETDATE()
    )
    
    PRINT 'Property & Facilities Manager Review responsibility type added successfully.'
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager Review responsibility type already exists.'
END
GO

-- Verify insertion
SELECT * FROM dbo.ResponsibilityTypes WHERE [Key] = 'r_property_facilities_manager_review'
GO
