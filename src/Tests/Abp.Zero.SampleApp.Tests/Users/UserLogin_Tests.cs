using System.Threading.Tasks;
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
        private readonly UserTenantManager _userTenantManager;


        public UserLogin_Tests()
        {
            UsingDbContext(
                context =>
                {
                    var tenant1 = context.Tenants.Add(new Tenant("tenant1", "Tenant one") { Id = 1 });
                    var userOwner = new User
                        {
                           Id=1,
                            UserName = "userOwner",
                            Name = "Owner",
                            Surname = "One",
                            EmailAddress = "owner@aspnetboilerplate.com",
                            IsEmailConfirmed = true,
                            Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                        };

                    context.Users.Add(userOwner);

                    var userOwner_TenantNull = context.UserTenants.Add(new UserTenant()
                        {
                            Id=2,
                            User = userOwner,
                            Tenant  = null
                        });

                    var user1 = new User
                        {
                            Id = 2,
                            UserName = "user1",
                            Name = "User",
                            Surname = "One",
                            EmailAddress = "user-one@aspnetboilerplate.com",
                            IsEmailConfirmed = false,
                            Password = "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" //123qwe
                        };

                    context.Users.Add(user1);

                    var user1_Tenant1 = context.UserTenants.Add(new UserTenant()
                    {
                        Id = 3,
                        UserId = 2,
                        User = user1,
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
                        User = user3
                    });

                });

            _userManager = LocalIocManager.Resolve<UserManager>();;
            
            _userTenantManager = LocalIocManager.Resolve<UserTenantManager>();
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
            //AbpSession.TenantId = 1; //TODO: We should not need to set this and implement AbpSession instead of TestSession.

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
        
        [Fact]
        public async Task Should_Not_Login_TenancyOwner_With_Correct_Values_In_Tenant1()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            var loginResult = await _userManager.LoginAsync("userOwner", "123qwe", "tenant1");
            loginResult.Result.ShouldBe(AbpLoginResultType.InvalidUserNameOrEmailAddress);  
            loginResult.Identity.ShouldBe(null);
        }

        [Fact]
        public async Task Should_Login_User1_With_Correct_Values_In_Tenant1()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            var loginResult = await _userManager.LoginAsync("user1", "123qwe", "tenant1");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.Identity.ShouldNotBe(null);
            loginResult.Identity.IsAuthenticated.ShouldBe(true);
        }

        [Fact]
        public async Task Should_Login_User1_With_Correct_Values_In_Tenant1_TwoFactorLogin()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            var loginResult = await _userManager.LoginAsync("user1", "123qwe", "tenant1");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.Identity.ShouldNotBe(null);
            loginResult.Identity.IsAuthenticated.ShouldBe(true);
        }
        
        [Fact]
        public async Task Should_Not_Login_User1_With_Correct_Values_Without_Tenancyname_And_IsTenantNameRequiredWithLogin_true()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;
            await Resolve<ISettingManager>().ChangeSettingForApplicationAsync( AbpZeroSettingNames.TenantManagement.IsTenantNameRequiredWithLogin, "true");
            var loginResult = await _userManager.LoginAsync("user1", "123qwe");
            loginResult.Result.ShouldBe(AbpLoginResultType.NoTenancyNameProvided);
            loginResult.User.ShouldBe(null);
            loginResult.Identity.ShouldBe(null);
            
        }


        [Fact]
        public async Task Should_Login_User1_With_Correct_Values_Without_Tenancyname_And_IsTenantNameRequiredWithLogin_false()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;
            await Resolve<ISettingManager>().ChangeSettingForApplicationAsync( AbpZeroSettingNames.TenantManagement.IsTenantNameRequiredWithLogin, "false");
            var loginResult = await _userManager.LoginAsync("user1", "123qwe");
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
        }
        
        [Fact]
        public async Task Should_Not_Login_User3_With_Correct_Values_Without_Tenancyname_In_Host()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;
            var user = await UserManager.GetUserByIdAsync(3);
            
            //int aantalUserTenants = user.UserInTenants.Count;
            var loginResult = await _userManager.LoginAsync("user3", "123qwe");
            loginResult.Result.ShouldBe(AbpLoginResultType.UserNeedsToChooseTenant);
            loginResult.Identity.ShouldBe(null);
            loginResult.User.ShouldNotBe(null);
            loginResult.User.UserName.ShouldBe("user3");
        }

        [Fact]
        public async Task Should_Have_Two_UserTenants_Stored_For_User3()
        {
            var usertenants = await _userTenantManager.GetAllByUserId(3);
            usertenants.Count.ShouldBe(2);
        }


        [Fact]
        public async Task Should_Login_User3_With_Correct_Values_In_Host()
        {
            Resolve<IMultiTenancyConfig>().IsEnabled = true;

            var hostname = await Resolve<ISettingManager>().GetSettingValueForApplicationAsync(AbpZeroSettingNames.TenantManagement.HostDisplayName);
            //await Resolve<ISettingManager>().ChangeSettingForApplicationAsync( AbpZeroSettingNames.UserManagement.IsTwoFactorRequiredForLogin, "true");
            //ToDo settingmanager hostdisplayname
            var loginResult = await _userManager.LoginAsync("user3", "123qwe", hostname);
            loginResult.Result.ShouldBe(AbpLoginResultType.Success);
            loginResult.Identity.ShouldNotBe(null);
        }
        
        //[Fact]
        //public async Task Should_Not_Login_TenancyOwner_With_Correct_Values_In_Tenant1()
        //{
        //    Resolve<IMultiTenancyConfig>().IsEnabled = true;

        //    var loginResult = await _userManager.LoginAsync("userOwner", "123qwe", "tenant1");
        //    loginResult.Result.ShouldBe(AbpLoginResultType.UserNotWithTenant);           
        //    loginResult.Identity.ShouldBe(null);
        //}

    }
}
