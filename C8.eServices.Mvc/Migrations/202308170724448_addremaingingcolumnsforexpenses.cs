namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addremaingingcolumnsforexpenses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MonthlyExpenseAudits", "LELESupportMaintenance", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEBankCharges", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LECellularAirtimeData", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEClothing", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LECreditCards", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEDomesticEmployees", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEDonations", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEEducationSchool", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEEntertainment", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEGroceries", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEInstalmentAccounts", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEMedicalAid", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEMemberships", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEPersonalLoans", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEPetCare", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LERetailAccounts", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LESecurity", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LESubscriptions", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LETelephones", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LETransport", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LETV", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LEOtherExpenses", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenseAudits", "LETotalExpenses", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LESupportMaintenance", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEBankCharges", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LECellularAirtimeData", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEClothing", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LECreditCards", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEDomesticEmployees", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEDonations", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEEducationSchool", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEEntertainment", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEGroceries", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEInstalmentAccounts", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEMedicalAid", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEMemberships", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEPersonalLoans", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEPetCare", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LERetailAccounts", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LESecurity", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LESubscriptions", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LETelephones", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LETransport", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LETV", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LEOtherExpenses", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.MonthlyExpenses", "LETotalExpenses", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MonthlyExpenses", "LETotalExpenses");
            DropColumn("dbo.MonthlyExpenses", "LEOtherExpenses");
            DropColumn("dbo.MonthlyExpenses", "LETV");
            DropColumn("dbo.MonthlyExpenses", "LETransport");
            DropColumn("dbo.MonthlyExpenses", "LETelephones");
            DropColumn("dbo.MonthlyExpenses", "LESubscriptions");
            DropColumn("dbo.MonthlyExpenses", "LESecurity");
            DropColumn("dbo.MonthlyExpenses", "LERetailAccounts");
            DropColumn("dbo.MonthlyExpenses", "LEPetCare");
            DropColumn("dbo.MonthlyExpenses", "LEPersonalLoans");
            DropColumn("dbo.MonthlyExpenses", "LEMemberships");
            DropColumn("dbo.MonthlyExpenses", "LEMedicalAid");
            DropColumn("dbo.MonthlyExpenses", "LEInstalmentAccounts");
            DropColumn("dbo.MonthlyExpenses", "LEGroceries");
            DropColumn("dbo.MonthlyExpenses", "LEEntertainment");
            DropColumn("dbo.MonthlyExpenses", "LEEducationSchool");
            DropColumn("dbo.MonthlyExpenses", "LEDonations");
            DropColumn("dbo.MonthlyExpenses", "LEDomesticEmployees");
            DropColumn("dbo.MonthlyExpenses", "LECreditCards");
            DropColumn("dbo.MonthlyExpenses", "LEClothing");
            DropColumn("dbo.MonthlyExpenses", "LECellularAirtimeData");
            DropColumn("dbo.MonthlyExpenses", "LEBankCharges");
            DropColumn("dbo.MonthlyExpenses", "LESupportMaintenance");
            DropColumn("dbo.MonthlyExpenseAudits", "LETotalExpenses");
            DropColumn("dbo.MonthlyExpenseAudits", "LEOtherExpenses");
            DropColumn("dbo.MonthlyExpenseAudits", "LETV");
            DropColumn("dbo.MonthlyExpenseAudits", "LETransport");
            DropColumn("dbo.MonthlyExpenseAudits", "LETelephones");
            DropColumn("dbo.MonthlyExpenseAudits", "LESubscriptions");
            DropColumn("dbo.MonthlyExpenseAudits", "LESecurity");
            DropColumn("dbo.MonthlyExpenseAudits", "LERetailAccounts");
            DropColumn("dbo.MonthlyExpenseAudits", "LEPetCare");
            DropColumn("dbo.MonthlyExpenseAudits", "LEPersonalLoans");
            DropColumn("dbo.MonthlyExpenseAudits", "LEMemberships");
            DropColumn("dbo.MonthlyExpenseAudits", "LEMedicalAid");
            DropColumn("dbo.MonthlyExpenseAudits", "LEInstalmentAccounts");
            DropColumn("dbo.MonthlyExpenseAudits", "LEGroceries");
            DropColumn("dbo.MonthlyExpenseAudits", "LEEntertainment");
            DropColumn("dbo.MonthlyExpenseAudits", "LEEducationSchool");
            DropColumn("dbo.MonthlyExpenseAudits", "LEDonations");
            DropColumn("dbo.MonthlyExpenseAudits", "LEDomesticEmployees");
            DropColumn("dbo.MonthlyExpenseAudits", "LECreditCards");
            DropColumn("dbo.MonthlyExpenseAudits", "LEClothing");
            DropColumn("dbo.MonthlyExpenseAudits", "LECellularAirtimeData");
            DropColumn("dbo.MonthlyExpenseAudits", "LEBankCharges");
            DropColumn("dbo.MonthlyExpenseAudits", "LELESupportMaintenance");
        }
    }
}
