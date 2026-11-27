-- DPRE-Q39 (confirmed with the BA, 26 Nov 2026): the application fee is paid after submission.
-- Applications submitted without proof of payment wait in 's_awaiting_application_fee_upload'
-- until the applicant uploads it from My Applications. This makes sure that status exists.
-- Safe to run more than once.

IF NOT EXISTS (SELECT 1 FROM dbo.Status WHERE [Key] = 's_awaiting_application_fee_upload')
BEGIN
    DECLARE @StatusTypeId INT = (SELECT TOP 1 Id FROM dbo.StatusTypes WHERE [Key] = 'st_rcs');
    INSERT INTO dbo.Status ([Key], [Name], [Description], StatusTypeId, IsActive, IsDeleted, CreatedDateTime)
    VALUES ('s_awaiting_application_fee_upload', 'In Awaiting Application Fee Upload',
            'Submitted; waiting for the applicant to upload proof of application fee payment', @StatusTypeId, 1, 0, GETDATE());
END
GO
