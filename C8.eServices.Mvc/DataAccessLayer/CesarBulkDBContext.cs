using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class CesarBulkDBContext : DbContext
    {
        public CesarBulkDBContext()
            : base("CesarBulkDbContext")
        { }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }


        public DbSet<EmailQueueProcessedBulk> EmailQueueProcessedBulk { get; set; }


        public DbSet<EmailQueueItemBulk> EmailQueueBulk { get; set; }
        public DbSet<EmailAttachmentQueueBulk> EmailAttachmentQueueBulk { get; set; }

        public DbSet<EmailAccountsBulk> EmailAccountsBulk { get; set; }

        //SMS
        public DbSet<SmsQueueProcessedBulk> SmsQueueProcessedBulk { get; set; }


        public DbSet<SmsQueueItemBulk> SmsQueueBulk { get; set; }

        public DbSet<SmsAccountsBulk> SmsAccountsBulk { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Mapping to value assist table for property information.
            modelBuilder.Entity<EmailQueueItemBulk>().ToTable("tb_EmailQueue");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.EmailQueueId).HasColumnName("EmailQueueId");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.EmailAccountId).HasColumnName("EmailAccountId");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.ToList).HasColumnName("ToList");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.CcList).HasColumnName("CcList");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.BccList).HasColumnName("BccList");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.Subject).HasColumnName("Subject");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.Body).HasColumnName("Body");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.IsHtml).HasColumnName("IsHtml");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<EmailQueueItemBulk>().Property(o => o.HasAttachments).HasColumnName("HasAttachments");
            modelBuilder.Entity<EmailQueueItemBulk>().HasKey(o => o.EmailQueueId);

            modelBuilder.Entity<EmailAttachmentQueueBulk>().ToTable("tb_EmailAttachmentQueue");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.EmailAttachmentQueueId).HasColumnName("EmailAttachmentQueueId");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.EmailQueueId).HasColumnName("EmailQueueId");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.Filename).HasColumnName("Filename");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.ContentType).HasColumnName("ContentType");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().Property(o => o.Attachment).HasColumnName("Attachment");
            modelBuilder.Entity<EmailAttachmentQueueBulk>().HasKey(o => o.EmailAttachmentQueueId);


            modelBuilder.Entity<EmailQueueProcessedBulk>().ToTable("tb_EmailQueueProcessed");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.EmailQueueId).HasColumnName("EmailQueueId");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.EmailAccountId).HasColumnName("EmailAccountId");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.ToList).HasColumnName("ToList");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.CcList).HasColumnName("CcList");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.BccList).HasColumnName("BccList");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.Subject).HasColumnName("Subject");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.Body).HasColumnName("Body");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.IsHtml).HasColumnName("IsHtml");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.HasAttachments).HasColumnName("HasAttachments");
            modelBuilder.Entity<EmailQueueProcessedBulk>().Property(o => o.ProcessedDateTime).HasColumnName("ProcessedDateTime");
            modelBuilder.Entity<EmailQueueProcessedBulk>().HasKey(o => o.EmailQueueId);

            modelBuilder.Entity<EmailAccountsBulk>().ToTable("tb_EmailAccounts");
            modelBuilder.Entity<EmailAccountsBulk>().Property(o => o.EmailAccountId).HasColumnName("EmailAccountId");
            modelBuilder.Entity<EmailAccountsBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailAccountsBulk>().Property(o => o.MaxFailureCount).HasColumnName("MaxFailureCount");
            modelBuilder.Entity<EmailAccountsBulk>().HasKey(o => o.EmailAccountId);

            // SMS
            modelBuilder.Entity<SmsQueueItemBulk>().ToTable("tb_SmsQueue");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.SmsQueueId).HasColumnName("SmsQueueId");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.MobileNumber).HasColumnName("MobileNumber");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.TextMessage).HasColumnName("TextMessage");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<SmsQueueItemBulk>().Property(o => o.StatusId).HasColumnName("StatusId");
            modelBuilder.Entity<SmsQueueItemBulk>().HasKey(o => o.SmsQueueId);

            modelBuilder.Entity<SmsQueueProcessedBulk>().ToTable("tb_SmsQueueProcessed");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.SmsQueueId).HasColumnName("SmsQueueId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.MobileNumber).HasColumnName("MobileNumber");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.TextMessage).HasColumnName("TextMessage");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.StatusId).HasColumnName("StatusId");
            modelBuilder.Entity<SmsQueueProcessedBulk>().Property(o => o.ProcessedDateTime).HasColumnName("ProcessedDateTime");
            modelBuilder.Entity<SmsQueueProcessedBulk>().HasKey(o => o.SmsQueueId);

            modelBuilder.Entity<SmsAccountsBulk>().ToTable("tb_SmsAccounts");
            modelBuilder.Entity<SmsAccountsBulk>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsAccountsBulk>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsAccountsBulk>().Property(o => o.MaxFailureCount).HasColumnName("MaxFailureCount");
            modelBuilder.Entity<SmsAccountsBulk>().HasKey(o => o.SmsAccountId);

        }
    }
}