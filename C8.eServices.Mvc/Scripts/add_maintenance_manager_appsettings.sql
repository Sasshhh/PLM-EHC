-- =============================================
-- Add AppSettings for Maintenance Manager and Property & Facilities Manager
-- These settings define system-wide default user IDs for these roles
-- =============================================

USE [eServicesDb]
GO

-- Add Maintenance Manager AppSetting
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = 'u_maintenance_manager')
BEGIN
    INSERT INTO dbo.AppSettings ([Key], [Value], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'u_maintenance_manager',
        '0', -- Default: 0 (must be updated with actual Customer ID)
        'System-wide fallback Maintenance Manager user ID. Used when complex-specific Maintenance Manager is not assigned.',
        1, -- IsActive
        0, -- IsDeleted
        GETDATE(),
        GETDATE()
    )
    
    PRINT 'Maintenance Manager AppSetting added successfully.'
END
ELSE
BEGIN
    PRINT 'Maintenance Manager AppSetting already exists.'
END
GO

-- Add Property & Facilities Manager AppSetting
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = 'u_property_facilities_manager')
BEGIN
    INSERT INTO dbo.AppSettings ([Key], [Value], [Description], IsActive, IsDeleted, CapturedDateTime, ModifiedDateTime)
    VALUES (
        'u_property_facilities_manager',
        '0', -- Default: 0 (must be updated with actual Customer ID)
        'System-wide Property & Facilities Manager user ID. This role is NOT per-complex and applies across the entire system.',
        1, -- IsActive
        0, -- IsDeleted
        GETDATE(),
        GETDATE()
    )
    
    PRINT 'Property & Facilities Manager AppSetting added successfully.'
END
ELSE
BEGIN
    PRINT 'Property & Facilities Manager AppSetting already exists.'
END
GO

-- Verify insertions
SELECT * FROM dbo.AppSettings WHERE [Key] IN ('u_maintenance_manager', 'u_property_facilities_manager')
GO

-- =============================================
-- IMPORTANT: Update these values with actual Customer IDs
-- =============================================
-- Example:
-- UPDATE dbo.AppSettings SET [Value] = '12345' WHERE [Key] = 'u_maintenance_manager'
-- UPDATE dbo.AppSettings SET [Value] = '67890' WHERE [Key] = 'u_property_facilities_manager'
