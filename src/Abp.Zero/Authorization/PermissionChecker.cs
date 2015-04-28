using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Castle.Core.Logging;

namespace Abp.Authorization
{
    /// <summary>
    /// Application should inherit this class to implement <see cref="IPermissionChecker"/>.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TUserTenant"></typeparam>
    public abstract class PermissionChecker<TTenant, TRole, TUser, TUserTenant> : IPermissionChecker, ITransientDependency
        where TRole : AbpRole<TTenant, TUser, TUserTenant>, new()
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>, new()
    {
        private readonly AbpUserManager<TTenant, TRole, TUser, TUserTenant> _userManager;

        public ILogger Logger { get; set; }

        public IAbpSession AbpSession { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected PermissionChecker(AbpUserManager<TTenant, TRole, TUser, TUserTenant> userManager)
        {
            _userManager = userManager;

            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        public async Task<bool> IsGrantedAsync(string permissionName)
        {
            return AbpSession.UserId.HasValue && await _userManager.IsGrantedAsync(AbpSession.UserId.Value, permissionName);
        }

        public async Task<bool> IsGrantedAsync(long userId, string permissionName)
        {
            return await _userManager.IsGrantedAsync(userId, permissionName);
        }
    }
}
