namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddEscalationToTenantComplaint : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TenantComplaints", "EscalationTriggered", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.TenantComplaints", "EscalationDate", c => c.DateTime());
        }

        public override void Down()
        {
            DropColumn("dbo.TenantComplaints", "EscalationDate");
            DropColumn("dbo.TenantComplaints", "EscalationTriggered");
        }
    }
}
