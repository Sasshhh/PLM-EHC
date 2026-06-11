namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig34 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EvictionServiceRecords",
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
                        LeaseDetailsId = c.Int(),
                        EvictionReferenceNumber = c.String(maxLength: 50),
                        ServiceMethod = c.String(maxLength: 100),
                        ServiceDate = c.DateTime(),
                        OfficialNumber = c.String(maxLength: 50),
                        NoticeServed = c.Boolean(nullable: false),
                        ProofOfServiceType = c.String(maxLength: 100),
                        ProofServiceDate = c.DateTime(),
                        ProofComments = c.String(maxLength: 2000),
                        ProofCaptured = c.Boolean(nullable: false),
                        StatusId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.LeaseDetails", t => t.LeaseDetailsId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.LeaseDetailsId)
                .Index(t => t.StatusId);
            
            CreateTable(
                "dbo.LeaseDisputes",
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
                        LeaseDetailsId = c.Int(),
                        DisputeReferenceNumber = c.String(maxLength: 50),
                        ReportedByName = c.String(maxLength: 200),
                        ContactNumber = c.String(maxLength: 50),
                        EmailAddress = c.String(maxLength: 200),
                        ComplexName = c.String(maxLength: 200),
                        BlockNumber = c.String(maxLength: 50),
                        UnitNumber = c.String(maxLength: 50),
                        Category = c.String(maxLength: 100),
                        SubCategory = c.String(maxLength: 200),
                        Description = c.String(maxLength: 4000),
                        RiskClassification = c.String(maxLength: 100),
                        ReviewComment = c.String(maxLength: 2000),
                        OfficialNumber = c.String(maxLength: 50),
                        LegalAgencyName = c.String(maxLength: 200),
                        LegalAgencyAddress = c.String(maxLength: 500),
                        LegalAgencyContact = c.String(maxLength: 100),
                        LegalAgencyEmail = c.String(maxLength: 200),
                        LegalBriefDescription = c.String(maxLength: 2000),
                        ResolutionOutcomeType = c.String(maxLength: 200),
                        ResolutionSummary = c.String(maxLength: 4000),
                        IsResolved = c.Boolean(),
                        NotResolvedReason = c.String(maxLength: 2000),
                        RevenueManagerSignature = c.String(),
                        RevenueManagerSignDate = c.DateTime(),
                        ClosureOutcome = c.String(maxLength: 200),
                        ClosureSummary = c.String(maxLength: 4000),
                        IsClosed = c.Boolean(),
                        RejectionOption = c.String(maxLength: 200),
                        RejectionReason = c.String(maxLength: 2000),
                        CEOSignature = c.String(),
                        CEOSignDate = c.DateTime(),
                        StatusId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.LeaseDetails", t => t.LeaseDetailsId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.LeaseDetailsId)
                .Index(t => t.StatusId);
            
            AddColumn("dbo.LeaseTerminations", "TerminationReferenceNumber", c => c.String());
            AddColumn("dbo.LeaseTerminations", "ClauseReference", c => c.String());
            AddColumn("dbo.LeaseTerminations", "NoticePeriod", c => c.String());
            AddColumn("dbo.LeaseTerminations", "OfficialNumber", c => c.String());
            AddColumn("dbo.LeaseTerminations", "EvictionReferenceNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LeaseDisputes", "StatusId", "dbo.Status");
            DropForeignKey("dbo.LeaseDisputes", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.LeaseDisputes", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.LeaseDisputes", "LeaseDetailsId", "dbo.LeaseDetails");
            DropForeignKey("dbo.LeaseDisputes", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.LeaseDisputes", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.EvictionServiceRecords", "StatusId", "dbo.Status");
            DropForeignKey("dbo.EvictionServiceRecords", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.EvictionServiceRecords", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.EvictionServiceRecords", "LeaseDetailsId", "dbo.LeaseDetails");
            DropForeignKey("dbo.EvictionServiceRecords", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.EvictionServiceRecords", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.LeaseDisputes", new[] { "StatusId" });
            DropIndex("dbo.LeaseDisputes", new[] { "LeaseDetailsId" });
            DropIndex("dbo.LeaseDisputes", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.LeaseDisputes", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.LeaseDisputes", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.LeaseDisputes", new[] { "DepartmentId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "StatusId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "LeaseDetailsId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.EvictionServiceRecords", new[] { "DepartmentId" });
            DropColumn("dbo.LeaseTerminations", "EvictionReferenceNumber");
            DropColumn("dbo.LeaseTerminations", "OfficialNumber");
            DropColumn("dbo.LeaseTerminations", "NoticePeriod");
            DropColumn("dbo.LeaseTerminations", "ClauseReference");
            DropColumn("dbo.LeaseTerminations", "TerminationReferenceNumber");
            DropTable("dbo.LeaseDisputes");
            DropTable("dbo.EvictionServiceRecords");
        }
    }
}
