using System.Threading.Tasks;
using Abp.Zero.SampleApp.MultiTenancy;
using Shouldly;
using Xunit;
using Abp.Configuration;
using Abp.Zero.Configuration;

namespace Abp.Zero.SampleApp.Tests.MultiTenancy
{
    public class TenantManager_Tests : SampleAppTestBase
    {
        private readonly TenantManager _tenantManager;
        
        public TenantManager_Tests()
        {
            _tenantManager = Resolve<TenantManager>();
            
        }

        [Fact]
        public async Task Should_Not_Create_Duplicate_Tenant()
        {
            (await _tenantManager.CreateAsync(new Tenant("Tenant-X", "Tenant X"))).Succeeded.ShouldBe(true);
            
            //Trying to re-create with same tenancy name
            
            var result = (await _tenantManager.CreateAsync(new Tenant("Tenant-X", "Tenant X")));
            result.Succeeded.ShouldBe(false);
        }


        [Fact]
        public async Task Should_Not_Create_Tenant_With_Same_Name_As_Host()
        {
            string HostDisplayName = "Hostname123";
            await Resolve<ISettingManager>().ChangeSettingForApplicationAsync(AbpZeroSettingNames.TenantManagement.HostDisplayName, HostDisplayName);

            (await _tenantManager.CreateAsync(new Tenant(HostDisplayName, HostDisplayName))).Succeeded.ShouldBe(false);
        }
    }
}
