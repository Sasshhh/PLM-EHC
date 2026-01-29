using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<C8.eServices.Mvc.DataAccessLayer.eServicesDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "C8.eServices.Mvc.DataAccessLayer.eServicesDbContext";
        }

        protected override void Seed( C8.eServices.Mvc.DataAccessLayer.eServicesDbContext context )
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            // JK.20140726a - Have to pass the same instance of the context to the identity manager, or it will crash (duplicate instance).
            var idManager = new IdentityManager( context );

            // JK.20140916a - Standard user roles for the system.
            if ( !idManager.RoleExists( "Super Administrators" ) )
                idManager.CreateRole( "Super Administrators" );

            if ( !idManager.RoleExists( "Administrators" ) )
                idManager.CreateRole( "Administrators" );

            if ( !idManager.RoleExists( "Agents" ) )
                idManager.CreateRole( "Agents" );

            if ( !idManager.RoleExists( "Customers" ) )
                idManager.CreateRole( "Customers" );

            if ( !idManager.RoleExists( "Guests" ) )
                idManager.CreateRole( "Guests" );

            if (!idManager.RoleExists("Financial Clerk"))
                idManager.CreateRole("Financial Clerk");

            var superAdmin = new SystemIdentityUser()
            {
                UserName = "SuperAdmin",
                SystemUser = new SystemUser()
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = "SuperAdmin",
                    EmailAddress = "jayan.kistasami@durban.gov.za"
                }
            };

            if ( !idManager.UserExists( superAdmin.UserName ) )
            {
                idManager.CreateUser( superAdmin, "password" );
                idManager.AddUserToRole( superAdmin.Id, "Super Administrators" );
            }

            var user01 = new SystemIdentityUser()
            {
                UserName = "JohnD",
                SystemUser = new SystemUser()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "JohnD",
                    EmailAddress = "jaykay4@gmail.com"
                }
            };

            if ( !idManager.UserExists( user01.UserName ) )
            {
                idManager.CreateUser( user01, "password" );
                idManager.AddUserToRole( user01.Id, "Customers" );
            }

            context.SaveChanges();

            //context.CustomerTypes.AddOrUpdate(o => o.Key,
            //    new CustomerType { Name = "Individual", Key = "ct_individual", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Company", Key = "ct_company", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Organisation", Key = "ct_organisation", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Government", Key = "ct_government", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Managing Agent", Key = "ct_managing_agent", IsActive = true, IsDeleted = false });

            //context.SaveChanges();
        }
    }
}
