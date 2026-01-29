namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", "dbo.Files");
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "SupportingDoccuments_Id" });
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
            AddColumn("dbo.SystemUserAudits", "isInternalUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.SystemUserAudits", "isActiveDirectoryUser", c => c.Boolean(nullable: false));
            CreateIndex("dbo.RiskAssessmentOutcomes", "RM_SystemUserId");
            CreateIndex("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId", "dbo.SystemUsers", "Id");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "RM_SystemUserId", "dbo.SystemUsers", "Id");
            DropColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", c => c.Int());
            DropForeignKey("dbo.RiskAssessmentOutcomes", "RM_SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.RiskAssessmentOutcomes", "CEO_SystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "CEO_SystemUserId" });
            DropIndex("dbo.RiskAssessmentOutcomes", new[] { "RM_SystemUserId" });
            DropColumn("dbo.SystemUserAudits", "isActiveDirectoryUser");
            DropColumn("dbo.SystemUserAudits", "isInternalUser");
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
            DropTable("dbo.ApplicationsEntities");
            CreateIndex("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id");
            AddForeignKey("dbo.RiskAssessmentOutcomes", "SupportingDoccuments_Id", "dbo.Files", "Id");
        }
    }
}
