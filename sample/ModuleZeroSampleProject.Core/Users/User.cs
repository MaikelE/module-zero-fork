using Abp.Authorization.Users;
using ModuleZeroSampleProject.MultiTenancy;

namespace ModuleZeroSampleProject.Users
{
    public class User : AbpUser<Tenant, User,UserTenant>
    {
        public override string ToString()
        {
            return string.Format("[User {0}] {1}", Id, UserName);
        }
    }
}