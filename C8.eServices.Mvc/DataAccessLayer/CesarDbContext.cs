using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class CesarDbContext : DbContext
    {
        public CesarDbContext()
            : base("CesarDbContext")
        { }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        
        public DbSet<EmailQueueProcessed> EmailQueueProcessed { get;set;}


        public DbSet<EmailQueueItem> EmailQueue { get; set; }
        public DbSet<EmailAttachmentQueue> EmailAttachmentQueue { get; set; }

        public DbSet<EmailAccounts> EmailAccounts { get; set; }

        //SMS
        public DbSet<SmsQueueProcessed> SmsQueueProcessed { get; set; }


        public DbSet<SmsQueueItem> SmsQueue { get; set; }
       
        public DbSet<SmsAccounts> SmsAccounts { get; set; }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Mapping to value assist table for property information.
            modelBuilder.Entity<EmailQueueItem>().ToTable( "tb_EmailQueue" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.EmailQueueId ).HasColumnName( "EmailQueueId" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.ApplicationId ).HasColumnName( "ApplicationId" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.EmailAccountId ).HasColumnName( "EmailAccountId" );
            modelBuilder.Entity<EmailQueueItem>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.ToList ).HasColumnName( "ToList" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.CcList ).HasColumnName( "CcList" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.BccList ).HasColumnName( "BccList" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.Subject ).HasColumnName( "Subject" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.Body ).HasColumnName( "Body" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.IsHtml ).HasColumnName( "IsHtml" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.FailureCount ).HasColumnName( "FailureCount" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.ReferenceId ).HasColumnName( "ReferenceId" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.ReferenceTypeId ).HasColumnName( "ReferenceTypeId" );
            modelBuilder.Entity<EmailQueueItem>().Property( o => o.HasAttachments ).HasColumnName( "HasAttachments" );
            modelBuilder.Entity<EmailQueueItem>().HasKey( o => o.EmailQueueId );

            modelBuilder.Entity<EmailAttachmentQueue>().ToTable( "tb_EmailAttachmentQueue" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.EmailAttachmentQueueId ).HasColumnName( "EmailAttachmentQueueId" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.EmailQueueId ).HasColumnName( "EmailQueueId" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.ApplicationId ).HasColumnName( "ApplicationId" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.Filename ).HasColumnName( "Filename" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.ContentType ).HasColumnName( "ContentType" );
            modelBuilder.Entity<EmailAttachmentQueue>().Property( o => o.Attachment ).HasColumnName( "Attachment" );
            modelBuilder.Entity<EmailAttachmentQueue>().HasKey( o => o.EmailAttachmentQueueId );


            modelBuilder.Entity<EmailQueueProcessed>().ToTable("tb_EmailQueueProcessed");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.EmailQueueId).HasColumnName("EmailQueueId");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.EmailAccountId).HasColumnName("EmailAccountId");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.ToList).HasColumnName("ToList");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.CcList).HasColumnName("CcList");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.BccList).HasColumnName("BccList");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.Subject).HasColumnName("Subject");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.Body).HasColumnName("Body");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.IsHtml).HasColumnName("IsHtml");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.HasAttachments).HasColumnName("HasAttachments");
            modelBuilder.Entity<EmailQueueProcessed>().Property(o => o.ProcessedDateTime).HasColumnName("ProcessedDateTime");
            modelBuilder.Entity<EmailQueueProcessed>().HasKey(o => o.EmailQueueId);

            modelBuilder.Entity<EmailAccounts>().ToTable("tb_EmailAccounts");
            modelBuilder.Entity<EmailAccounts>().Property(o => o.EmailAccountId).HasColumnName("EmailAccountId");
            modelBuilder.Entity<EmailAccounts>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<EmailAccounts>().Property(o => o.MaxFailureCount).HasColumnName("MaxFailureCount");
            modelBuilder.Entity<EmailAccounts>().HasKey(o => o.EmailAccountId);

            // SMS
            modelBuilder.Entity<SmsQueueItem>().ToTable("tb_SmsQueue");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.SmsQueueId).HasColumnName("SmsQueueId");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.MobileNumber).HasColumnName("MobileNumber");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.TextMessage).HasColumnName("TextMessage");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<SmsQueueItem>().Property(o => o.StatusId).HasColumnName("StatusId");
            modelBuilder.Entity<SmsQueueItem>().HasKey(o => o.SmsQueueId);

            modelBuilder.Entity<SmsQueueProcessed>().ToTable("tb_SmsQueueProcessed");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.SmsQueueId).HasColumnName("SmsQueueId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.QueueDateTime).HasColumnName("QueueDateTime");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.MobileNumber).HasColumnName("MobileNumber");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.TextMessage).HasColumnName("TextMessage");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.FailureCount).HasColumnName("FailureCount");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.ReferenceId).HasColumnName("ReferenceId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.ReferenceTypeId).HasColumnName("ReferenceTypeId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.StatusId).HasColumnName("StatusId");
            modelBuilder.Entity<SmsQueueProcessed>().Property(o => o.ProcessedDateTime).HasColumnName("ProcessedDateTime");
            modelBuilder.Entity<SmsQueueProcessed>().HasKey(o => o.SmsQueueId);

            modelBuilder.Entity<SmsAccounts>().ToTable("tb_SmsAccounts");
            modelBuilder.Entity<SmsAccounts>().Property(o => o.SmsAccountId).HasColumnName("SmsAccountId");
            modelBuilder.Entity<SmsAccounts>().Property(o => o.ApplicationId).HasColumnName("ApplicationId");
            modelBuilder.Entity<SmsAccounts>().Property(o => o.MaxFailureCount).HasColumnName("MaxFailureCount");
            modelBuilder.Entity<SmsAccounts>().HasKey(o => o.SmsAccountId);

        }
    }
}