namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig : DbMigration
    {
        public override void Up()
        {
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
                        TrainingInvitedDate = c.DateTime(),
                        TrainingCompletedDate = c.DateTime(),
                        ExamAttempts = c.Int(nullable: false),
                        ExamPassedDate = c.DateTime(),
                        ExamScore = c.Decimal(precision: 18, scale: 2),
                        CurrentSlideIndex = c.Int(nullable: false),
                        IsTrainingCompleted = c.Boolean(nullable: false),
                        IsExamPassed = c.Boolean(nullable: false),
                        TrainingLinkToken = c.String(),
                        TokenExpiryDate = c.DateTime(),
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
                        TrainingInvitedDate = c.DateTime(),
                        TrainingCompletedDate = c.DateTime(),
                        ExamAttempts = c.Int(nullable: false),
                        ExamPassedDate = c.DateTime(),
                        ExamScore = c.Decimal(precision: 18, scale: 2),
                        CurrentSlideIndex = c.Int(nullable: false),
                        IsTrainingCompleted = c.Boolean(nullable: false),
                        IsExamPassed = c.Boolean(nullable: false),
                        TrainingLinkToken = c.String(maxLength: 500),
                        TokenExpiryDate = c.DateTime(),
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
            
        }
        
        public override void Down()
        {
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
            DropForeignKey("dbo.ExaminationQuestions", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.ExaminationQuestions", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.ExaminationQuestions", "CreatedBySystemUserId", "dbo.SystemUsers");
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
            DropIndex("dbo.ExaminationQuestions", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.ExaminationQuestions", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.ExaminationQuestions", new[] { "DepartmentId" });
            DropTable("dbo.TrainingSlides");
            DropTable("dbo.TrainingSlideAudits");
            DropTable("dbo.TenantTrainings");
            DropTable("dbo.TenantTrainingAudits");
            DropTable("dbo.TenantExamAnswers");
            DropTable("dbo.TenantExamAnswerAudits");
            DropTable("dbo.ExaminationQuestions");
            DropTable("dbo.ExaminationQuestionAudits");
        }
    }
}
