namespace C8.eServices.Mvc.DataAccessLayer
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using C8.eServices.Mvc.Models;

    public partial class DataWarehouse : DbContext
    {
        public DataWarehouse()
            : base( "name=DataWarehouse" )
        {
        }

        public DbSet<Payment> Payments { get; set; }
        
        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Entity<Payment>().ToTable( "RMS_BILL_INFO_PER_ACCOUNT_PER_MONTH_REF" );
            modelBuilder.Entity<Payment>().Property( b => b.FullName ).HasColumnName( "CUSTOMER_FULL_NAME" );
            modelBuilder.Entity<Payment>().Property( b => b.IdentificationNumber ).HasColumnName( "CUSTOMER_ID_NUMBER" );
            modelBuilder.Entity<Payment>().Property( b => b.AccountNumber ).HasColumnName( "CUST_ACCT_NO" );
            modelBuilder.Entity<Payment>().Property( b => b.BillDate ).HasColumnName( "BILL_DATE" );
            modelBuilder.Entity<Payment>().Property( b => b.BillYear ).HasColumnName( "BILL_YEAR" );
            modelBuilder.Entity<Payment>().Property( b => b.BillMonth ).HasColumnName( "BILL_MONTH" );
            modelBuilder.Entity<Payment>().Property( b => b.Amount ).HasColumnName( "RATES_TOTAL_PER_ACCOUNT_PER_MONTH" );
        }
    }
}
