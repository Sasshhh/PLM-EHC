namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sync_All_Missing_Changes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean());
            AlterColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV", c => c.Boolean());
            AlterColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean());
            AlterColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseAgreementMasterAudits", "HasDSTV", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean(nullable: false));
        }
    }
}
