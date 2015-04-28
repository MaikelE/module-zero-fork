using Abp.Authorization.Users;
using Abp.Domain.Entities;

namespace Abp.MultiTenancy
{
    /// <summary>
    /// Implement this interface for an entity which may have Tenant.
    /// </summary>
    public interface IMayHaveTenant<TTenant, TUser, TUserTenant> : IMayHaveTenant
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        /// <summary>
        /// Tenant.
        /// </summary>
        TTenant Tenant { get; set; }
    }
}