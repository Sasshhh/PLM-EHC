namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Logs", "LogEntry", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "LogEntry", c => c.String(maxLength: 100));
        }
    }
}
