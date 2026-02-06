-- =============================================
-- TENANT TRAINING SYSTEM - DATABASE MIGRATION
-- Creates 4 main tables + 4 audit tables + 4 new status entries
-- Generated: 2025-01-15
-- =============================================

USE [CRMPLMDEV_2025]
GO

-- =============================================
-- STEP 1: CREATE MAIN TABLES
-- =============================================

-- Table 1: TenantTraining (Tracks user progress)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantTrainings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantTrainings](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropertyLeaseApplicationId] [int] NOT NULL,
        [TrainingInvitedDate] [datetime] NULL,
        [TrainingCompletedDate] [datetime] NULL,
        [ExamAttempts] [int] NOT NULL DEFAULT 0,
        [ExamPassedDate] [datetime] NULL,
        [ExamScore] [decimal](5, 2) NULL,
        [CurrentSlideIndex] [int] NOT NULL DEFAULT 0,
        [IsTrainingCompleted] [bit] NOT NULL DEFAULT 0,
        [IsExamPassed] [bit] NOT NULL DEFAULT 0,
        [TrainingLinkToken] [nvarchar](500) NULL,
        [TokenExpiryDate] [datetime] NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [IsLocked] [bit] NOT NULL DEFAULT 0,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TenantTrainings] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TenantTrainings_PropertyLeaseApplications] FOREIGN KEY ([PropertyLeaseApplicationId])
            REFERENCES [dbo].[PropertyLeaseApplications] ([Id])
    )
    PRINT 'Table TenantTrainings created successfully'
END
ELSE
    PRINT 'Table TenantTrainings already exists'
GO

-- Table 2: TrainingSlides (24 slides)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrainingSlides]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TrainingSlides](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SlideNumber] [int] NOT NULL,
        [Title] [nvarchar](500) NULL,
        [Content] [nvarchar](max) NULL,
        [ImagePath] [nvarchar](500) NULL,
        [DisplayOrder] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [IsLocked] [bit] NOT NULL DEFAULT 0,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TrainingSlides] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_TrainingSlides_SlideNumber] ON [dbo].[TrainingSlides] ([SlideNumber] ASC)
    PRINT 'Table TrainingSlides created successfully'
END
ELSE
    PRINT 'Table TrainingSlides already exists'
GO

-- Table 3: ExaminationQuestions (15 questions)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExaminationQuestions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExaminationQuestions](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [QuestionText] [nvarchar](max) NOT NULL,
        [OptionA] [nvarchar](500) NULL,
        [OptionB] [nvarchar](500) NULL,
        [OptionC] [nvarchar](500) NULL,
        [OptionD] [nvarchar](500) NULL,
        [CorrectAnswer] [nvarchar](1) NOT NULL,
        [QuestionOrder] [int] NOT NULL,
        [IsExampleQuestion] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [IsLocked] [bit] NOT NULL DEFAULT 0,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_ExaminationQuestions] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_ExaminationQuestions_QuestionOrder] ON [dbo].[ExaminationQuestions] ([QuestionOrder] ASC)
    PRINT 'Table ExaminationQuestions created successfully'
END
ELSE
    PRINT 'Table ExaminationQuestions already exists'
GO

-- Table 4: TenantExamAnswers (User responses)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantExamAnswers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantExamAnswers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropertyLeaseApplicationId] [int] NOT NULL,
        [ExaminationQuestionId] [int] NOT NULL,
        [SelectedAnswer] [nvarchar](1) NOT NULL,
        [IsCorrect] [bit] NOT NULL DEFAULT 0,
        [AnsweredDateTime] [datetime] NOT NULL DEFAULT GETDATE(),
        [AttemptNumber] [int] NOT NULL DEFAULT 1,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [IsLocked] [bit] NOT NULL DEFAULT 0,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NOT NULL DEFAULT GETDATE(),
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TenantExamAnswers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TenantExamAnswers_PropertyLeaseApplications] FOREIGN KEY ([PropertyLeaseApplicationId])
            REFERENCES [dbo].[PropertyLeaseApplications] ([Id]),
        CONSTRAINT [FK_TenantExamAnswers_ExaminationQuestions] FOREIGN KEY ([ExaminationQuestionId])
            REFERENCES [dbo].[ExaminationQuestions] ([Id])
    )
    CREATE NONCLUSTERED INDEX [IX_TenantExamAnswers_PropertyLeaseApplicationId] ON [dbo].[TenantExamAnswers] ([PropertyLeaseApplicationId] ASC)
    PRINT 'Table TenantExamAnswers created successfully'
END
ELSE
    PRINT 'Table TenantExamAnswers already exists'
GO

-- =============================================
-- STEP 2: CREATE AUDIT TABLES
-- =============================================

-- Audit Table 1: TenantTrainingAudits
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantTrainingAudits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantTrainingAudits](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Action] [nvarchar](50) NULL,
        [PropertyLeaseApplicationId] [int] NOT NULL,
        [TrainingInvitedDate] [datetime] NULL,
        [TrainingCompletedDate] [datetime] NULL,
        [ExamAttempts] [int] NULL,
        [ExamPassedDate] [datetime] NULL,
        [ExamScore] [decimal](5, 2) NULL,
        [CurrentSlideIndex] [int] NULL,
        [IsTrainingCompleted] [bit] NULL,
        [IsExamPassed] [bit] NULL,
        [TrainingLinkToken] [nvarchar](500) NULL,
        [TokenExpiryDate] [datetime] NULL,
        [IsActive] [bit] NULL,
        [IsDeleted] [bit] NULL,
        [IsLocked] [bit] NULL,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NULL,
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TenantTrainingAudits] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    PRINT 'Table TenantTrainingAudits created successfully'
END
ELSE
    PRINT 'Table TenantTrainingAudits already exists'
