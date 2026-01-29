namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addconsetdatefield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseApplications", "ConsentDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseApplications", "ConsentDate");
        }
    }
}
