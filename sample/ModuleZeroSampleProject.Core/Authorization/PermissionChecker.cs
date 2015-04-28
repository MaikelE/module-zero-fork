using Abp.Authorization;
using ModuleZeroSampleProject.MultiTenancy;
using ModuleZeroSampleProject.Users;

namespace ModuleZeroSampleProject.Authorization
{
    public class PermissionChecker : PermissionChecker<Tenant, Role, User,UserTenant>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}