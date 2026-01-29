namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddnewFieldsInPLMTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseApplications", "LivingAddressPeriod", c => c.String());
            AddColumn("dbo.PropertyLeaseApplications", "AgentName", c => c.String());
            AddColumn("dbo.PropertyLeaseApplications", "AgentPhoneNumber", c => c.String(maxLength: 15));
            AddColumn("dbo.PropertyLeaseApplications", "CoPhoneNumber", c => c.String(maxLength: 15));
            AddColumn("dbo.PropertyLeaseApplications", "CoFax", c => c.String());
            AddColumn("dbo.PropertyLeaseApplications", "CoPeriodWorking", c => c.String());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "LivingAddressPeriod", c => c.String());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "AgentName", c => c.String());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "AgentPhoneNumber", c => c.String(maxLength: 15));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "CoPhoneNumber", c => c.String(maxLength: 15));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "CoFax", c => c.String());
            AddColumn("dbo.PropertyLeaseApplicationAudits", "CoPeriodWorking", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseApplicationAudits", "CoPeriodWorking");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "CoFax");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "CoPhoneNumber");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "AgentPhoneNumber");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "AgentName");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "LivingAddressPeriod");
            DropColumn("dbo.PropertyLeaseApplications", "CoPeriodWorking");
            DropColumn("dbo.PropertyLeaseApplications", "CoFax");
            DropColumn("dbo.PropertyLeaseApplications", "CoPhoneNumber");
            DropColumn("dbo.PropertyLeaseApplications", "AgentPhoneNumber");
            DropColumn("dbo.PropertyLeaseApplications", "AgentName");
            DropColumn("dbo.PropertyLeaseApplications", "LivingAddressPeriod");
        }
    }
}
