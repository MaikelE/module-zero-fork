using System.Linq;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using EntityFramework.DynamicFilters;
using ModuleZeroSampleProject.Authorization;
using ModuleZeroSampleProject.EntityFramework;
using ModuleZeroSampleProject.MultiTenancy;
using ModuleZeroSampleProject.Questions;
using ModuleZeroSampleProject.Users;
using Abp.MultiTenancy;

namespace ModuleZeroSampleProject.Migrations.Data
{
    public class InitialDataBuilder
    {
        public void Build(GlobalDbContext context)
        {
            context.DisableAllFilters();
            CreateUserAndRoles(context);
        }

        private void CreateUserAndRoles(GlobalDbContext context)
        {
            //Admin role for tenancy owner

            var adminRoleForTenancyOwner = context.Roles.FirstOrDefault(r => r.TenantId == null && r.Name == "Admin");
            if (adminRoleForTenancyOwner == null)
            {
                adminRoleForTenancyOwner = context.Roles.Add(new Role(null, "Admin", "Admin"));
                context.SaveChanges();
            }

            //Admin user for tenancy owner

            var adminUserForTenancyOwner = context.Users.FirstOrDefault(u => u.UserInTenants.Any(ut => ut.Tenant == null) && u.UserName == "systemadmin");
            if (adminUserForTenancyOwner == null)
            {
                adminUserForTenancyOwner = context.Users.Add(
                    new User
                    {
                        
                        UserName = "systemadmin",
                        Name = "System",
                        Surname = "Administrator",
                        EmailAddress = "admin@aspnetboilerplate.com",
                        IsEmailConfirmed = true,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    });

                context.SaveChanges();

                context.UserRoles.Add(new UserRole(adminUserForTenancyOwner.Id, adminRoleForTenancyOwner.Id));

                context.SaveChanges();
            }

            //add admin user to host account
            if (context.UserTenants.FirstOrDefault(ut => ut.Tenant == null && ut.UserId == adminUserForTenancyOwner.Id) == null)
            {

                context.UserTenants.Add(new UserTenant()
                {
                    UserId = adminUserForTenancyOwner.Id
                });
                context.SaveChanges();
            }

            //Default tenant
            var defaultTenant = context.Tenants.FirstOrDefault(t => t.TenancyName == "Default");
            if (defaultTenant == null)
            {
                defaultTenant = context.Tenants.Add(new Tenant("Default", "Default"));
                context.SaveChanges();
            }

            //Admin role for 'Default' tenant
            var adminRoleForDefaultTenant = context.Roles.FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == "Admin");
            if (adminRoleForDefaultTenant == null)
            {
                adminRoleForDefaultTenant = context.Roles.Add(new Role(defaultTenant.Id, "Admin", "Admin"));
                context.SaveChanges();

                //Permission definitions for Admin of 'Default' tenant
                context.Permissions.Add(new RolePermissionSetting { RoleId = adminRoleForDefaultTenant.Id, Name = "CanDeleteAnswers", IsGranted = true });
                context.Permissions.Add(new RolePermissionSetting { RoleId = adminRoleForDefaultTenant.Id, Name = "CanDeleteQuestions", IsGranted = true });
                context.SaveChanges();
            }

            //User role for 'Default' tenant
            var userRoleForDefaultTenant = context.Roles.FirstOrDefault(r => r.TenantId == defaultTenant.Id && r.Name == "User");
            if (userRoleForDefaultTenant == null)
            {
                userRoleForDefaultTenant = context.Roles.Add(new Role(defaultTenant.Id, "User", "User"));
                context.SaveChanges();

                //Permission definitions for User of 'Default' tenant
                context.Permissions.Add(new RolePermissionSetting { RoleId = userRoleForDefaultTenant.Id, Name = "CanCreateQuestions", IsGranted = true });
                context.SaveChanges();
            }

            //Admin for 'Default' tenant

            var adminUserForDefaultTenant = context.Users.FirstOrDefault(u => u.UserInTenants.Any(ut => ut.TenantId == defaultTenant.Id) && u.UserName == "systemadmin");
            if (adminUserForDefaultTenant == null)
            {
                adminUserForDefaultTenant = context.Users.Add(
                    new User
                    {
                        UserName = "admin_default",
                        Name = "System",
                        Surname = "Administrator",
                        EmailAddress = "admin@aspnetboilerplate.com",
                        IsEmailConfirmed = true,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    });

                context.SaveChanges();


                context.UserRoles.Add(new UserRole(adminUserForDefaultTenant.Id, adminRoleForDefaultTenant.Id));
                context.UserRoles.Add(new UserRole(adminUserForDefaultTenant.Id, userRoleForDefaultTenant.Id));
                context.SaveChanges();

                //question1.CreatorUserId = adminUserForDefaultTenant.Id;
                //context.SaveChanges();
            }

            if (context.UserTenants.FirstOrDefault(ut => ut.TenantId == defaultTenant.Id && ut.UserId == adminUserForDefaultTenant.Id) == null)
            {
                
                context.UserTenants.Add(new UserTenant()
                {
                    UserId = adminUserForDefaultTenant.Id,
                    TenantId = defaultTenant.Id
                });
                context.SaveChanges();
            }

            //User 'Emre' for 'Default' tenant
            

            var emreUserForDefaultTenant = context.Users.FirstOrDefault(u => u.UserInTenants.Any(ut => ut.TenantId == defaultTenant.Id) && u.UserName == "emre");
            if (emreUserForDefaultTenant == null)
            {
                emreUserForDefaultTenant = context.Users.Add(
                    new User
                    {
                        UserName = "emre",
                        Name = "Yunus Emre",
                        Surname = "Kalkan",
                        EmailAddress = "emre@aspnetboilerplate.com",
                        IsEmailConfirmed = true,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    });
                context.SaveChanges();


                context.UserRoles.Add(new UserRole(emreUserForDefaultTenant.Id, userRoleForDefaultTenant.Id));
                context.SaveChanges();


                //question2.CreatorUserId = emreUserForDefaultTenant.Id;
                //context.SaveChanges();
            }

            if (context.UserTenants.FirstOrDefault(ut => ut.TenantId == defaultTenant.Id && ut.UserId == emreUserForDefaultTenant.Id) == null)
            {

                context.UserTenants.Add(new UserTenant()
                {
                    UserId = emreUserForDefaultTenant.Id,
                    TenantId = defaultTenant.Id
                });
                context.SaveChanges();
            }
        }

        public void BuildAppContext(ModuleZeroSampleProjectDbContext context)
        {

            var question2 = context.Questions.Add(
                new Question(
                    "Jquery content replacement not working within my function",
                    @"What I am trying to achieve, and I am nearly there, is the user clicks on a checkbox and it turns green (Checkbox-active class). However, I also want the text/content of the clicked element to change to ""Activated"" and then reverts back to the original text when clicked again or on a sibling."
                    )
                );
            context.SaveChanges();

            var question1 = context.Questions.Add(
                new Question(
                    "What's the answer of ultimate question of life the universe and everything?",
                    "What's the answer of ultimate question of life the universe and everything? Please answer this question!"
                    )
                );
            context.SaveChanges();


        }
    }
}