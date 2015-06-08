using System.Linq;
using Abp.Zero.SampleApp.EntityFramework;
using Abp.Zero.SampleApp.MultiTenancy;
using Abp.Zero.SampleApp.Users;

namespace Abp.Zero.SampleApp.Tests.Users
{
    public class UserLoginHelper
    {
        public static void CreateTestUsers(AppDbContext context)
        {
            var defaultTenant = context.Tenants.Single(t => t.TenancyName == Tenant.DefaultTenantName);

            var testUser1 = context.Users.Add(
                new User
                {
                    //Tenant = null, //Tenancy owner
                    UserName = "userOwner",
                    Name = "Owner",
                    Surname = "One",
                    EmailAddress = "owner@aspnetboilerplate.com",
                    IsEmailConfirmed = true,
                    Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                });
            testUser1.UserInTenants.Add(new UserTenant()
                {
                    User = testUser1,
                });
            var testUser2 = context.Users.Add(
                new User
                {
                    //Tenant = defaultTenant, //A user of tenant1
                    UserName = "user1",
                    Name = "User",
                    Surname = "One",
                    EmailAddress = "user-one@aspnetboilerplate.com",
                    IsEmailConfirmed = false,
                    Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                });
           testUser2.UserInTenants.Add(new UserTenant()
            {
                User = testUser2,
                Tenant = defaultTenant,
            });
        }
    }
}