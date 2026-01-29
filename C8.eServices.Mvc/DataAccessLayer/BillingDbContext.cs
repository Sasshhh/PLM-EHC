namespace C8.eServices.Mvc.DataAccessLayer
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using C8.eServices.Mvc.Models;

    public partial class BillingDbContext : DbContext
    {
        public BillingDbContext()
            : base("name=BillingDbContext")
        {
        }

        public DbSet<BillInfo> BillInfos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Mapping to value assist table for property information.
            modelBuilder.Entity<BillInfo>().ToTable("TBL_CUST_BILL_HEADER_DETAILS_NEW");
            modelBuilder.Entity<BillInfo>().Property(o => o.Id).HasColumnName("LINE_COUNTER");
            modelBuilder.Entity<BillInfo>().Property(o => o.AccountNumber).HasColumnName("ACCOUNT_NO");
            modelBuilder.Entity<BillInfo>().Property(o => o.DueDate).HasColumnName("DUE_DATE");
            modelBuilder.Entity<BillInfo>().Property(o => o.RunDate).HasColumnName("RUNDATE");
            modelBuilder.Entity<BillInfo>().Property(o => o.Total).HasColumnName("PAYABLE");
            modelBuilder.Entity<BillInfo>().Property(o => o.InvDate).HasColumnName("INV_DATE");
            modelBuilder.Entity<BillInfo>().Property(o => o.CycleNo).HasColumnName("Cycle");
            modelBuilder.Entity<BillInfo>().HasKey(o => o.Id);

        }
    }
}
