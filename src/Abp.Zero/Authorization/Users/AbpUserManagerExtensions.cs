using System;
using Abp.Authorization.Roles;
using Abp.MultiTenancy;
using Abp.Threading;

namespace Abp.Authorization.Users
{
    /// <summary>
    /// Extension methods for <see cref="AbpUserManager{TTenant,TRole,TUser}"/>.
    /// </summary>
    public static class AbpUserManagerExtensions
    {
        /// <summary>
        /// Check whether a user is granted for a permission.
        /// </summary>
        /// <param name="manager">User manager</param>
        /// <param name="userId">User id</param>
        /// <param name="permissionName">Permission name</param>
        public static bool IsGranted<TTenant, TRole, TUser, TUserTenant>(AbpUserManager<TTenant, TRole, TUser, TUserTenant> manager, long userId, string permissionName)
            where TTenant : AbpTenant<TTenant, TUser,TUserTenant >
            where TRole : AbpRole<TTenant, TUser, TUserTenant>, new()
            where TUser : AbpUser<TTenant, TUser, TUserTenant>
            where TUserTenant : AbpUserTenant<TTenant, TUser,TUserTenant >, new()
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            return AsyncHelper.RunSync(() => manager.IsGrantedAsync(userId, permissionName));
        }

        public static AbpUserManager<TTenant, TRole, TUser, TUserTenant>.AbpLoginResult Login<TTenant, TRole, TUser, TUserTenant>(AbpUserManager<TTenant, TRole, TUser, TUserTenant> manager, string userNameOrEmailAddress, string plainPassword, string tenancyName = null)
            where TTenant : AbpTenant<TTenant, TUser,TUserTenant>
            where TRole : AbpRole<TTenant, TUser, TUserTenant>, new()
            where TUser : AbpUser<TTenant, TUser, TUserTenant>
            where TUserTenant : AbpUserTenant<TTenant, TUser,TUserTenant>, new()
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            return AsyncHelper.RunSync(() => manager.LoginAsync(userNameOrEmailAddress, plainPassword, tenancyName));
        }
    }
}