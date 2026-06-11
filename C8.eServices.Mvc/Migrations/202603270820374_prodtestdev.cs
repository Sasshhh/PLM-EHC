namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class prodtestdev : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RiskAssessmentOutcomeAudits", "Id", "dbo.Files");
            DropForeignKey("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", "dbo.Files");
            DropIndex("dbo.RiskAssessmentOutcomeAudits", new[] { "Id" });
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "SupportingDoccuments_Id" });
            CreateTable(
                "dbo.MaintenanceJobCardSignatures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AllocatedUnitMaintenanceEHCId = c.Int(nullable: false),
                        OfficialNumber = c.String(nullable: false, maxLength: 50),
                        ApprovalAction = c.String(nullable: false, maxLength: 20),
                        Reason = c.String(nullable: false, maxLength: 1000),
                        SignatureData = c.String(nullable: false),
                        ApprovalDate = c.DateTime(nullable: false),
                        SignedByCustomerId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(nullable: false),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AllocatedUnitMaintenanceEHCs", t => t.AllocatedUnitMaintenanceEHCId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.SignedByCustomerId)
                .Index(t => t.AllocatedUnitMaintenanceEHCId)
                .Index(t => t.SignedByCustomerId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.MaintenanceJobCardTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AllocatedUnitMaintenanceEHCId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(nullable: false, storeType: "date"),
                        Activity = c.String(nullable: false, maxLength: 500),
                        MaterialUsed = c.String(maxLength: 200),
                        QuantityUsed = c.Decimal(precision: 18, scale: 2),
                        LabourUsed = c.String(maxLength: 200),
                        TotalCosts = c.Decimal(precision: 18, scale: 2),
                        TaskComments = c.String(maxLength: 1000),
                        SupportingDocumentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(nullable: false),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AllocatedUnitMaintenanceEHCs", t => t.AllocatedUnitMaintenanceEHCId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Documents", t => t.SupportingDocumentId)
                .Index(t => t.AllocatedUnitMaintenanceEHCId)
                .Index(t => t.SupportingDocumentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ApplicationsEntities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Key = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ComplaintCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Key = c.String(maxLength: 50),
                        Description = c.String(),
                        DisplayOrder = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.TenantComplaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CaseReferenceNumber = c.String(nullable: false, maxLength: 50),
                        OfficialNumber = c.String(),
                        ComplainantComplexId = c.Int(nullable: false),
                        ComplainantFirstName = c.String(nullable: false, maxLength: 100),
                        ComplainantSurname = c.String(nullable: false, maxLength: 100),
                        ComplainantEmail = c.String(maxLength: 100),
                        ComplainantCellphone = c.String(maxLength: 20),
                        ComplainantUnitNumber = c.String(maxLength: 20),
                        ComplainantBlockNumber = c.String(maxLength: 20),
                        RespondentComplexId = c.Int(nullable: false),
                        RespondentBlockNumber = c.String(nullable: false, maxLength: 20),
                        RespondentUnitNumber = c.String(nullable: false, maxLength: 20),
                        RespondentFirstName = c.String(maxLength: 100),
                        RespondentSurname = c.String(maxLength: 100),
                        ComplaintCategoryId = c.Int(nullable: false),
                        ComplaintTypeId = c.Int(),
                        DetailedDescription = c.String(nullable: false),
                        StatusId = c.Int(nullable: false),
                        SubmittedByCustomerId = c.Int(),
                        SubmittedByUserId = c.Int(),
                        DateSubmitted = c.DateTime(),
                        AssignedToId = c.Int(),
                        DateAssigned = c.DateTime(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.AssignedToId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.ComplainantComplexId)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId)
                .ForeignKey("dbo.ComplaintTypes", t => t.ComplaintTypeId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.RespondentComplexId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .ForeignKey("dbo.Customers", t => t.SubmittedByCustomerId)
                .Index(t => t.ComplainantComplexId)
                .Index(t => t.RespondentComplexId)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.ComplaintTypeId)
                .Index(t => t.StatusId)
                .Index(t => t.SubmittedByCustomerId)
                .Index(t => t.AssignedToId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComplaintCategoryId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Key = c.String(maxLength: 50),
                        Description = c.String(),
                        DisplayOrder = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintCategories", t => t.ComplaintCategoryId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.ComplaintCategoryId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintEvidences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantComplaintId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(nullable: false, maxLength: 100),
                        FilePath = c.String(),
                        FileSize = c.Long(),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(),
                        Description = c.String(maxLength: 500),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.UploadedById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintInvestigations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantComplaintId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(),
                        AppointmentTime = c.Time(precision: 7),
                        ScheduledById = c.Int(),
                        DateScheduled = c.DateTime(),
                        RespondentConfirmed = c.Boolean(nullable: false),
                        DateConfirmed = c.DateTime(),
                        Outcome = c.String(maxLength: 50),
                        OutcomeDetails = c.String(),
                        OutcomeDate = c.DateTime(),
                        InvestigatedById = c.Int(),
                        ExternalReferralId = c.Int(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.ComplaintExternalReferrals", t => t.ExternalReferralId)
                .ForeignKey("dbo.Customers", t => t.InvestigatedById)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.ScheduledById)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.ScheduledById)
                .Index(t => t.InvestigatedById)
                .Index(t => t.ExternalReferralId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintInvestigationDocuments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ComplaintInvestigationId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(nullable: false, maxLength: 100),
                        FilePath = c.String(),
                        FileSize = c.Long(),
                        UploadedById = c.Int(),
                        UploadDate = c.DateTime(),
                        Description = c.String(maxLength: 500),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ComplaintInvestigations", t => t.ComplaintInvestigationId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.UploadedById)
                .Index(t => t.ComplaintInvestigationId)
                .Index(t => t.UploadedById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ComplaintExternalReferrals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AgencyName = c.String(nullable: false, maxLength: 200),
                        Address = c.String(),
                        ContactPerson = c.String(maxLength: 100),
                        ContactNumber = c.String(maxLength: 20),
                        ContactEmail = c.String(maxLength: 100),
                        BriefDescription = c.String(),
                        ReferredById = c.Int(),
                        DateReferred = c.DateTime(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.Customers", t => t.ReferredById)
                .Index(t => t.ReferredById)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.ExaminationQuestionAudits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 50),
                        QuestionText = c.String(),
                        OptionA = c.String(),
                        OptionB = c.String(),
                        OptionC = c.String(),
                        OptionD = c.String(),
                        CorrectAnswer = c.String(),
                        QuestionOrder = c.Int(nullable: false),
                        IsExampleQuestion = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExaminationQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionText = c.String(nullable: false),
                        OptionA = c.String(maxLength: 500),
                        OptionB = c.String(maxLength: 500),
                        OptionC = c.String(maxLength: 500),
                        OptionD = c.String(maxLength: 500),
                        CorrectAnswer = c.String(nullable: false, maxLength: 1),
                        QuestionOrder = c.Int(nullable: false),
                        IsExampleQuestion = c.Boolean(nullable: false),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.LeaseReviewComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        LeaseDetailsId = c.Int(),
                        StatusId = c.Int(),
                        RCSActionTypeId = c.Int(),
                        Comment = c.String(maxLength: 100),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.LeaseDetails", t => t.LeaseDetailsId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .ForeignKey("dbo.RCSActionTypes", t => t.RCSActionTypeId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.LeaseDetailsId)
                .Index(t => t.StatusId)
                .Index(t => t.RCSActionTypeId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.PropertyLeaseWaitingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        PreferredComplexId = c.Int(nullable: false),
                        PreferredTypologyId = c.Int(nullable: false),
                        DateAdded = c.DateTime(nullable: false),
                        QueueStatus = c.String(),
                        OfferedUnitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.HumanEHCOptions", t => t.PreferredTypologyId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.PreferredComplexId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.PreferredComplexId)
                .Index(t => t.PreferredTypologyId);
            
            CreateTable(
                "dbo.TenantExamAnswerAudits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 50),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ExaminationQuestionId = c.Int(nullable: false),
                        SelectedAnswer = c.String(),
                        IsCorrect = c.Boolean(nullable: false),
                        AnsweredDateTime = c.DateTime(nullable: false),
                        AttemptNumber = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TenantExamAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        ExaminationQuestionId = c.Int(nullable: false),
                        SelectedAnswer = c.String(nullable: false, maxLength: 1),
                        IsCorrect = c.Boolean(nullable: false),
                        AnsweredDateTime = c.DateTime(nullable: false),
                        AttemptNumber = c.Int(nullable: false),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.ExaminationQuestions", t => t.ExaminationQuestionId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.ExaminationQuestionId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.TenantTrainingAudits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 50),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        InvitationToken = c.String(maxLength: 500),
                        TokenExpiryDate = c.DateTime(),
                        InvitationSentDate = c.DateTime(),
                        TrainingStartedDate = c.DateTime(),
                        TrainingCompletedDate = c.DateTime(),
                        CurrentSlideNumber = c.Int(nullable: false),
                        IsTrainingCompleted = c.Boolean(nullable: false),
                        ExamAttempts = c.Int(nullable: false),
                        ExamScore = c.Decimal(precision: 18, scale: 2),
                        IsExamPassed = c.Boolean(nullable: false),
                        ExamPassedDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TenantTrainings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        InvitationToken = c.String(maxLength: 500),
                        TokenExpiryDate = c.DateTime(),
                        InvitationSentDate = c.DateTime(),
                        TrainingStartedDate = c.DateTime(),
                        TrainingCompletedDate = c.DateTime(),
                        CurrentSlideNumber = c.Int(nullable: false),
                        IsTrainingCompleted = c.Boolean(nullable: false),
                        ExamAttempts = c.Int(nullable: false),
                        ExamScore = c.Decimal(precision: 18, scale: 2),
                        IsExamPassed = c.Boolean(nullable: false),
                        ExamPassedDate = c.DateTime(),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.TrainingSlideAudits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 50),
                        SlideNumber = c.Int(nullable: false),
                        Title = c.String(),
                        Content = c.String(),
                        ImagePath = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrainingSlides",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SlideNumber = c.Int(nullable: false),
                        Title = c.String(maxLength: 500),
                        Content = c.String(),
                        ImagePath = c.String(maxLength: 500),
                        DisplayOrder = c.Int(nullable: false),
                        DepartmentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            AddColumn("dbo.PreferredComplexAreas", "MaintenanceManagerId", c => c.Int());
            AddColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "KeyDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", c => c.Int());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", c => c.Int());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmitted", c => c.Boolean());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmittedDate", c => c.DateTime());
            AddColumn("dbo.AllocatedUnitMaintenanceEHCs", "Inspection", c => c.Boolean());
            AddColumn("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", c => c.Int());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "PropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "RevenueManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "AccessCardDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "KeyDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVActivationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVMonthlyLevy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "CommencementDay", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode", c => c.String(maxLength: 10));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "PropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "RevenueManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "Witness1SignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay", c => c.String(maxLength: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName", c => c.String(maxLength: 100));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber", c => c.String(maxLength: 20));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType", c => c.String(maxLength: 50));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode", c => c.String(maxLength: 10));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "SupportingDoccuments", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "RM_OfficialNumber", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "RM_Outcome", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "RM_Reason", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "RM_DateStamp", c => c.DateTime());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "RM_SystemUserId", c => c.Int());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_OfficialNumber", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_Outcome", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_Reason", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_DateStamp", c => c.DateTime());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_SystemUserId", c => c.Int());
            AddColumn("dbo.RiskAssessmentOutcomeAudits", "SignatureBlob", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "RM_OfficialNumber", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "RM_Outcome", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "RM_Reason", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "RM_DateStamp", c => c.DateTime());
            AddColumn("dbo.RiskAssessmentOutcomes", "RM_SystemUserId", c => c.Int());
            AddColumn("dbo.RiskAssessmentOutcomes", "CEO_OfficialNumber", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "CEO_Outcome", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "CEO_Reason", c => c.String());
            AddColumn("dbo.RiskAssessmentOutcomes", "CEO_DateStamp", c => c.DateTime());
            AddColumn("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId", c => c.Int());
            AddColumn("dbo.RiskAssessmentOutcomes", "SignatureBlob", c => c.String());
            AddColumn("dbo.SystemUserAudits", "isInternalUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.SystemUserAudits", "isActiveDirectoryUser", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseApplications", "GrossIncome", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.PropertyLeaseApplications", "NetIncome", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.PropertyLeaseApplications", "TotalCombinedIncome", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.PreferredComplexAreas", "MaintenanceManagerId");
            CreateIndex("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId");
            CreateIndex("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId");
            CreateIndex("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId");
            CreateIndex("dbo.RiskAssessmentOutcomeAudits", "RM_SystemUserId");
            CreateIndex("dbo.RiskAssessmentOutcomeAudits", "CEO_SystemUserId");
            CreateIndex("dbo.RiskAssessmentOutcomes", "RM_SystemUserId");
            CreateIndex("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId");
            AddForeignKey("dbo.PreferredComplexAreas", "MaintenanceManagerId", "dbo.Customers", "Id");
            AddForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", "dbo.Documents", "Id");
            AddForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", "dbo.Documents", "Id");
            AddForeignKey("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", "dbo.Customers", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomeAudits", "CEO_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomeAudits", "RM_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "RM_SystemUserId", "dbo.SystemUsers", "Id");
            DropColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", c => c.Int());
            DropForeignKey("dbo.TrainingSlides", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TrainingSlides", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.TrainingSlides", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantTrainings", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.TenantTrainings", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantTrainings", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.TenantTrainings", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantExamAnswers", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.TenantExamAnswers", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantExamAnswers", "ExaminationQuestionId", "dbo.ExaminationQuestions");
            DropForeignKey("dbo.TenantExamAnswers", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.TenantExamAnswers", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RiskAssessmentOutcomes", "RM_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RiskAssessmentOutcomeAudits", "RM_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RiskAssessmentOutcomeAudits", "CEO_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "PreferredTypologyId", "dbo.HumanEHCOptions");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.PropertyLeaseWaitingLists", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId", "dbo.Customers");
            DropForeignKey("dbo.LeaseReviewComments", "StatusId", "dbo.Status");
            DropForeignKey("dbo.LeaseReviewComments", "RCSActionTypeId", "dbo.RCSActionTypes");
            DropForeignKey("dbo.LeaseReviewComments", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.LeaseReviewComments", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.LeaseReviewComments", "LeaseDetailsId", "dbo.LeaseDetails");
            DropForeignKey("dbo.LeaseReviewComments", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.LeaseReviewComments", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ExaminationQuestions", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ExaminationQuestions", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ExaminationQuestions", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintCategories", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintCategories", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintCategories", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "SubmittedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.TenantComplaints", "StatusId", "dbo.Status");
            DropForeignKey("dbo.TenantComplaints", "RespondentComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.TenantComplaints", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.ComplaintInvestigations", "ScheduledById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigations", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "InvestigatedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintExternalReferrals", "ReferredById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintExternalReferrals", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigations", "ExternalReferralId", "dbo.ComplaintExternalReferrals");
            DropForeignKey("dbo.ComplaintExternalReferrals", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintExternalReferrals", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintInvestigationDocuments", "ComplaintInvestigationId", "dbo.ComplaintInvestigations");
            DropForeignKey("dbo.ComplaintInvestigations", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintInvestigations", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintEvidences", "UploadedById", "dbo.Customers");
            DropForeignKey("dbo.ComplaintEvidences", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.ComplaintEvidences", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintEvidences", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintEvidences", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.TenantComplaints", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintTypes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ComplaintTypes", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ComplaintTypes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.TenantComplaints", "ComplaintTypeId", "dbo.ComplaintTypes");
            DropForeignKey("dbo.ComplaintTypes", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropForeignKey("dbo.TenantComplaints", "ComplaintCategoryId", "dbo.ComplaintCategories");
            DropForeignKey("dbo.TenantComplaints", "ComplainantComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.TenantComplaints", "AssignedToId", "dbo.Customers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "SupportingDocumentId", "dbo.Documents");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardTasks", "AllocatedUnitMaintenanceEHCId", "dbo.AllocatedUnitMaintenanceEHCs");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "SignedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.MaintenanceJobCardSignatures", "AllocatedUnitMaintenanceEHCId", "dbo.AllocatedUnitMaintenanceEHCs");
            DropForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId", "dbo.Documents");
            DropForeignKey("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId", "dbo.Documents");
            DropForeignKey("dbo.PreferredComplexAreas", "MaintenanceManagerId", "dbo.Customers");
            DropIndex("dbo.TrainingSlides", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TrainingSlides", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TrainingSlides", new[] { "DepartmentId" });
            DropIndex("dbo.TenantTrainings", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TenantTrainings", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TenantTrainings", new[] { "DepartmentId" });
            DropIndex("dbo.TenantTrainings", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.TenantExamAnswers", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TenantExamAnswers", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TenantExamAnswers", new[] { "DepartmentId" });
            DropIndex("dbo.TenantExamAnswers", new[] { "ExaminationQuestionId" });
            DropIndex("dbo.TenantExamAnswers", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "CEO_SystemUserId" });
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "RM_SystemUserId" });
            DropIndex("dbo.RiskAssessmentOutcomeAudits", new[] { "CEO_SystemUserId" });
            DropIndex("dbo.RiskAssessmentOutcomeAudits", new[] { "RM_SystemUserId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PreferredTypologyId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PreferredComplexId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseWaitingLists", new[] { "DepartmentId" });
            DropIndex("dbo.PreferredComplexAreaAudits", new[] { "MaintenanceManagerId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "DepartmentId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "RCSActionTypeId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "StatusId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "LeaseDetailsId" });
            DropIndex("dbo.LeaseReviewComments", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.ExaminationQuestions", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ExaminationQuestions", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ExaminationQuestions", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintExternalReferrals", new[] { "ReferredById" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintInvestigationDocuments", new[] { "ComplaintInvestigationId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ExternalReferralId" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "InvestigatedById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "ScheduledById" });
            DropIndex("dbo.ComplaintInvestigations", new[] { "TenantComplaintId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintEvidences", new[] { "UploadedById" });
            DropIndex("dbo.ComplaintEvidences", new[] { "TenantComplaintId" });
            DropIndex("dbo.ComplaintTypes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintTypes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintTypes", new[] { "DepartmentId" });
            DropIndex("dbo.ComplaintTypes", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.TenantComplaints", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.TenantComplaints", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TenantComplaints", new[] { "DepartmentId" });
            DropIndex("dbo.TenantComplaints", new[] { "AssignedToId" });
            DropIndex("dbo.TenantComplaints", new[] { "SubmittedByCustomerId" });
            DropIndex("dbo.TenantComplaints", new[] { "StatusId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintTypeId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplaintCategoryId" });
            DropIndex("dbo.TenantComplaints", new[] { "RespondentComplexId" });
            DropIndex("dbo.TenantComplaints", new[] { "ComplainantComplexId" });
            DropIndex("dbo.ComplaintCategories", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ComplaintCategories", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ComplaintCategories", new[] { "DepartmentId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "SupportingDocumentId" });
            DropIndex("dbo.MaintenanceJobCardTasks", new[] { "AllocatedUnitMaintenanceEHCId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "SignedByCustomerId" });
            DropIndex("dbo.MaintenanceJobCardSignatures", new[] { "AllocatedUnitMaintenanceEHCId" });
            DropIndex("dbo.AllocatedUnitMaintenanceEHCs", new[] { "AfterImageId" });
            DropIndex("dbo.AllocatedUnitMaintenanceEHCs", new[] { "BeforeImageId" });
            DropIndex("dbo.PreferredComplexAreas", new[] { "MaintenanceManagerId" });
            AlterColumn("dbo.PropertyLeaseApplications", "TotalCombinedIncome", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PropertyLeaseApplications", "NetIncome", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PropertyLeaseApplications", "GrossIncome", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.SystemUserAudits", "isActiveDirectoryUser");
            DropColumn("dbo.SystemUserAudits", "isInternalUser");
            DropColumn("dbo.RiskAssessmentOutcomes", "SignatureBlob");
            DropColumn("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId");
            DropColumn("dbo.RiskAssessmentOutcomes", "CEO_DateStamp");
            DropColumn("dbo.RiskAssessmentOutcomes", "CEO_Reason");
            DropColumn("dbo.RiskAssessmentOutcomes", "CEO_Outcome");
            DropColumn("dbo.RiskAssessmentOutcomes", "CEO_OfficialNumber");
            DropColumn("dbo.RiskAssessmentOutcomes", "RM_SystemUserId");
            DropColumn("dbo.RiskAssessmentOutcomes", "RM_DateStamp");
            DropColumn("dbo.RiskAssessmentOutcomes", "RM_Reason");
            DropColumn("dbo.RiskAssessmentOutcomes", "RM_Outcome");
            DropColumn("dbo.RiskAssessmentOutcomes", "RM_OfficialNumber");
            DropColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "SignatureBlob");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_SystemUserId");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_DateStamp");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_Reason");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_Outcome");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "CEO_OfficialNumber");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "RM_SystemUserId");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "RM_DateStamp");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "RM_Reason");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "RM_Outcome");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "RM_OfficialNumber");
            DropColumn("dbo.RiskAssessmentOutcomeAudits", "SupportingDoccuments");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "TenantBankName");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "Witness1Signature");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "RevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "PropertyManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBranchCode");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountType");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountHolderName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantAccountNumber");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "TenantBankName");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "CommencementDay");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "RevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "PropertyManagerSignatureDate");
            DropColumn("dbo.PreferredComplexAreaAudits", "MaintenanceManagerId");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "Inspection");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmittedDate");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "JobCardSubmitted");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "AfterImageId");
            DropColumn("dbo.AllocatedUnitMaintenanceEHCs", "BeforeImageId");
            DropColumn("dbo.PropertyLeaseApplications", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplications", "HasDSTV");
            DropColumn("dbo.PreferredComplexAreas", "MaintenanceManagerId");
            DropTable("dbo.TrainingSlides");
            DropTable("dbo.TrainingSlideAudits");
            DropTable("dbo.TenantTrainings");
            DropTable("dbo.TenantTrainingAudits");
            DropTable("dbo.TenantExamAnswers");
            DropTable("dbo.TenantExamAnswerAudits");
            DropTable("dbo.PropertyLeaseWaitingLists");
            DropTable("dbo.LeaseReviewComments");
            DropTable("dbo.ExaminationQuestions");
            DropTable("dbo.ExaminationQuestionAudits");
            DropTable("dbo.ComplaintExternalReferrals");
            DropTable("dbo.ComplaintInvestigationDocuments");
            DropTable("dbo.ComplaintInvestigations");
            DropTable("dbo.ComplaintEvidences");
            DropTable("dbo.ComplaintTypes");
            DropTable("dbo.TenantComplaints");
            DropTable("dbo.ComplaintCategories");
            DropTable("dbo.ApplicationsEntities");
            DropTable("dbo.MaintenanceJobCardTasks");
            DropTable("dbo.MaintenanceJobCardSignatures");
            CreateIndex("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id");
            CreateIndex("dbo.RiskAssessmentOutcomeAudits", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", "dbo.Files", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomeAudits", "Id", "dbo.Files", "Id");
        }
    }
}
