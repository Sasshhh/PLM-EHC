namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTenantComplaintToSchedules : DbMigration
    {
        public override void Up()
        {
            // AddColumn("dbo.MaintenanceJobCardSignatures", "SignatureRole", c => c.String(maxLength: 50));
            // AddColumn("dbo.TenantComplaints", "EscalationTriggered", c => c.Boolean(nullable: false));
            // AddColumn("dbo.TenantComplaints", "EscalationDate", c => c.DateTime());
            AddColumn("dbo.DateToSchedules", "TenantComplaintId", c => c.Int());
            AddColumn("dbo.InspectionSchedules", "TenantComplaintId", c => c.Int());
            CreateIndex("dbo.DateToSchedules", "TenantComplaintId");
            CreateIndex("dbo.InspectionSchedules", "TenantComplaintId");
            AddForeignKey("dbo.DateToSchedules", "TenantComplaintId", "dbo.TenantComplaints", "Id");
            AddForeignKey("dbo.InspectionSchedules", "TenantComplaintId", "dbo.TenantComplaints", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InspectionSchedules", "TenantComplaintId", "dbo.TenantComplaints");
            DropForeignKey("dbo.DateToSchedules", "TenantComplaintId", "dbo.TenantComplaints");
            DropIndex("dbo.InspectionSchedules", new[] { "TenantComplaintId" });
            DropIndex("dbo.DateToSchedules", new[] { "TenantComplaintId" });
            DropColumn("dbo.InspectionSchedules", "TenantComplaintId");
            DropColumn("dbo.DateToSchedules", "TenantComplaintId");
            // DropColumn("dbo.TenantComplaints", "EscalationDate");
            // DropColumn("dbo.TenantComplaints", "EscalationTriggered");
            // DropColumn("dbo.MaintenanceJobCardSignatures", "SignatureRole");
        }
    }
}
