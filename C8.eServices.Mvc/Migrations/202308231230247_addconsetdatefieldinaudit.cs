namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addconsetdatefieldinaudit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseApplicationAudits", "ConsentDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseApplicationAudits", "ConsentDate");
        }
    }
}
