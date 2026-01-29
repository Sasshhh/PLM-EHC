namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initmig : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TransferInformations", new[] { "TransferType" });
            CreateTable(
                "dbo.PropertyLeaseApplicationAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        Action = c.String(maxLength: 10),
                        Id = c.Int(nullable: false),
                        ApplicantType = c.Int(nullable: false),
                        Gender = c.String(maxLength: 10),
                        Title = c.String(maxLength: 10),
                        MaritalStatus = c.String(maxLength: 20),
                        Initial = c.String(maxLength: 10),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        IDNo = c.String(maxLength: 25),
                        DOB = c.DateTime(storeType: "date"),
                        Nationality = c.String(maxLength: 50),
                        CellNo = c.String(maxLength: 15),
                        HomeNo = c.String(maxLength: 15),
                        WorkNo = c.String(maxLength: 15),
                        PurEmail = c.String(maxLength: 50),
                        ResAddress = c.String(maxLength: 50),
                        ResSuburb = c.String(maxLength: 50),
                        ResPostal = c.String(maxLength: 10),
                        IncomeSourcee = c.String(maxLength: 20),
                        SassaNumber = c.String(maxLength: 50),
                        GrossIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalCombinedIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HouseRequired = c.String(maxLength: 50),
                        PrefArea = c.String(maxLength: 20),
                        CompanyName = c.String(maxLength: 100),
                        NameTradingAs = c.String(maxLength: 100),
                        TrustRegNo = c.String(maxLength: 100),
                        TrustName = c.String(maxLength: 100),
                        CompanyType = c.String(maxLength: 100),
                        CIPCRegistrationNo = c.String(maxLength: 100),
                        CoAddress = c.String(maxLength: 50),
                        CoSuburb = c.String(maxLength: 50),
                        CoPostal = c.String(maxLength: 10),
                        DirectorName = c.String(maxLength: 300),
                        DirectorSName = c.String(maxLength: 300),
                        DirectorIdNo = c.String(maxLength: 300),
                        ContactPSurname = c.String(maxLength: 10),
                        ContactPName = c.String(maxLength: 10),
                        Capacity = c.String(maxLength: 300),
                        CPIdNumber = c.String(maxLength: 300),
                        CPCellNo = c.String(maxLength: 15),
                        CPHomeNo = c.String(maxLength: 15),
                        CPWorkNo = c.String(maxLength: 15),
                        CPEmail1 = c.String(maxLength: 50),
                        CPEmail2 = c.String(maxLength: 50),
                        PurchaserTypeKey = c.String(maxLength: 300),
                        BType = c.String(maxLength: 50),
                        BName = c.String(maxLength: 50),
                        BAddress = c.String(maxLength: 50),
                        BStreet = c.String(maxLength: 50),
                        BSuburb = c.String(maxLength: 50),
                        BUsage = c.String(maxLength: 10),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.AuditId)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            CreateTable(
                "dbo.PropertyLeaseApplications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApplicantType = c.Int(nullable: false),
                        Gender = c.String(maxLength: 10),
                        Title = c.String(maxLength: 10),
                        MaritalStatus = c.String(maxLength: 20),
                        Initial = c.String(maxLength: 10),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        IDNo = c.String(maxLength: 25),
                        DOB = c.DateTime(storeType: "date"),
                        Nationality = c.String(maxLength: 50),
                        CellNo = c.String(maxLength: 15),
                        HomeNo = c.String(maxLength: 15),
                        WorkNo = c.String(maxLength: 15),
                        PurEmail = c.String(maxLength: 50),
                        ResAddress = c.String(maxLength: 50),
                        ResSuburb = c.String(maxLength: 50),
                        ResPostal = c.String(maxLength: 10),
                        IncomeSourcee = c.String(maxLength: 20),
                        SassaNumber = c.String(maxLength: 50),
                        GrossIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NetIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalCombinedIncome = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HouseRequired = c.String(maxLength: 50),
                        PrefArea = c.String(maxLength: 20),
                        CompanyName = c.String(maxLength: 100),
                        NameTradingAs = c.String(maxLength: 100),
                        TrustRegNo = c.String(maxLength: 100),
                        TrustName = c.String(maxLength: 100),
                        CompanyType = c.String(maxLength: 100),
                        CIPCRegistrationNo = c.String(maxLength: 100),
                        CoAddress = c.String(maxLength: 50),
                        CoSuburb = c.String(maxLength: 50),
                        CoPostal = c.String(maxLength: 10),
                        DirectorName = c.String(maxLength: 300),
                        DirectorSName = c.String(maxLength: 300),
                        DirectorIdNo = c.String(maxLength: 300),
                        ContactPSurname = c.String(maxLength: 10),
                        ContactPName = c.String(maxLength: 10),
                        Capacity = c.String(maxLength: 300),
                        CPIdNumber = c.String(maxLength: 300),
                        CPCellNo = c.String(maxLength: 15),
                        CPHomeNo = c.String(maxLength: 15),
                        CPWorkNo = c.String(maxLength: 15),
                        CPEmail1 = c.String(maxLength: 50),
                        CPEmail2 = c.String(maxLength: 50),
                        PurchaserTypeKey = c.String(maxLength: 300),
                        BType = c.String(maxLength: 50),
                        BName = c.String(maxLength: 50),
                        BAddress = c.String(maxLength: 50),
                        BStreet = c.String(maxLength: 50),
                        BSuburb = c.String(maxLength: 50),
                        BUsage = c.String(maxLength: 10),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(),
                        CreatedBySystemUserId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedBySystemUserId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SystemUsers", t => t.CreatedBySystemUserId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            AlterColumn("dbo.TransferInformations", "TransferType", c => c.Int());
            AlterColumn("dbo.TransferInformations", "Category", c => c.Int());
            AlterColumn("dbo.WaterMeterInformations", "WaterMeterReadingDateTaken", c => c.String());
            CreateIndex("dbo.TransferInformations", "TransferType");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyLeaseApplications", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseApplications", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseApplicationAudits", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.PropertyLeaseApplicationAudits", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.PropertyLeaseApplications", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseApplications", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseApplicationAudits", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.PropertyLeaseApplicationAudits", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.TransferInformations", new[] { "TransferType" });
            AlterColumn("dbo.WaterMeterInformations", "WaterMeterReadingDateTaken", c => c.DateTime());
            AlterColumn("dbo.TransferInformations", "Category", c => c.Int(nullable: false));
            AlterColumn("dbo.TransferInformations", "TransferType", c => c.Int(nullable: false));
            DropTable("dbo.PropertyLeaseApplications");
            DropTable("dbo.PropertyLeaseApplicationAudits");
            CreateIndex("dbo.TransferInformations", "TransferType");
        }
    }
}
