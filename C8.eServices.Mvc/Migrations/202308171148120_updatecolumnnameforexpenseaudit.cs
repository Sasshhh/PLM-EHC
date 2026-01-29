namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatecolumnnameforexpenseaudit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MonthlyExpenseAudits", "LESupportMaintenance", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.MonthlyExpenseAudits", "LELESupportMaintenance");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MonthlyExpenseAudits", "LELESupportMaintenance", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.MonthlyExpenseAudits", "LESupportMaintenance");
        }
    }
}
