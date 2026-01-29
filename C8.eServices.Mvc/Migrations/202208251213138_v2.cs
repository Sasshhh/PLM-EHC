namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.HumanSettlementApplicationAudits", "WalkInBySystemUserId");
            DropColumn("dbo.HumanSettlementApplications", "WalkInBySystemUserId");
            RenameColumn(table: "dbo.HumanSettlementApplicationAudits", name: "SecAppTitleTypeId", newName: "WalkInBySystemUserId");
            RenameColumn(table: "dbo.HumanSettlementApplications", name: "SecAppTitleTypeId", newName: "WalkInBySystemUserId");
            RenameIndex(table: "dbo.HumanSettlementApplicationAudits", name: "IX_SecAppTitleTypeId", newName: "IX_WalkInBySystemUserId");
            RenameIndex(table: "dbo.HumanSettlementApplications", name: "IX_SecAppTitleTypeId", newName: "IX_WalkInBySystemUserId");
            CreateTable(
                "dbo.HumanSettlementLeaseDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyLeaseApplicationId = c.Int(nullable: false),
                        PurchaserTypeId = c.Int(),
                        StartDate = c.DateTime(storeType: "date"),
                        PeriodInMonths = c.Int(),
                        EndDate = c.DateTime(storeType: "date"),
                        RenewalNotice = c.DateTime(storeType: "date"),
                        TerminationNotice = c.DateTime(storeType: "date"),
                        DepositeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RentalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VATAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalIncludingVAT = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StatementDate = c.DateTime(storeType: "date"),
                        EscalationDate = c.DateTime(storeType: "date"),
                        Email = c.Boolean(nullable: false),
                        SMS = c.Boolean(nullable: false),
                        Postal = c.Boolean(nullable: false),
                        LastName = c.String(),
                        FirstNames = c.String(),
                        IDNo = c.String(maxLength: 25),
                        LeasedAddress = c.String(maxLength: 500),
                        LeasedPostal = c.String(maxLength: 50),
                        LeasedSuburb = c.String(),
                        buildingName = c.String(),
                        SpaceUnitNo = c.String(),
                        OfficeParkName = c.String(),
                        StatusId = c.Int(),
                        SystemUserId = c.Int(),
                        IsNew = c.Boolean(nullable: false),
                        IsRenewed = c.Boolean(nullable: false),
                        DetailsUpdated = c.Boolean(),
                        PreparationFee = c.Double(),
                        CreditCheckFee = c.Double(),
                        CalculatedAsFolllows = c.Double(),
                        InitialDepositPremises = c.Double(),
                        DepositTenantContribution = c.Double(),
                        ShadePortParking = c.Double(),
                        SPP = c.Boolean(),
                        OpenParking = c.Double(),
                        OPP = c.Boolean(),
                        StoreRooms = c.Double(),
                        STR = c.Boolean(),
                        Electricity = c.Double(),
                        ELEC = c.Boolean(),
                        Refuse = c.Double(),
                        SecurityFee = c.Double(),
                        SEC = c.Boolean(),
                        Sewerage = c.Double(),
                        Water = c.Double(),
                        WTR = c.Boolean(),
                        TerminationDate = c.DateTime(),
                        CarportParkingBayNumber = c.String(),
                        OPenParkingBayNumber = c.String(),
                        ShadePortBayNumber = c.String(),
                        CarportParkingBay = c.Double(),
                        LeaseAdministrationFee = c.Double(),
                        FloorNumber = c.String(),
                        TotalMonthlyCharges = c.Double(),
                        ApplicantComment = c.String(),
                        MonthsOffered = c.Int(nullable: false),
                        Completed = c.Boolean(nullable: false),
                        DepositHeld = c.Double(),
                        NoticeDate = c.DateTime(),
                        TerminationReminder = c.DateTime(nullable: false),
                        DepartmentId = c.Int(),
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
                .ForeignKey("dbo.ApplicationEntities", t => t.DepartmentId)
                .ForeignKey("dbo.SystemUsers", t => t.ModifiedBySystemUserId)
                .ForeignKey("dbo.PropertyLeaseApplications", t => t.PropertyLeaseApplicationId)
                .ForeignKey("dbo.PurchaserTypes", t => t.PurchaserTypeId)
                .ForeignKey("dbo.Status", t => t.StatusId)
                .ForeignKey("dbo.SystemUsers", t => t.SystemUserId)
                .Index(t => t.PropertyLeaseApplicationId)
                .Index(t => t.PurchaserTypeId)
                .Index(t => t.StatusId)
                .Index(t => t.SystemUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.CreatedBySystemUserId)
                .Index(t => t.ModifiedBySystemUserId);
            
            AddColumn("dbo.PLMApplicationHistortyLogAudits", "HumanSettlementApplicationId", c => c.Int());
            AddColumn("dbo.PLMApplicationHistortyLogs", "HumanSettlementApplicationId", c => c.Int());
            CreateIndex("dbo.HumanSettlementApplicationAudits", "SecAppTitleTypeId");
            CreateIndex("dbo.HumanSettlementApplications", "SecAppTitleTypeId");
            CreateIndex("dbo.PLMApplicationHistortyLogAudits", "HumanSettlementApplicationId");
            CreateIndex("dbo.PLMApplicationHistortyLogs", "HumanSettlementApplicationId");
            AddForeignKey("dbo.PLMApplicationHistortyLogAudits", "HumanSettlementApplicationId", "dbo.HumanSettlementApplications", "Id");
            AddForeignKey("dbo.PLMApplicationHistortyLogs", "HumanSettlementApplicationId", "dbo.HumanSettlementApplications", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PLMApplicationHistortyLogs", "HumanSettlementApplicationId", "dbo.HumanSettlementApplications");
            DropForeignKey("dbo.PLMApplicationHistortyLogAudits", "HumanSettlementApplicationId", "dbo.HumanSettlementApplications");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "SystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "StatusId", "dbo.Status");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "PurchaserTypeId", "dbo.PurchaserTypes");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "PropertyLeaseApplicationId", "dbo.PropertyLeaseApplications");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "ModifiedBySystemUserId", "dbo.SystemUsers");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "DepartmentId", "dbo.ApplicationEntities");
            DropForeignKey("dbo.HumanSettlementLeaseDetails", "CreatedBySystemUserId", "dbo.SystemUsers");
            DropIndex("dbo.PLMApplicationHistortyLogs", new[] { "HumanSettlementApplicationId" });
            DropIndex("dbo.PLMApplicationHistortyLogAudits", new[] { "HumanSettlementApplicationId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "ModifiedBySystemUserId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "CreatedBySystemUserId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "DepartmentId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "SystemUserId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "StatusId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "PurchaserTypeId" });
            DropIndex("dbo.HumanSettlementLeaseDetails", new[] { "PropertyLeaseApplicationId" });
            DropIndex("dbo.HumanSettlementApplications", new[] { "SecAppTitleTypeId" });
            DropIndex("dbo.HumanSettlementApplicationAudits", new[] { "SecAppTitleTypeId" });
            DropColumn("dbo.PLMApplicationHistortyLogs", "HumanSettlementApplicationId");
            DropColumn("dbo.PLMApplicationHistortyLogAudits", "HumanSettlementApplicationId");
            DropTable("dbo.HumanSettlementLeaseDetails");
            RenameIndex(table: "dbo.HumanSettlementApplications", name: "IX_WalkInBySystemUserId", newName: "IX_SecAppTitleTypeId");
            RenameIndex(table: "dbo.HumanSettlementApplicationAudits", name: "IX_WalkInBySystemUserId", newName: "IX_SecAppTitleTypeId");
            RenameColumn(table: "dbo.HumanSettlementApplications", name: "WalkInBySystemUserId", newName: "SecAppTitleTypeId");
            RenameColumn(table: "dbo.HumanSettlementApplicationAudits", name: "WalkInBySystemUserId", newName: "SecAppTitleTypeId");
            AddColumn("dbo.HumanSettlementApplications", "WalkInBySystemUserId", c => c.Int());
            AddColumn("dbo.HumanSettlementApplicationAudits", "WalkInBySystemUserId", c => c.Int());
        }
    }
}
