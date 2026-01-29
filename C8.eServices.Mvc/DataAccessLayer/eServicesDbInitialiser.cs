using System;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class eServicesDbInitialiser : System.Data.Entity.DropCreateDatabaseIfModelChanges<eServicesDbContext>
    {
        protected override void Seed(eServicesDbContext context)
        {
            base.Seed(context);

            try
            {
                // JK.20140726a - Have to pass the same instance of the context to the identity manager, or it will crash (duplicate instance).
                var idManager = new IdentityManager(context);

                // JK.20140916a - Standard user roles for the system.
                if (!idManager.RoleExists("Super Administrators"))
                    idManager.CreateRole("Super Administrators");

                if (!idManager.RoleExists("Administrators"))
                    idManager.CreateRole("Administrators");

                if (!idManager.RoleExists("Clerks"))
                    idManager.CreateRole("Clerks");

                //if (!idManager.RoleExists("Agents"))
                //    idManager.CreateRole("Agents");

                if (!idManager.RoleExists("Customers"))
                    idManager.CreateRole("Customers");

                if (!idManager.RoleExists("Guests"))
                    idManager.CreateRole("Guests");

                var admin = new SystemIdentityUser()
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

                idManager.CreateUser(admin, "password");
                idManager.AddUserToRole(admin.Id, "Super Administrators");

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

                idManager.CreateUser(user01, "password");
                idManager.AddUserToRole(user01.Id, "Customers");

                context.SaveChanges();
            }
            catch (Exception x)
            {

                throw x;
            }
        }
    }
}