GO

-- Audit Table 2: TrainingSlideAudits
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrainingSlideAudits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TrainingSlideAudits](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Action] [nvarchar](50) NULL,
        [SlideNumber] [int] NULL,
        [Title] [nvarchar](500) NULL,
        [Content] [nvarchar](max) NULL,
        [ImagePath] [nvarchar](500) NULL,
        [DisplayOrder] [int] NULL,
        [IsActive] [bit] NULL,
        [IsDeleted] [bit] NULL,
        [IsLocked] [bit] NULL,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NULL,
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TrainingSlideAudits] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    PRINT 'Table TrainingSlideAudits created successfully'
END
ELSE
    PRINT 'Table TrainingSlideAudits already exists'
GO

-- Audit Table 3: ExaminationQuestionAudits
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExaminationQuestionAudits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExaminationQuestionAudits](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Action] [nvarchar](50) NULL,
        [QuestionText] [nvarchar](max) NULL,
        [OptionA] [nvarchar](500) NULL,
        [OptionB] [nvarchar](500) NULL,
        [OptionC] [nvarchar](500) NULL,
        [OptionD] [nvarchar](500) NULL,
        [CorrectAnswer] [nvarchar](1) NULL,
        [QuestionOrder] [int] NULL,
        [IsExampleQuestion] [bit] NULL,
        [IsActive] [bit] NULL,
        [IsDeleted] [bit] NULL,
        [IsLocked] [bit] NULL,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NULL,
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_ExaminationQuestionAudits] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    PRINT 'Table ExaminationQuestionAudits created successfully'
END
ELSE
    PRINT 'Table ExaminationQuestionAudits already exists'
GO

-- Audit Table 4: TenantExamAnswerAudits
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantExamAnswerAudits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantExamAnswerAudits](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Action] [nvarchar](50) NULL,
        [PropertyLeaseApplicationId] [int] NULL,
        [ExaminationQuestionId] [int] NULL,
        [SelectedAnswer] [nvarchar](1) NULL,
        [IsCorrect] [bit] NULL,
        [AnsweredDateTime] [datetime] NULL,
        [AttemptNumber] [int] NULL,
        [IsActive] [bit] NULL,
        [IsDeleted] [bit] NULL,
        [IsLocked] [bit] NULL,
        [CreatedBySystemUserId] [int] NULL,
        [CreatedDateTime] [datetime] NULL,
        [ModifiedBySystemUserId] [int] NULL,
        [ModifiedDateTime] [datetime] NULL,
        CONSTRAINT [PK_TenantExamAnswerAudits] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    PRINT 'Table TenantExamAnswerAudits created successfully'
END
ELSE
    PRINT 'Table TenantExamAnswerAudits already exists'
GO

-- =============================================
-- STEP 3: ADD NEW STATUS KEYS
-- =============================================

-- Add status keys for training workflow
IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_awaiting_online_training')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Name], [Description], [IsActive], [IsDeleted], [IsLocked], [CreatedDateTime], [ModifiedDateTime])
    VALUES ('s_awaiting_online_training', 'Awaiting Online Training', 'Tenant has been invited to complete online training', 1, 0, 0, GETDATE(), GETDATE())
    PRINT 'Status: s_awaiting_online_training added'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_training_in_progress')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Name], [Description], [IsActive], [IsDeleted], [IsLocked], [CreatedDateTime], [ModifiedDateTime])
    VALUES ('s_training_in_progress', 'Training In Progress', 'Tenant is currently completing the training programme', 1, 0, 0, GETDATE(), GETDATE())
    PRINT 'Status: s_training_in_progress added'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_training_completed')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Name], [Description], [IsActive], [IsDeleted], [IsLocked], [CreatedDateTime], [ModifiedDateTime])
    VALUES ('s_training_completed', 'Training Completed', 'Tenant has completed the training programme', 1, 0, 0, GETDATE(), GETDATE())
    PRINT 'Status: s_training_completed added'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_examination_passed')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Name], [Description], [IsActive], [IsDeleted], [IsLocked], [CreatedDateTime], [ModifiedDateTime])
    VALUES ('s_examination_passed', 'Examination Passed', 'Tenant has passed the examination with 100%', 1, 0, 0, GETDATE(), GETDATE())
    PRINT 'Status: s_examination_passed added'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Status] WHERE [Key] = 's_examination_failed')
BEGIN
    INSERT INTO [dbo].[Status] ([Key], [Name], [Description], [IsActive], [IsDeleted], [IsLocked], [CreatedDateTime], [ModifiedDateTime])
    VALUES ('s_examination_failed', 'Examination Failed', 'Tenant failed the examination', 1, 0, 0, GETDATE(), GETDATE())
    PRINT 'Status: s_examination_failed added'
END
GO

PRINT ''
PRINT '========================================='
PRINT 'DATABASE MIGRATION COMPLETED SUCCESSFULLY!'
PRINT '========================================='
PRINT 'Tables Created: 4 main + 4 audit = 8 tables'
PRINT 'Status Keys Added: 5'
PRINT ''
PRINT 'NEXT STEPS:'
PRINT '1. Run Insert_TrainingData.sql to populate slides and questions'
PRINT '2. Deploy the ASP.NET MVC code'
PRINT '3. Test the training workflow'
PRINT '========================================='
GO
