namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWitness1SignatureToLeaseAgreementMaster : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PropertyLeaseAgreementMaster", "Witness1Signature", c => c.String());
            AddColumn("dbo.PropertyLeaseAgreementMaster", "Witness1Name", c => c.String(maxLength: 200));
            AddColumn("dbo.PropertyLeaseAgreementMaster", "Witness1SignatureDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PropertyLeaseAgreementMaster", "Witness1SignatureDate");
            DropColumn("dbo.PropertyLeaseAgreementMaster", "Witness1Name");
            DropColumn("dbo.PropertyLeaseAgreementMaster", "Witness1Signature");
        }
    }
}
