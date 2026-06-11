namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDSTVAndDepositFieldsToPropertyLeaseApplication : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseApplications", "HasDSTV", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplications", "KeyDeposit", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            
            // Add the same columns to the audit table
            AddColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit", c => c.Decimal(nullable: true, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            // Drop audit table columns first
            DropColumn("dbo.PropertyLeaseApplicationAudits", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplicationAudits", "HasDSTV");
            
            // Drop main table columns
            DropColumn("dbo.PropertyLeaseApplications", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "AccessCardDeposit");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseApplications", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseApplications", "HasDSTV");
        }
    }
}