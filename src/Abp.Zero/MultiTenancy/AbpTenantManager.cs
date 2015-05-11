using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Zero;
using Microsoft.AspNet.Identity;
using Abp.MultiTenancy;
using Abp.Configuration;
using Abp.Zero.Configuration;

namespace Abp.MultiTenancy
{
    /// <summary>
    /// Tenant manager.
    /// Implements domain logic for <see cref="AbpTenant{TTenant,TUser}"/>.
    /// </summary>
    /// <typeparam name="TTenant">Type of the application Tenant</typeparam>
    /// <typeparam name="TRole">Type of the application Role</typeparam>
    /// <typeparam name="TUser">Type of the application User</typeparam>
    public abstract class AbpTenantManager<TTenant, TRole, TUser, TUserTenant> : ITransientDependency
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TRole : AbpRole<TTenant, TUser, TUserTenant>
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        public ILocalizationManager LocalizationManager { get; set; }
       
        protected ISettingManager SettingManager { get; private set; }
        private readonly IRepository<TTenant> _tenantRepository;

        protected AbpTenantManager(IRepository<TTenant> tenantRepository,
            ISettingManager settingManager)
        {
            _tenantRepository = tenantRepository;
            SettingManager = settingManager;
            LocalizationManager = NullLocalizationManager.Instance;
        }

        public virtual IQueryable<TTenant> Tenants { get { return _tenantRepository.GetAll(); } }

        public virtual async Task<IdentityResult> CreateAsync(TTenant tenant)
        {            
            string hostName = await SettingManager.GetSettingValueForApplicationAsync(AbpZeroSettingNames.TenantManagement.HostDisplayName);
            if (hostName == tenant.TenancyName || tenant.Name == hostName)
            {
                return AbpIdentityResult.Failed(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            if (await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenant.TenancyName) != null)
            {
                return AbpIdentityResult.Failed(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            var validationResult = await ValidateTenantAsync(tenant);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            await _tenantRepository.InsertAsync(tenant);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TTenant tenant)
        {
            string hostName = await SettingManager.GetSettingValueForApplicationAsync(AbpZeroSettingNames.TenantManagement.HostDisplayName);
            if (hostName == tenant.TenancyName || tenant.Name == hostName)
            {
                return AbpIdentityResult.Failed(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            if (await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenant.TenancyName && t.Id != tenant.Id) != null)
            {
                return AbpIdentityResult.Failed(string.Format(L("TenancyNameIsAlreadyTaken"), tenant.TenancyName));
            }

            await _tenantRepository.UpdateAsync(tenant);
            return IdentityResult.Success;
        }

        public virtual async Task<TTenant> FindByIdAsync(int id)
        {
            return await _tenantRepository.FirstOrDefaultAsync(id);
        }

        public virtual async Task<TTenant> GetByIdAsync(int id)
        {
            var tenant = await FindByIdAsync(id);
            if (tenant == null)
            {
                throw new AbpException("There is no tenant with id: " + id);
            }

            return tenant;
        }

        public Task<TTenant> FindByTenancyNameAsync(string tenancyName)
        {
            return _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
        }

        public virtual async Task<IdentityResult> DeleteAsync(TTenant tenant)
        {
            await _tenantRepository.DeleteAsync(tenant);
            return IdentityResult.Success;
        }

        protected virtual async Task<IdentityResult> ValidateTenantAsync(TTenant tenant)
        {
            var nameValidationResult = await ValidateTenancyNameAsync(tenant.TenancyName);
            if (!nameValidationResult.Succeeded)
            {
                return nameValidationResult;
            }

            return IdentityResult.Success;
        }

        protected virtual async Task<IdentityResult> ValidateTenancyNameAsync(string tenancyName)
        {
            if (!Regex.IsMatch(tenancyName, AbpTenant<TTenant, TUser,TUserTenant>.TenancyNameRegex))
            {
                return AbpIdentityResult.Failed(L("InvalidTenancyName"));
            }

            return IdentityResult.Success;
        }

        private string L(string name)
        {
            return LocalizationManager.GetString(AbpZeroConsts.LocalizationSourceName, name);
        }
    }
}