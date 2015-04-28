﻿using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using Abp.Zero.SampleApp.MultiTenancy;
using Abp.Zero.SampleApp.Users;
using Shouldly;
using Xunit;

namespace Abp.Zero.SampleApp.Tests.Users
{
    public class UserLogin_Tests : SampleAppTestBase
    {
        private readonly UserManager _userManager;

        public UserLogin_Tests()
        {
            UsingDbContext(
                context =>
                {
                    var tenant1 = context.Tenants.Add(new Tenant("tenant1", "Tenant one") { Id = 1 });
                    var user1 =  new User
                        {
                           Id=1,
                            UserName = "userOwner",
                            Name = "Owner",
                            Surname = "One",
                            EmailAddress = "owner@aspnetboilerplate.com",
                            IsEmailConfirmed = true,
                            Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                        };

                    context.Users.Add(user1);

                    var userOwner_TenantNull = context.UserTenants.Add(new UserTenant()
                        {
                            Id=2,
                            User = user1,
                            Tenant  = null
                        });

                    var user2 = new User
                        {
                            Id = 2,
                            UserName = "user1",
                            Name = "User",
                            Surname = "One",
                            EmailAddress = "user-one@aspnetboilerplate.com",
                            IsEmailConfirmed = false,
                            Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                        };

                    context.Users.Add(user2);

                    var user1_Tenant1 = context.UserTenants.Add(new UserTenant()
                    {
                        Id = 3,
                        UserId = 2,
                        User = user2,
                        TenantId = 1,
                        Tenant = tenant1
                    });


                    var user3 = new User
                    {
                        Id = 3,
                        UserName = "user3",
                        Name = "User3",
                        Surname = "One3",
                        EmailAddress = "user-3@aspnetboilerplate.com",
                        IsEmailConfirmed = false,
                        Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                    };

                    context.Users.Add(user3);

                    context.UserTenants.Add(new UserTenant()
                    {
                        Id = 4,
                        UserId = 3,
                        User = user3,
                        TenantId = 1,
                        Tenant = tenant1
                    });

                    context.UserTenants.Add(new UserTenant()
                    {
                        Id = 5,
                        UserId = 3,
                        User = user3,
                        Tenant = null
                    });

                });

            _userManager = LocalIocManager.Resolve<UserManager>();
        }

        [Fact]
        public async Task Should_Login_With_Correct_Values_Without_MultiTenancy()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = false;
            AbpSession.TenantId = 1; //TODO: We should not need to set this and implement AbpSession instead of TestSession.

            var loginResult = await _userManager.LoginAsync("user1", "123qwe");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.User.Name.ShouldBe("User");
            loginResult.Identity.ShouldNotBe(null);
        }

        [Fact]
        public async Task Should_Not_Login_With_Invalid_UserName_Without_MultiTenancy()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = false;
            AbpSession.TenantId = 1; //TODO: We should not need to set this and implement AbpSession instead of TestSession.

            var loginResult = await _userManager.LoginAsync("wrongUserName", "asdfgh");
            loginResult.Result.ShouldBe(AbpLoginResultType.InvalidUserNameOrEmailAddress);
            loginResult.User.ShouldBe(null);
            loginResult.Identity.ShouldBe(null);
        }

        [Fact]
        public async Task Should_Login_With_Correct_Values_With_MultiTenancy()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;
            AbpSession.TenantId = 1; //TODO: We should not need to set this and implement AbpSession instead of TestSession.

            var loginResult = await _userManager.LoginAsync("user1", "123qwe", "tenant1");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.User.Name.ShouldBe("User");
            loginResult.Identity.ShouldNotBe(null);
        }

        [Fact]
        public async Task Should_Not_Login_If_Email_Confirmation_Is_Enabled_And_User_Has_Not_Confirmed()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            //Set session
            AbpSession.TenantId = 1;
            AbpSession.UserId = 1;

            //Email confirmation is disabled as default
            (await _userManager.LoginAsync("user1", "123qwe", "tenant1")).Result.ShouldBe(AbpLoginResultType.Success);

            //Change configuration
            await Resolve<ISettingManager>().ChangeSettingForTenantAsync(AbpSession.GetTenantId(), AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin, "true");

            //Email confirmation is enabled now
            (await _userManager.LoginAsync("user1", "123qwe", "tenant1")).Result.ShouldBe(AbpLoginResultType.UserEmailIsNotConfirmed);
        }

        [Fact]
        public async Task Should_Login_TenancyOwner_With_Correct_Values()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            var loginResult = await _userManager.LoginAsync("userOwner", "123qwe");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.User.Name.ShouldBe("Owner");
            loginResult.Identity.ShouldNotBe(null);
        }
    }
}
