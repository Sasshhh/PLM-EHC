namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnewdocumentcolumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DocumentAudits", "WarningDocRefId", c => c.Int());
            AddColumn("dbo.Documents", "WarningDocRefId", c => c.Int());
            AddColumn("dbo.PropertyLeaseActionComments", "WarningDocRefId", c => c.Int());
            AddColumn("dbo.PropertyLeaseActionCommentsAudits", "WarningDocRefId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseActionCommentsAudits", "WarningDocRefId");
            DropColumn("dbo.PropertyLeaseActionComments", "WarningDocRefId");
            DropColumn("dbo.Documents", "WarningDocRefId");
            DropColumn("dbo.DocumentAudits", "WarningDocRefId");
        }
    }
}
