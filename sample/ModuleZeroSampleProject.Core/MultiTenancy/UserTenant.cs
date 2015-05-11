using Abp.MultiTenancy;
using ModuleZeroSampleProject.Users;

namespace ModuleZeroSampleProject.MultiTenancy
{
    public class UserTenant : AbpUserTenant<Tenant, User, UserTenant>
    {
        public UserTenant()
        {

        }

    }
}