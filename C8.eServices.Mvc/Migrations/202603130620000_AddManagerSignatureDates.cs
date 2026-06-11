namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddManagerSignatureDates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseAgreementMaster", "PropertyManagerSignatureDate", c => c.DateTime());
            AddColumn("dbo.PropertyLeaseAgreementMaster", "RevenueManagerSignatureDate", c => c.DateTime());
        }

        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseAgreementMaster", "RevenueManagerSignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMaster", "PropertyManagerSignatureDate");
        }
    }
}
