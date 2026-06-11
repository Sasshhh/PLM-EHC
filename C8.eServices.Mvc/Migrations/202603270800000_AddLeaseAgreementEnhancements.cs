namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLeaseAgreementEnhancements : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay", c => c.String(maxLength: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseAgreementMasters", "CommencementDay");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "HasDSTV");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVMonthlyLevy");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "DSTVActivationFee");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "KeyDeposit");
            DropColumn("dbo.PropertyLeaseAgreementMasters", "AccessCardDeposit");
        }
    }
}