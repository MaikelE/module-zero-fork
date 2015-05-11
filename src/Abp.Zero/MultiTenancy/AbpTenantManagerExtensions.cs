using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Threading;

namespace Abp.MultiTenancy
{
    //TODO: Create other sync extension methods.
    public static class AbpTenantManagerExtensions
    {
        public static TTenant GetById<TTenant, TRole, TUser, TUserTenant>(this AbpTenantManager<TTenant, TRole, TUser, TUserTenant> tenantManager, int id)
            where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
            where TRole : AbpRole<TTenant, TUser, TUserTenant>
            where TUser : AbpUser<TTenant, TUser, TUserTenant>
            where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
        {
            return AsyncHelper.RunSync(() => tenantManager.GetByIdAsync(id));
        }
    }
}