DECLARE @ApplicationId INT = 1234; -- REPLACE WITH YOUR APPLICATION ID

-- 1. Get the StatusId for "s_reupload_application_docs"
DECLARE @UploadDocsStatusId INT = (SELECT Id FROM Status WHERE [Key] = 's_reupload_application_docs');

IF @UploadDocsStatusId IS NOT NULL
BEGIN
    -- 2. Update the Application Status
    UPDATE PropertyLeaseApplications
    SET StatusId = @UploadDocsStatusId
    WHERE Id = @ApplicationId;

    -- 3. Archive any active Round Robin Queues for this application so it doesn't show in any BO inbox
    UPDATE RoundRobinQueues
    SET IsActive = 0, 
        IsDeleted = 1, 
        EndTaskDateTime = GETDATE()
    WHERE PropertyLeaseApplicationId = @ApplicationId 
      AND IsActive = 1;

    PRINT 'Application ' + CAST(@ApplicationId AS VARCHAR) + ' successfully rolled back to "Upload Application Docs".';
END
ELSE
BEGIN
    PRINT 'Error: Status key "s_reupload_application_docs" not found in Status table.';
END
