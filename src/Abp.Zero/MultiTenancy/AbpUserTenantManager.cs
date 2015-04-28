using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.Zero;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.MultiTenancy
{
    /// <summary>
    /// User Tenant manager.
    /// Implements domain logic for <see cref="AbpUserTenant{TTenant,TUser}"/>.
    /// </summary>
    /// <typeparam name="TTenant">Type of the application Tenant</typeparam>
    /// <typeparam name="TUser">Type of the application User</typeparam>
    public abstract class AbpUserTenantManager<TTenant, TUser, TUserTenant> : ITransientDependency
        where TTenant : AbpTenant<TTenant, TUser,TUserTenant >
        where TUser : AbpUser<TTenant, TUser,TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant,TUser,TUserTenant>
    {
        public ILocalizationManager LocalizationManager { get; set; }

        private readonly IRepository<TUserTenant> _userTenantRepository;

        protected AbpUserTenantManager(IRepository<TUserTenant> userTenantRepository)
        {
            _userTenantRepository = userTenantRepository;         
            LocalizationManager = NullLocalizationManager.Instance;
        }

        public virtual async Task<IdentityResult> CreateAsync(TUserTenant userTenant)
        {
          
            var validationResult = await ValidateUserTenantAsync(userTenant);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            await _userTenantRepository.InsertAsync(userTenant);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TUserTenant userTenant)
        {
            if (await _userTenantRepository.FirstOrDefaultAsync(t => t.Id != userTenant.Id && t.TenantId == userTenant.TenantId && t.UserId == userTenant.UserId) != null)
            {
                return AbpIdentityResult.Failed(string.Format(L("UserTenantRelationAlreadyExists")));
            }

            await _userTenantRepository.UpdateAsync(userTenant);
            return IdentityResult.Success;
        }

        public virtual async Task<TUserTenant> FindByIdAsync(int id)
        {
            return await _userTenantRepository.FirstOrDefaultAsync(id);
        }

        public virtual async Task<TUserTenant> GetByIdAsync(int id)
        {
            var tenant = await FindByIdAsync(id);
            if (tenant == null)
            {
                throw new AbpException("There is no usertenant with id: " + id);
            }

            return tenant;
        }

        public async Task<ICollection<TUserTenant>> GetAllByUserId(long userId)
        {
            return await _userTenantRepository.GetAllListAsync(ut => ut.UserId == userId); 
        }

        public async Task<ICollection<TUserTenant>> GetAllByTenantId(int tenantId)
        {
            return await _userTenantRepository.GetAllListAsync(ut => ut.TenantId == tenantId);
        }

        //public async Task<ICollection<TUser>> GetAllUsersByTenantId(int tenantId)
        //{
        //    var list = await _userTenantRepository.GetAllListAsync(ut => ut.TenantId == tenantId);
        //  //  var listUsers = ;
        //    return await _userRepository.GetAllListAsync(u => list.Any(ut => ut.UserId ==  u.Id));
        //}

        //public async Task<ICollection<TTenant>> GetAllTenantsByUserId(long userId)
        //{
        //    var list = await _userTenantRepository.GetAllListAsync(ut => ut.UserId == userId);
        //    return await _tenantRepository.GetAllListAsync(u => list.Any(ut => ut.TenantId ==  u.Id));
        //}

        public virtual async Task<IdentityResult> DeleteAsync(TUserTenant userTenant)
        {
            await _userTenantRepository.DeleteAsync(userTenant);
            return IdentityResult.Success;
        }

        protected virtual async Task<IdentityResult> ValidateUserTenantAsync(TUserTenant userTenant)
        {

            var nameValidationResult = await ValidateUserTenancyExistAsync(userTenant.UserId, userTenant.TenantId);
            if (!nameValidationResult.Succeeded)
            {
                return nameValidationResult;
            }

            return IdentityResult.Success;
        }

        protected virtual async Task<IdentityResult> ValidateUserTenancyExistAsync(long userId, int? tenantId)
        {
            if (await _userTenantRepository.FirstOrDefaultAsync(ut => ut.TenantId == tenantId && ut.UserId == userId) != null)
            {
                return AbpIdentityResult.Failed(L("UserTenantRelationAlreadyExists"));
            }
           
            return IdentityResult.Success;
        }

        private string L(string name)
        {
            return LocalizationManager.GetString(AbpZeroConsts.LocalizationSourceName, name);
        }
    }
}
