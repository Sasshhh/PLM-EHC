using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class eBilling6MonthsDbContext : DbContext
    {
        public eBilling6MonthsDbContext()
            : base( "eBilling6MonthsDbContext" )
        { }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }


        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            base.OnModelCreating(modelBuilder);

            // JK.20140902a - Include this to remove cascade deletions.
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }
    }
}