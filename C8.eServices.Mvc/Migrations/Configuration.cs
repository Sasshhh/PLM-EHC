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
            AutomaticMigrationsEnabled = false; // DISABLED: Use explicit migrations only
            AutomaticMigrationDataLossAllowed = false;
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

            // Seed Training Slides (24 slides)
            SeedTrainingSlides(context);

            // Seed Examination Questions (1 example + 15 questions)
            SeedExaminationQuestions(context);

            // Seed Training Status Keys
            SeedTrainingStatusKeys(context);

            context.SaveChanges();

            //context.CustomerTypes.AddOrUpdate(o => o.Key,
            //    new CustomerType { Name = "Individual", Key = "ct_individual", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Company", Key = "ct_company", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Organisation", Key = "ct_organisation", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Government", Key = "ct_government", IsActive = true, IsDeleted = false },
            //    new CustomerType { Name = "Managing Agent", Key = "ct_managing_agent", IsActive = true, IsDeleted = false });

            //context.SaveChanges();
        }

        private void SeedTrainingSlides(eServicesDbContext context)
        {
            context.TrainingSlides.AddOrUpdate(s => s.SlideNumber,
                new TrainingSlide { SlideNumber = 1, Title = "PRE-TENANCY TRAINING PROGRAMME", Content = "Welcome to the Pre-Tenancy Training Programme - 5 JULY 2025", ImagePath = "/Content/Training Slides/slide-01.jpg", DisplayOrder = 1, IsActive = true },
                new TrainingSlide { SlideNumber = 2, Title = "THE SOCIAL HOUSING PROGRAMME", Content = "Overview of the Social Housing Programme", ImagePath = "/Content/Training Slides/slide-02.jpg", DisplayOrder = 2, IsActive = true },
                new TrainingSlide { SlideNumber = 3, Title = "WHAT IS SOCIAL HOUSING?", Content = "Urban rental housing programme with focus on integration, inclusivity and restructuring of urban fabric. Densified rental housing (3-4 storey walk ups or high-rise buildings). Rental in perpetuity - NO OWNERSHIP. EHC owns the units.", ImagePath = "/Content/Training Slides/slide-03.jpg", DisplayOrder = 3, IsActive = true },
                new TrainingSlide { SlideNumber = 4, Title = "THE SOCIAL HOUSING MODEL", Content = "Spatial integration, Social housing grant, Financial model, Products and services", ImagePath = "/Content/Training Slides/slide-04.jpg", DisplayOrder = 4, IsActive = true },
                new TrainingSlide { SlideNumber = 5, Title = "SOCIAL HOUSING QUALIFYING CRITERIA", Content = "Income: R1,850 to R22,000 per month. Citizenship: South African. Property ownership: Must not own or have owned property. Household types: Single with financial dependents, nuclear families. Affordability: Must have sufficient net disposable income.", ImagePath = "/Content/Training Slides/slide-05.jpg", DisplayOrder = 5, IsActive = true },
                new TrainingSlide { SlideNumber = 6, Title = "WHAT IS A SOCIAL HOUSING ASSOCIATION", Content = "EHC is a Social Housing institution providing rental accommodation to households earning R1,850-R22,000. Regulated by Social Housing Regulatory Authority. Owns and manages SH stock and related tenancies.", ImagePath = "/Content/Training Slides/slide-06.jpg", DisplayOrder = 6, IsActive = true },
                new TrainingSlide { SlideNumber = 7, Title = "ROLES AND RESPONSIBILITIES", Content = "Overview of EHC and tenant roles: Social housing, Social rights & responsibilities, Financial rights & responsibilities, Repairs & maintenance, Tenant participation", ImagePath = "/Content/Training Slides/slide-07.jpg", DisplayOrder = 7, IsActive = true },
                new TrainingSlide { SlideNumber = 8, Title = "SOCIAL RIGHTS AND RESPONSIBILITIES", Content = "Understanding nuisance, responsibilities, and problem-solving", ImagePath = "/Content/Training Slides/slide-08.jpg", DisplayOrder = 8, IsActive = true },
                new TrainingSlide { SlideNumber = 9, Title = "USE OF THE UNIT AND SUBLETTING", Content = "Units designed for certain number of people. Must personally occupy units. Residential purposes only - no business activities. Exceptional subletting circumstances only with EHC approval.", ImagePath = "/Content/Training Slides/slide-09.jpg", DisplayOrder = 9, IsActive = true },
                new TrainingSlide { SlideNumber = 10, Title = "FINANCIAL RIGHTS AND RESPONSIBILITIES", Content = "EHC and tenant financial responsibilities overview", ImagePath = "/Content/Training Slides/slide-10.jpg", DisplayOrder = 10, IsActive = true },
                new TrainingSlide { SlideNumber = 11, Title = "FINANCIAL RIGHTS AND RESPONSIBILITIES - EHC", Content = "Setting rentals and deposits. Sending monthly rent accounts. Collecting rent. Lawful eviction for non-payment.", ImagePath = "/Content/Training Slides/slide-11.jpg", DisplayOrder = 11, IsActive = true },
                new TrainingSlide { SlideNumber = 12, Title = "FINANCIAL RIGHTS AND RESPONSIBILITIES - TENANT", Content = "Pay deposit before signing lease. Sign preferred debit order. Pay rental by 1st of each month. Make arrangements for late payment. Cannot withhold rental. Lodge complaints to EHC or rental tribunal.", ImagePath = "/Content/Training Slides/slide-12.jpg", DisplayOrder = 12, IsActive = true },
                new TrainingSlide { SlideNumber = 13, Title = "RENTAL BREAKDOWN - EXAMPLE", Content = "Direct property costs: Rates, taxes, insurance, cleaning, gardening, maintenance. Overheads: Office, staff. Facilities: Security, common areas, refuse removal.", ImagePath = "/Content/Training Slides/slide-13.jpg", DisplayOrder = 13, IsActive = true },
                new TrainingSlide { SlideNumber = 14, Title = "RENTAL PAYMENTS", Content = "Rentals due on 1st of month. Payment by preferred debit order. Non-payment leads to eviction. No winner in eviction.", ImagePath = "/Content/Training Slides/slide-14.jpg", DisplayOrder = 14, IsActive = true },
                new TrainingSlide { SlideNumber = 15, Title = "RENT INCREASE - EXAMPLE", Content = "Increases due 1st of July. Linked to inflation rate. Example calculation provided.", ImagePath = "/Content/Training Slides/slide-15.jpg", DisplayOrder = 15, IsActive = true },
                new TrainingSlide { SlideNumber = 16, Title = "MAINTENANCE RIGHTS AND RESPONSIBILITIES", Content = "Maintenance overview", ImagePath = "/Content/Training Slides/slide-16.jpg", DisplayOrder = 16, IsActive = true },
                new TrainingSlide { SlideNumber = 17, Title = "MAINTENANCE, ALTERATIONS, REPAIRS", Content = "Shared responsibility: EHC (outside) and tenants (inside). Always contact Complex supervisor first. Leave home as found when moved in.", ImagePath = "/Content/Training Slides/slide-17.jpg", DisplayOrder = 17, IsActive = true },
                new TrainingSlide { SlideNumber = 18, Title = "INSURANCE", Content = "EHC insures property including units. Tenants insure household contents. Example: geyser bursts and damages lounge suite.", ImagePath = "/Content/Training Slides/slide-18.jpg", DisplayOrder = 18, IsActive = true },
                new TrainingSlide { SlideNumber = 19, Title = "TENANT PARTICIPATION & COMMUNITY DEVELOPMENT", Content = "Overview of tenant participation", ImagePath = "/Content/Training Slides/slide-19.jpg", DisplayOrder = 19, IsActive = true },
                new TrainingSlide { SlideNumber = 20, Title = "TENANT PARTICIPATION/COMMUNITY DEVELOPMENT", Content = "What is participation? Why should tenants participate? How can tenants participate?", ImagePath = "/Content/Training Slides/slide-20.jpg", DisplayOrder = 20, IsActive = true },
                new TrainingSlide { SlideNumber = 21, Title = "SUMMARY SLIDE 1", Content = "Key points summary", ImagePath = "/Content/Training Slides/slide-21.jpg", DisplayOrder = 21, IsActive = true },
                new TrainingSlide { SlideNumber = 22, Title = "SUMMARY SLIDE 2", Content = "Additional key points", ImagePath = "/Content/Training Slides/slide-22.jpg", DisplayOrder = 22, IsActive = true },
                new TrainingSlide { SlideNumber = 23, Title = "SUMMARY SLIDE 3", Content = "Final summary points", ImagePath = "/Content/Training Slides/slide-23.jpg", DisplayOrder = 23, IsActive = true },
                new TrainingSlide { SlideNumber = 24, Title = "CONCLUSION", Content = "Thank you for completing the training. Please proceed to the examination.", ImagePath = "/Content/Training Slides/slide-24.jpg", DisplayOrder = 24, IsActive = true }
            );
        }

        private void SeedExaminationQuestions(eServicesDbContext context)
        {
            context.ExaminationQuestions.AddOrUpdate(q => q.QuestionOrder,
                // EXAMPLE QUESTION
                new ExaminationQuestion { QuestionText = "The sun always rises in:", OptionA = "The afternoon", OptionB = "The morning", OptionC = "The evening", OptionD = null, CorrectAnswer = "B", QuestionOrder = 0, IsExampleQuestion = true, IsActive = true },
                
                // ACTUAL QUESTIONS (15)
                new ExaminationQuestion { QuestionText = "Who owns your unit?", OptionA = "The tenant", OptionB = "EHC", OptionC = "The tenant and EHC jointly", OptionD = null, CorrectAnswer = "B", QuestionOrder = 1, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The social housing programme includes the following tenure options:", OptionA = "Rental forever", OptionB = "Rent to buy", OptionC = "Rent to own", OptionD = "Instalment sale", CorrectAnswer = "A", QuestionOrder = 2, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "If noisy neighbours are disturbing you, it is best to:", OptionA = "Call the police first", OptionB = "Call Complex Supervisor or a staff member at EHC first", OptionC = "Discuss the situation with your neighbour", OptionD = null, CorrectAnswer = "B", QuestionOrder = 3, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "You can be evicted because of:", OptionA = "Rental arrears", OptionB = "Nuisance", OptionC = "Both of the above", OptionD = null, CorrectAnswer = "C", QuestionOrder = 4, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The deposit of 1 month's rental must be paid:", OptionA = "Before you sign the lease agreement", OptionB = "After the 1st month of rental", OptionC = "EHC does not charge a deposit", OptionD = null, CorrectAnswer = "A", QuestionOrder = 5, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "I pay rental to cover:", OptionA = "Insurance of the property, security", OptionB = "The maintenance of the buildings", OptionC = "Lighting, cleaning, gardening of communal areas", OptionD = "All of the above", CorrectAnswer = "D", QuestionOrder = 6, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The wind blows the roof off and your bed is damaged as a result, who is responsible for replacing your bed?", OptionA = "EHC", OptionB = "The tenant", OptionC = "Both EHC and the tenant", OptionD = "None of the above", CorrectAnswer = "B", QuestionOrder = 7, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "When your unit needs a light bulb to be replaced:", OptionA = "EHC is responsible for the outside of your home and you are responsible for the inside", OptionB = "EHC is responsible for both the inside and outside of your home", OptionC = "You are responsible for both the inside and the outside of your home", OptionD = null, CorrectAnswer = "A", QuestionOrder = 8, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The rental will:", OptionA = "Increase every year on the 1st of October", OptionB = "Increase every year on the 1st of July", OptionC = "Always stay the same", OptionD = null, CorrectAnswer = "B", QuestionOrder = 9, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The rent must be paid:", OptionA = "Before the 15th of each month", OptionB = "Before the 1st of each month", OptionC = "When it suits you", OptionD = null, CorrectAnswer = "B", QuestionOrder = 10, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "The preferred way in which rent must be paid is:", OptionA = "In cash at the EHC reception", OptionB = "Via direct payroll deduction", OptionC = "By preferred debit order", OptionD = null, CorrectAnswer = "C", QuestionOrder = 11, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "If you want to end the lease agreement:", OptionA = "You give EHC 2 months' notice", OptionB = "You give EHC 1 month's notice", OptionC = "You can leave whenever you like", OptionD = null, CorrectAnswer = "B", QuestionOrder = 12, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "When you leave your home and you have met all your obligations:", OptionA = "You will lose your deposit", OptionB = "Your deposit will be refunded", OptionC = "Your deposit will be used for maintenance", OptionD = null, CorrectAnswer = "B", QuestionOrder = 13, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "If you want to do alterations to your home:", OptionA = "You ask a friend to do it for you", OptionB = "You ask a contractor to do it for you", OptionC = "You ask EHC for permission in writing first", OptionD = null, CorrectAnswer = "C", QuestionOrder = 14, IsExampleQuestion = false, IsActive = true },
                new ExaminationQuestion { QuestionText = "You cannot afford the rent anymore and you decide to take in a person who will pay you rent:", OptionA = "You are allowed to relet/sublet your unit", OptionB = "You are required to apply to EHC to sublet", OptionC = "You can sublet on condition that the joint income does not exceed R22,000", OptionD = "You can sublet if your household does not exceed 4 people", CorrectAnswer = "B", QuestionOrder = 15, IsExampleQuestion = false, IsActive = true }
            );
        }

        private void SeedTrainingStatusKeys(eServicesDbContext context)
        {
            // First, get or create a StatusType for training statuses
            var trainingStatusType = context.StatusTypes
                .FirstOrDefault(st => st.Key == "st_tenant_training");

            if (trainingStatusType == null)
            {
                trainingStatusType = new StatusType
                {
                    Key = "st_tenant_training",
                    Name = "Tenant Training",
                    Description = "Status type for tenant training and examination workflow",
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                };
                context.StatusTypes.Add(trainingStatusType);
                context.SaveChanges(); // Save to get the Id
            }

            // Now add the status records with the valid StatusTypeId
            context.Status.AddOrUpdate(s => s.Key,
                new Status 
                { 
                    Key = "s_awaiting_online_training", 
                    Name = "Awaiting Online Training", 
                    Description = "Tenant has been invited to complete online training", 
                    StatusTypeId = trainingStatusType.Id,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                },
                new Status 
                { 
                    Key = "s_training_in_progress", 
                    Name = "Training In Progress", 
                    Description = "Tenant is currently completing the training programme", 
                    StatusTypeId = trainingStatusType.Id,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                },
                new Status 
                { 
                    Key = "s_training_completed", 
                    Name = "Training Completed", 
                    Description = "Tenant has completed the training programme", 
                    StatusTypeId = trainingStatusType.Id,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                },
                new Status 
                { 
                    Key = "s_examination_passed", 
                    Name = "Examination Passed", 
                    Description = "Tenant has passed the examination with 100%", 
                    StatusTypeId = trainingStatusType.Id,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                },
                new Status 
                { 
                    Key = "s_examination_failed", 
                    Name = "Examination Failed", 
                    Description = "Tenant failed the examination", 
                    StatusTypeId = trainingStatusType.Id,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false
                }
            );
        }
    }
}
