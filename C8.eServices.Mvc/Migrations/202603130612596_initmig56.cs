namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig56 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMasterAudits", "Witness1Signature");
        }
    }
}
