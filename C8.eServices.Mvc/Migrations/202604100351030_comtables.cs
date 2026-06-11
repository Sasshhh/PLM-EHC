namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class comtables : DbMigration
    {
        public override void Up()
        {
            // =====================================================
            // ALTER EXISTING TABLES
            // =====================================================

            AlterColumn("dbo.TenantComplaints", "CaseReferenceNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.TenantComplaints", "WarningLetterCount", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.TenantComplaints", "LastWarningDate", c => c.DateTime());
            AddColumn("dbo.TenantComplaints", "LeaseTerminationTriggered", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.TenantComplaints", "LeaseTerminationDate", c => c.DateTime());

            AddColumn("dbo.ComplaintInvestigations", "ProposedAlternativeDate", c => c.DateTime());
            AddColumn("dbo.ComplaintInvestigations", "ProposedAlternativeTime", c => c.Time(precision: 7));
            AddColumn("dbo.ComplaintInvestigations", "AlternativeDateReason", c => c.String());
            AddColumn("dbo.ComplaintInvestigations", "AlternativeApproved", c => c.Boolean());

            AddColumn("dbo.RoundRobinQueues", "TenantComplaintId", c => c.Int());
            AddColumn("dbo.RoundRobinQueueAudits", "TenantComplaintId", c => c.Int());
            CreateIndex("dbo.RoundRobinQueues", "TenantComplaintId");
            AddForeignKey("dbo.RoundRobinQueues", "TenantComplaintId", "dbo.TenantComplaints", "Id");

            // =====================================================
            // CREATE NEW TABLES
            // =====================================================

            CreateTable(
                "dbo.ComplaintAuditLogs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    TenantComplaintId = c.Int(nullable: false),
                    Action = c.String(nullable: false, maxLength: 100),
                    Details = c.String(),
                    PerformedByCustomerId = c.Int(),
                    PerformedAt = c.DateTime(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TenantComplaints", t => t.TenantComplaintId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.PerformedByCustomerId)
                .Index(t => t.TenantComplaintId)
                .Index(t => t.PerformedByCustomerId);

            CreateTable(
                "dbo.PaymentTransgressionCategories",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    DisplayOrder = c.Int(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.PaymentTransgressionSeverities",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    Level = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.PaymentTransgressionTypes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    PaymentTransgressionCategoryId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PaymentTransgressionCategories", t => t.PaymentTransgressionCategoryId, cascadeDelete: false)
                .Index(t => t.PaymentTransgressionCategoryId);

            CreateTable(
                "dbo.PaymentTransgressions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CaseReferenceNumber = c.String(nullable: false, maxLength: 50),
                    OfficialNumber = c.String(nullable: false, maxLength: 50),
                    TenancyReferenceNumber = c.String(nullable: false, maxLength: 50),
                    TenantName = c.String(nullable: false, maxLength: 100),
                    TenantSurname = c.String(nullable: false, maxLength: 100),
                    TenantEmail = c.String(maxLength: 100),
                    TenantCellphone = c.String(maxLength: 20),
                    ComplexId = c.Int(nullable: false),
                    BlockNumber = c.String(maxLength: 50),
                    UnitNumber = c.String(maxLength: 50),
                    AccountNumber = c.String(maxLength: 50),
                    LastPaymentAmount = c.Decimal(precision: 18, scale: 2),
                    LastPaymentDate = c.DateTime(),
                    TotalAmountDue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    PaymentTransgressionCategoryId = c.Int(nullable: false),
                    PaymentTransgressionTypeId = c.Int(nullable: false),
                    PaymentTransgressionSeverityId = c.Int(nullable: false),
                    DetailedDescription = c.String(nullable: false),
                    LetterType = c.String(maxLength: 100),
                    LetterGeneratedDate = c.DateTime(),
                    LetterSentDate = c.DateTime(),
                    LetterFilePath = c.String(maxLength: 500),
                    StatusId = c.Int(nullable: false),
                    DateSubmitted = c.DateTime(nullable: false),
                    AssignedToCustomerId = c.Int(),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PaymentTransgressionCategories", t => t.PaymentTransgressionCategoryId, cascadeDelete: false)
                .ForeignKey("dbo.PaymentTransgressionTypes", t => t.PaymentTransgressionTypeId, cascadeDelete: false)
                .ForeignKey("dbo.PaymentTransgressionSeverities", t => t.PaymentTransgressionSeverityId, cascadeDelete: false)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.ComplexId, cascadeDelete: false)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.AssignedToCustomerId)
                .Index(t => t.PaymentTransgressionCategoryId)
                .Index(t => t.PaymentTransgressionTypeId)
                .Index(t => t.PaymentTransgressionSeverityId)
                .Index(t => t.ComplexId)
                .Index(t => t.StatusId)
                .Index(t => t.AssignedToCustomerId);

            CreateTable(
                "dbo.PaymentTransgressionDocuments",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PaymentTransgressionId = c.Int(nullable: false),
                    FileName = c.String(nullable: false, maxLength: 255),
                    FilePath = c.String(nullable: false, maxLength: 500),
                    FileType = c.String(maxLength: 100),
                    FileSize = c.Long(nullable: false),
                    UploadedDate = c.DateTime(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PaymentTransgressions", t => t.PaymentTransgressionId, cascadeDelete: false)
                .Index(t => t.PaymentTransgressionId);

            CreateTable(
                "dbo.PaymentTransgressionAuditLogs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PaymentTransgressionId = c.Int(nullable: false),
                    Action = c.String(nullable: false, maxLength: 100),
                    Details = c.String(),
                    PerformedByCustomerId = c.Int(),
                    PerformedAt = c.DateTime(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PaymentTransgressions", t => t.PaymentTransgressionId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.PerformedByCustomerId)
                .Index(t => t.PaymentTransgressionId)
                .Index(t => t.PerformedByCustomerId);

            CreateTable(
                "dbo.ServiceRequestCategories",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    DisplayOrder = c.Int(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ServiceRequestPriorities",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Key = c.String(nullable: false, maxLength: 100),
                    Name = c.String(nullable: false, maxLength: 200),
                    Description = c.String(),
                    Level = c.Int(nullable: false),
                    ResponseTimeMinutes = c.Int(nullable: false),
                    ResolutionTimeHours = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ServiceRequests",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RequestReferenceNumber = c.String(nullable: false, maxLength: 50),
                    ReportedByName = c.String(nullable: false, maxLength: 100),
                    ReportedBySurname = c.String(nullable: false, maxLength: 100),
                    ContactNumber = c.String(maxLength: 20),
                    EmailAddress = c.String(nullable: false, maxLength: 100),
                    ComplexId = c.Int(nullable: false),
                    BlockNumber = c.String(maxLength: 50),
                    UnitNumber = c.String(maxLength: 50),
                    RoomType = c.String(maxLength: 100),
                    ServiceRequestCategoryId = c.Int(nullable: false),
                    ServiceRequestPriorityId = c.Int(),
                    DetailedDescription = c.String(nullable: false),
                    StatusId = c.Int(nullable: false),
                    DateSubmitted = c.DateTime(nullable: false),
                    ResponseDeadline = c.DateTime(),
                    ResolutionDeadline = c.DateTime(),
                    EscalationTriggered = c.Boolean(nullable: false),
                    DateResolved = c.DateTime(),
                    DateClosed = c.DateTime(),
                    CreatedByCustomerId = c.Int(),
                    DeletionReason = c.String(),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceRequestCategories", t => t.ServiceRequestCategoryId, cascadeDelete: false)
                .ForeignKey("dbo.ServiceRequestPriorities", t => t.ServiceRequestPriorityId)
                .ForeignKey("dbo.PreferredComplexAreas", t => t.ComplexId, cascadeDelete: false)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.CreatedByCustomerId)
                .Index(t => t.ServiceRequestCategoryId)
                .Index(t => t.ServiceRequestPriorityId)
                .Index(t => t.ComplexId)
                .Index(t => t.StatusId)
                .Index(t => t.CreatedByCustomerId);

            CreateTable(
                "dbo.ServiceRequestDocuments",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ServiceRequestId = c.Int(nullable: false),
                    FileName = c.String(nullable: false, maxLength: 255),
                    FilePath = c.String(nullable: false, maxLength: 500),
                    FileType = c.String(maxLength: 100),
                    FileSize = c.Long(nullable: false),
                    UploadedDate = c.DateTime(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceRequests", t => t.ServiceRequestId, cascadeDelete: false)
                .Index(t => t.ServiceRequestId);

            CreateTable(
                "dbo.ServiceRequestAuditLogs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ServiceRequestId = c.Int(nullable: false),
                    Action = c.String(nullable: false, maxLength: 100),
                    Details = c.String(),
                    PerformedByCustomerId = c.Int(),
                    PerformedAt = c.DateTime(nullable: false),
                    DepartmentId = c.Int(),
                    IsActive = c.Boolean(nullable: false, defaultValue: true),
                    IsDeleted = c.Boolean(nullable: false, defaultValue: false),
                    IsLocked = c.Boolean(),
                    CreatedBySystemUserId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedBySystemUserId = c.Int(),
                    ModifiedDateTime = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceRequests", t => t.ServiceRequestId, cascadeDelete: false)
                .ForeignKey("dbo.Customers", t => t.PerformedByCustomerId)
                .Index(t => t.ServiceRequestId)
                .Index(t => t.PerformedByCustomerId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ServiceRequestAuditLogs", "PerformedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.ServiceRequestAuditLogs", "ServiceRequestId", "dbo.ServiceRequests");
            DropIndex("dbo.ServiceRequestAuditLogs", new[] { "PerformedByCustomerId" });
            DropIndex("dbo.ServiceRequestAuditLogs", new[] { "ServiceRequestId" });
            DropTable("dbo.ServiceRequestAuditLogs");

            DropForeignKey("dbo.ServiceRequestDocuments", "ServiceRequestId", "dbo.ServiceRequests");
            DropIndex("dbo.ServiceRequestDocuments", new[] { "ServiceRequestId" });
            DropTable("dbo.ServiceRequestDocuments");

            DropForeignKey("dbo.ServiceRequests", "CreatedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.ServiceRequests", "StatusId", "dbo.Status");
            DropForeignKey("dbo.ServiceRequests", "ComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.ServiceRequests", "ServiceRequestPriorityId", "dbo.ServiceRequestPriorities");
            DropForeignKey("dbo.ServiceRequests", "ServiceRequestCategoryId", "dbo.ServiceRequestCategories");
            DropIndex("dbo.ServiceRequests", new[] { "CreatedByCustomerId" });
            DropIndex("dbo.ServiceRequests", new[] { "StatusId" });
            DropIndex("dbo.ServiceRequests", new[] { "ComplexId" });
            DropIndex("dbo.ServiceRequests", new[] { "ServiceRequestPriorityId" });
            DropIndex("dbo.ServiceRequests", new[] { "ServiceRequestCategoryId" });
            DropTable("dbo.ServiceRequests");

            DropTable("dbo.ServiceRequestPriorities");
            DropTable("dbo.ServiceRequestCategories");

            DropForeignKey("dbo.PaymentTransgressionAuditLogs", "PerformedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.PaymentTransgressionAuditLogs", "PaymentTransgressionId", "dbo.PaymentTransgressions");
            DropIndex("dbo.PaymentTransgressionAuditLogs", new[] { "PerformedByCustomerId" });
            DropIndex("dbo.PaymentTransgressionAuditLogs", new[] { "PaymentTransgressionId" });
            DropTable("dbo.PaymentTransgressionAuditLogs");

            DropForeignKey("dbo.PaymentTransgressionDocuments", "PaymentTransgressionId", "dbo.PaymentTransgressions");
            DropIndex("dbo.PaymentTransgressionDocuments", new[] { "PaymentTransgressionId" });
            DropTable("dbo.PaymentTransgressionDocuments");

            DropForeignKey("dbo.PaymentTransgressions", "AssignedToCustomerId", "dbo.Customers");
            DropForeignKey("dbo.PaymentTransgressions", "StatusId", "dbo.Status");
            DropForeignKey("dbo.PaymentTransgressions", "ComplexId", "dbo.PreferredComplexAreas");
            DropForeignKey("dbo.PaymentTransgressions", "PaymentTransgressionSeverityId", "dbo.PaymentTransgressionSeverities");
            DropForeignKey("dbo.PaymentTransgressions", "PaymentTransgressionTypeId", "dbo.PaymentTransgressionTypes");
            DropForeignKey("dbo.PaymentTransgressions", "PaymentTransgressionCategoryId", "dbo.PaymentTransgressionCategories");
            DropIndex("dbo.PaymentTransgressions", new[] { "AssignedToCustomerId" });
            DropIndex("dbo.PaymentTransgressions", new[] { "StatusId" });
            DropIndex("dbo.PaymentTransgressions", new[] { "ComplexId" });
            DropIndex("dbo.PaymentTransgressions", new[] { "PaymentTransgressionSeverityId" });
            DropIndex("dbo.PaymentTransgressions", new[] { "PaymentTransgressionTypeId" });
            DropIndex("dbo.PaymentTransgressions", new[] { "PaymentTransgressionCategoryId" });
            DropTable("dbo.PaymentTransgressions");

            DropForeignKey("dbo.PaymentTransgressionTypes", "PaymentTransgressionCategoryId", "dbo.PaymentTransgressionCategories");
            DropIndex("dbo.PaymentTransgressionTypes", new[] { "PaymentTransgressionCategoryId" });
            DropTable("dbo.PaymentTransgressionTypes");

            DropTable("dbo.PaymentTransgressionSeverities");
            DropTable("dbo.PaymentTransgressionCategories");

            DropForeignKey("dbo.ComplaintAuditLogs", "PerformedByCustomerId", "dbo.Customers");
            DropForeignKey("dbo.ComplaintAuditLogs", "TenantComplaintId", "dbo.TenantComplaints");
            DropIndex("dbo.ComplaintAuditLogs", new[] { "PerformedByCustomerId" });
            DropIndex("dbo.ComplaintAuditLogs", new[] { "TenantComplaintId" });
            DropTable("dbo.ComplaintAuditLogs");

            DropForeignKey("dbo.RoundRobinQueues", "TenantComplaintId", "dbo.TenantComplaints");
            DropIndex("dbo.RoundRobinQueues", new[] { "TenantComplaintId" });
            DropColumn("dbo.RoundRobinQueues", "TenantComplaintId");
            DropColumn("dbo.RoundRobinQueueAudits", "TenantComplaintId");

            DropColumn("dbo.ComplaintInvestigations", "AlternativeApproved");
            DropColumn("dbo.ComplaintInvestigations", "AlternativeDateReason");
            DropColumn("dbo.ComplaintInvestigations", "ProposedAlternativeTime");
            DropColumn("dbo.ComplaintInvestigations", "ProposedAlternativeDate");

            DropColumn("dbo.TenantComplaints", "LeaseTerminationDate");
            DropColumn("dbo.TenantComplaints", "LeaseTerminationTriggered");
            DropColumn("dbo.TenantComplaints", "LastWarningDate");
            DropColumn("dbo.TenantComplaints", "WarningLetterCount");
            AlterColumn("dbo.TenantComplaints", "CaseReferenceNumber", c => c.String(nullable: false, maxLength: 50));
        }
    }
}

