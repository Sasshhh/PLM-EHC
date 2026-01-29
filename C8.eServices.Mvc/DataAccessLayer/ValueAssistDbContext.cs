using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using Antlr.Runtime;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class ValueAssistDbContext : DbContext
    {
        public ValueAssistDbContext()
            : base("ValueAssistDbContext")
        { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<AdditionalPropertyOwner> AdditionalPropertyOwners { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PropertyAccount> PropertyAccounts { get; set; }
        public DbSet<RatesAccount> RatesAccounts { get; set; }
        public DbSet<PostalCode> PostalCodes { get; set; }
        public DbSet<RmsPropertyAccount> RmsPropertyAccounts { get; set; }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Mapping to value assist table for property information.
            modelBuilder.Entity<Property>().ToTable("VW_ETH_VA4");
            modelBuilder.Entity<Property>().Property(o => o.RatesNumber).HasColumnName("RateNumber");
            modelBuilder.Entity<Property>().Property(o => o.PhysicalAddress1).HasColumnName("DoorNumber");
            modelBuilder.Entity<Property>().Property(o => o.PhysicalAddress2).HasColumnName("SchemeName");
            modelBuilder.Entity<Property>().Property(o => o.PhysicalAddress3).HasColumnName("StreetNumber");
            modelBuilder.Entity<Property>().Property(o => o.PhysicalAddress4).HasColumnName("StreetName");
            modelBuilder.Entity<Property>().Property(o => o.PhysicalAddress5).HasColumnName("Suburb");
            modelBuilder.Entity<Property>().Property(o => o.MarketValue).HasColumnName("MarketValue");
            modelBuilder.Entity<Property>().Property(o => o.RatingCategoryCode).HasColumnName("RatingCategoryCode");
            modelBuilder.Entity<Property>().Property(o => o.SectionType).HasColumnName("SectionType");

            // JK.2016-02-02a - Value Assist database does not have a postal code.
            //modelBuilder.Entity<Property>().Property( o => o.PhysicalAddressCode ).HasColumnName( "" );
            modelBuilder.Entity<Property>().Property(o => o.PropertyDescription).HasColumnName("PropertyDescription");

            modelBuilder.Entity<Property>().HasKey(o => o.RatesNumber);

            modelBuilder.Entity<Property>().Ignore(o => o.Id);
            modelBuilder.Entity<Property>().Ignore(o => o.PhysicalAddressCode);
            modelBuilder.Entity<Property>().Ignore(o => o.StatusId);
            modelBuilder.Entity<Property>().Ignore(o => o.IsActive);
            modelBuilder.Entity<Property>().Ignore(o => o.IsDeleted);
            modelBuilder.Entity<Property>().Ignore(o => o.IsLocked);
            modelBuilder.Entity<Property>().Ignore(o => o.CreatedBySystemUserId);
            modelBuilder.Entity<Property>().Ignore(o => o.CreatedDateTime);
            modelBuilder.Entity<Property>().Ignore(o => o.ModifiedBySystemUserId);
            modelBuilder.Entity<Property>().Ignore(o => o.ModifiedDateTime);
            modelBuilder.Entity<Property>().Ignore(o => o.CreatedBySystemUser);
            modelBuilder.Entity<Property>().Ignore(o => o.ModifiedBySystemUser);
            modelBuilder.Entity<Property>().Ignore(o => o.Status);

            // Map to value assist table for property owners.
            modelBuilder.Entity<AdditionalPropertyOwner>().ToTable("VW_ETH_VA4_Owner");
            modelBuilder.Entity<AdditionalPropertyOwner>().Property(o => o.RatesNumber).HasColumnName("RateNumber");
            modelBuilder.Entity<AdditionalPropertyOwner>().Property(o => o.OwnerName).HasColumnName("OwnerName");
            modelBuilder.Entity<AdditionalPropertyOwner>().Property(o => o.OwnerIdentificationNumber).HasColumnName("OwnerIDNumber");

            modelBuilder.Entity<AdditionalPropertyOwner>().HasKey(o => o.RatesNumber);

            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.Id);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.NoLongerRegisteredOwnerReason);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.PropertyId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.Property);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.ExecutorContactNumber);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.ExecutorName);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IsDeceased);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IsRegisteredOwner);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.StatusId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IsActive);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IsDeleted);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IsLocked);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.CreatedBySystemUserId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.CreatedDateTime);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.ModifiedBySystemUserId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.ModifiedDateTime);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.CreatedBySystemUser);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.ModifiedBySystemUser);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.Status);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.CountryOfIssueTypeId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IdentificationTypeId);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.Country);
            modelBuilder.Entity<AdditionalPropertyOwner>().Ignore(o => o.IdentificationType);

            //Map to value assist table for owner accounts.
            modelBuilder.Entity<Account>().ToTable("E_BILLING_CUSTOMER_ACCOUNT_VW");
            modelBuilder.Entity<Account>().Property(oa => oa.AccountNumber).HasColumnName("CUST_ACC_NUMBER");
            modelBuilder.Entity<Account>().Property(oa => oa.ReferenceNumber).HasColumnName("Acct_Holder_id");
            modelBuilder.Entity<Account>().Property(oa => oa.AccountHolder).HasColumnName("CUST_ACC_NAME");
            modelBuilder.Entity<Account>().Property(oa => oa.IsRMSAccount).HasColumnName("RMS_ACCOUNT");

            modelBuilder.Entity<Account>().HasKey(oa => oa.AccountNumber);

            modelBuilder.Entity<Account>().Ignore(oa => oa.Id);
            modelBuilder.Entity<Account>().Ignore(oa => oa.PropertyId);
            modelBuilder.Entity<Account>().Ignore(oa => oa.Property);
            modelBuilder.Entity<Account>().Ignore(oa => oa.RateNumber);
            modelBuilder.Entity<Account>().Ignore(oa => oa.StatusId);
            modelBuilder.Entity<Account>().Ignore(oa => oa.Status);
            modelBuilder.Entity<Account>().Ignore(o => o.IsActive);
            modelBuilder.Entity<Account>().Ignore(o => o.IsDeleted);
            modelBuilder.Entity<Account>().Ignore(o => o.IsLocked);
            modelBuilder.Entity<Account>().Ignore(o => o.CreatedBySystemUserId);
            modelBuilder.Entity<Account>().Ignore(o => o.CreatedDateTime);
            modelBuilder.Entity<Account>().Ignore(o => o.ModifiedBySystemUserId);
            modelBuilder.Entity<Account>().Ignore(o => o.ModifiedDateTime);
            modelBuilder.Entity<Account>().Ignore(o => o.CreatedBySystemUser);
            modelBuilder.Entity<Account>().Ignore(o => o.ModifiedBySystemUser);

            //Map to value assist table for property accounts.
            modelBuilder.Entity<PropertyAccount>().Map(m =>
            {
                m.Properties(p => new
                {
                    p.BilledFirstName,
                    p.BilledLastName,
                    p.BilledIdentificationNumber
                });
                m.Property(p => p.BilledFirstName).HasColumnName("FIRST_NAME");
                m.Property(p => p.BilledLastName).HasColumnName("SURNAME");
                m.Property(p => p.BilledIdentificationNumber).HasColumnName("IDENTITY_NO");
                m.ToTable("E_BILLING_CUSTOMER_VW");
            }).HasKey(pa => pa.BilledIdentificationNumber)
                .Ignore(pa => pa.Id)
                .Ignore(pa => pa.AccountNumber)
                .Ignore(pa => pa.PropertyId)
                .Ignore(pa => pa.Property)
                .Ignore(pa => pa.AccountTypeId)
                .Ignore(pa => pa.AccountType)
                .Ignore(pa => pa.StatusId)
                .Ignore(pa => pa.Status)
                .Ignore(pa => pa.IsActive)
                .Ignore(pa => pa.IsDeleted)
                .Ignore(pa => pa.IsLocked)
                .Ignore(pa => pa.CreatedDateTime)
                .Ignore(pa => pa.CreatedBySystemUserId)
                .Ignore(pa => pa.CreatedBySystemUser)
                .Ignore(pa => pa.ModifiedBySystemUserId)
                .Ignore(pa => pa.ModifiedBySystemUser)
                .Ignore(pa => pa.ModifiedDateTime);

            //Map to value assist table for RMS Property Accounts
            modelBuilder.Entity<RmsPropertyAccount>().ToTable("RMS_Prods_Servs_Per_Account");
            modelBuilder.Entity<RmsPropertyAccount>().Property(oa => oa.AccountNumber).HasColumnName("CUSTOMER_ACCOUNT_NUMBER");
            modelBuilder.Entity<RmsPropertyAccount>().Property(oa => oa.AccountName).HasColumnName("CUSTOMER_ACCOUNT_NAME");
            modelBuilder.Entity<RmsPropertyAccount>().Property(oa => oa.AccountStatus).HasColumnName("CUSTOMER_ACCOUNT_STATUS");
            modelBuilder.Entity<RmsPropertyAccount>().Property(oa => oa.ServiceCode).HasColumnName("SERVICE_CODE");
            modelBuilder.Entity<RmsPropertyAccount>().HasKey(oa => oa.AccountNumber);

            //Map to value assist table for Rates Accounts
            modelBuilder.Entity<RatesAccount>().ToTable("RMS_PROPERTY_AND_UNITS_REF");
            modelBuilder.Entity<RatesAccount>().Property(oa => oa.AccountNumber).HasColumnName("CUST_ACCT_NO");
            modelBuilder.Entity<RatesAccount>().Property(oa => oa.ServiceUnitCode).HasColumnName("SERVICE_UNIT_CODE");
            modelBuilder.Entity<RatesAccount>().Property(oa => oa.RatesNumber).HasColumnName("PRTY_RATE_NO");
            modelBuilder.Entity<RatesAccount>().Property(oa => oa.CityIndicator).HasColumnName("CITY_INDICATOR");

            modelBuilder.Entity<RatesAccount>().HasKey(oa =>  oa.RatesNumber );
            modelBuilder.Entity<RatesAccount>().HasKey(oa => oa.ServiceUnitCode);

            modelBuilder.Entity<RatesAccount>().Ignore(ra => ra.OwnerIdentificationNumber);
            modelBuilder.Entity<RatesAccount>().Ignore(ra => ra.OwnerFirstName);
            modelBuilder.Entity<RatesAccount>().Ignore(ra => ra.OwnerLastName);

            #region Map to more than 1 table
            //modelBuilder.Entity<RatesAccount>().HasRequired(p => p.Account)
            //    .WithRequiredPrincipal();
            //.HasForeignKey(p => p.AccountNumber);
            //modelBuilder.Entity<RatesAccount>()
            //    .Map(m =>
            //    {
            //        m.Properties(p => new {p.AccountNumber, p.ServiceUnitCode, p.RatesNumber});
            //        m.Property(p => p.AccountNumber).HasColumnName("CUST_ACCT_NO");
            //        m.Property(p => p.ServiceUnitCode).HasColumnName("SERVICE_UNIT_CODE");
            //        m.Property(p => p.RatesNumber).HasColumnName("PRTY_RATE_NO");
            //        m.ToTable("RMS_PROPERTY_AND_UNITS_REF");
            //    }).HasKey(ra => ra.AccountNumber)
            //    .Map(m =>
            //    {
            //        m.Properties(p => new { p.AccountNumber });
            //        m.Property(p => p.AccountNumber).HasColumnName("CUST_ACC_NUMBER");
            //        m.Property(p => p.OwnerFirstName).HasColumnName("CUST_ACC_NAME");
            //        m.Property(p => p.OwnerIdentificationNumber).HasColumnName("Acct_Holder_id");
            //        m.ToTable("E_BILLING_CUSTOMER_ACCOUNT_VW");
            //    }).HasKey(ra => ra.AccountNumber)
            //    .Ignore(ra => ra.OwnerLastName)
            //    .Ignore(ra => ra.CustomerAccountStatus)
            //    .HasRequired(p => p.Account)
            //    .WithRequiredPrincipal();
            #endregion

            //Map to postal codes table.
            modelBuilder.Entity<PostalCode>().ToTable("Postal_Codes_VW");
            modelBuilder.Entity<PostalCode>().Property(pc => pc.Area).HasColumnName("Area");
            modelBuilder.Entity<PostalCode>().Property(pc => pc.PCode).HasColumnName("\"Postal Code\"");
            modelBuilder.Entity<PostalCode>().Property(pc => pc.StreetBox).HasColumnName("\"Street Box\"");
            modelBuilder.Entity<PostalCode>().Property(pc => pc.Town).HasColumnName("Town");

            modelBuilder.Entity<PostalCode>().HasKey(pc => pc.PCode);

            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.Id);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.IsActive);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.IsDeleted);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.IsLocked);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.CreatedBySystemUserId);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.CreatedDateTime);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.ModifiedBySystemUserId);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.ModifiedDateTime);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.CreatedBySystemUser);
            modelBuilder.Entity<PostalCode>().Ignore(pc => pc.ModifiedBySystemUser);

            modelBuilder.Entity<SystemUser>()
                .HasOptional(f => f.CreatedBySystemUser)
                .WithMany()
                .HasForeignKey(f => f.CreatedBySystemUserId);

            modelBuilder.Entity<SystemUser>()
                .HasOptional(f => f.ModifiedBySystemUser)
                .WithMany()
                .HasForeignKey(f => f.ModifiedBySystemUserId);



        }
    }
}