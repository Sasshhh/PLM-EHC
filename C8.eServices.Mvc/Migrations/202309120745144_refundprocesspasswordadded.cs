namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class refundprocesspasswordadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseApplications", "RefundProcessPassword", c => c.String());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "RefundProcessPassword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseApplicationAudits", "RefundProcessPassword");
            DropColumn("dbo.PropertyLeaseApplications", "RefundProcessPassword");
        }
    }
}
