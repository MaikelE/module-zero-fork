using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using Microsoft.AspNet.Identity;

namespace Abp.Authorization.Users
{
    /// <summary>
    /// Extends <see cref="UserManager{TUser,TKey}"/> of ASP.NET Identity Framework.
    /// </summary>
    public abstract class AbpUserManager<TTenant, TRole, TUser, TUserTenant> : UserManager<TUser, long>, ITransientDependency
        where TTenant : AbpTenant<TTenant, TUser,TUserTenant >
        where TRole : AbpRole<TTenant, TUser,TUserTenant>, new()
        where TUser : AbpUser<TTenant, TUser,TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser,TUserTenant >, new()
    {
        private IUserPermissionStore<TTenant, TUser,TUserTenant> UserPermissionStore
        {
            get
            {
                if (!(Store is IUserPermissionStore<TTenant, TUser,TUserTenant>))
                {
                    throw new AbpException("Store is not IUserPermissionStore");
                }

                return Store as IUserPermissionStore<TTenant, TUser,TUserTenant>;
            }
        }

        public ILocalizationManager LocalizationManager { get; set; }

        public IAbpSession AbpSession { get; set; }

        protected AbpRoleManager<TTenant, TRole, TUser,TUserTenant> RoleManager { get; private set; }
        protected ISettingManager SettingManager { get; private set; }
        
        protected AbpUserStore<TTenant, TRole, TUser,TUserTenant> AbpStore { get; private set; }

        protected AbpUserTenantManager<TTenant, TUser, TUserTenant> UserTenantManager { get; private set; }

        private readonly IPermissionManager _permissionManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IUserManagementConfig _userManagementConfig;
        private readonly IIocResolver _iocResolver;
        private readonly IRepository<TTenant> _tenantRepository;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        //TODO: Non-generic parameters may be converted to property-injection
        protected AbpUserManager(
            AbpUserStore<TTenant, TRole, TUser,TUserTenant> userStore,
            AbpRoleManager<TTenant, TRole, TUser,TUserTenant> roleManager,
            AbpUserTenantManager<TTenant, TUser, TUserTenant> userTenantManager,
            IRepository<TTenant> tenantRepository,
            IMultiTenancyConfig multiTenancyConfig,
            IPermissionManager permissionManager,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver)
            : base(userStore)
        {
            AbpStore = userStore;
            RoleManager = roleManager;
            SettingManager = settingManager;
            UserTenantManager = userTenantManager;
            _tenantRepository = tenantRepository;
            _multiTenancyConfig = multiTenancyConfig;
            _permissionManager = permissionManager;
            _unitOfWorkManager = unitOfWorkManager;
            _userManagementConfig = userManagementConfig;
            _iocResolver = iocResolver;
            LocalizationManager = NullLocalizationManager.Instance;
        }

        public override async Task<IdentityResult> CreateAsync(TUser user)
        {
            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress);
            if (!result.Succeeded)
            {
                return result;
            }

            
            //create user
            var createUserResult = await base.CreateAsync(user);
            if (!createUserResult.Succeeded)
                return createUserResult;
            //if it has a tenantId set, add a UserTenant relation record
            if (AbpSession.TenantId.HasValue)
            {
                return await CreateUserTenantAsync(user.Id, AbpSession.TenantId.Value);
            }
            else
            {
                //return result of usercreation
                return createUserResult;
            }
        }


        public virtual async Task<IdentityResult> CreateUserTenantAsync(long userId, int? tenantId)
        {
            return  await UserTenantManager.CreateAsync(new TUserTenant()
            {
                UserId = userId,
                TenantId = tenantId
            });
        }

        /// <summary>
        /// Check whether a user is granted for a permission.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="permissionName">Permission name</param>
        public virtual async Task<bool> IsGrantedAsync(long userId, string permissionName)
        {
            return await IsGrantedAsync(
                await GetUserByIdAsync(userId),
                _permissionManager.GetPermission(permissionName)
                );
        }



        /// <summary>
        /// Check whether a user is granted for a permission.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="permission">Permission</param>
        public virtual async Task<bool> IsGrantedAsync(TUser user, Permission permission)
        {
            //Check for multi-tenancy side
            if (!permission.MultiTenancySides.HasFlag(AbpSession.MultiTenancySide))
            {
                return false;
            }

            //Check for user-specific value
            if (await UserPermissionStore.HasPermissionAsync(user, new PermissionGrantInfo(permission.Name, true)))
            {
                return true;
            }

            if (await UserPermissionStore.HasPermissionAsync(user, new PermissionGrantInfo(permission.Name, false)))
            {
                return false;
            }

            //Check for roles
            var roleNames = await GetRolesAsync(user.Id);
            if (!roleNames.Any())
            {
                return permission.IsGrantedByDefault;
            }

            foreach (var roleName in roleNames)
            {
                if (await RoleManager.HasPermissionAsync(roleName, permission.Name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets granted permissions for a user.
        /// </summary>
        /// <param name="user">Role</param>
        /// <returns>List of granted permissions</returns>
        public virtual async Task<IReadOnlyList<Permission>> GetGrantedPermissionsAsync(TUser user)
        {
            var permissionList = new List<Permission>();

            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                if (await IsGrantedAsync(user, permission))
                {
                    permissionList.Add(permission);
                }
            }

            return permissionList;
        }

        /// <summary>
        /// Sets all granted permissions of a user at once.
        /// Prohibits all other permissions.
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="permissions">Permissions</param>
        public virtual async Task SetGrantedPermissionsAsync(TUser user, IEnumerable<Permission> permissions)
        {
            var oldPermissions = await GetGrantedPermissionsAsync(user);
            var newPermissions = permissions.ToArray();

            foreach (var permission in oldPermissions.Where(p => !newPermissions.Contains(p)))
            {
                await ProhibitPermissionAsync(user, permission);
            }

            foreach (var permission in newPermissions.Where(p => !oldPermissions.Contains(p)))
            {
                await GrantPermissionAsync(user, permission);
            }
        }

        /// <summary>
        /// Prohibits all permissions for a user.
        /// </summary>
        /// <param name="user">User</param>
        public async Task ProhibitAllPermissionsAsync(TUser user)
        {
            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                await ProhibitPermissionAsync(user, permission);
            }
        }

        /// <summary>
        /// Resets all permission settings for a user.
        /// It removes all permission settings for the user.
        /// User will have permissions according to his roles.
        /// This method does not prohibit all permissions.
        /// For that, use <see cref="ProhibitAllPermissionsAsync"/>.
        /// </summary>
        /// <param name="user">User</param>
        public async Task ResetAllPermissionsAsync(TUser user)
        {
            await UserPermissionStore.RemoveAllPermissionSettingsAsync(user);
        }

        /// <summary>
        /// Grants a permission for a user if not already granted.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="permission">Permission</param>
        public virtual async Task GrantPermissionAsync(TUser user, Permission permission)
        {
            await UserPermissionStore.RemovePermissionAsync(user, new PermissionGrantInfo(permission.Name, false));

            if (await IsGrantedAsync(user, permission))
            {
                return;
            }

            await UserPermissionStore.AddPermissionAsync(user, new PermissionGrantInfo(permission.Name, true));
        }

        /// <summary>
        /// Prohibits a permission for a user if it's granted.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="permission">Permission</param>
        public virtual async Task ProhibitPermissionAsync(TUser user, Permission permission)
        {
            await UserPermissionStore.RemovePermissionAsync(user, new PermissionGrantInfo(permission.Name, true));

            if (!await IsGrantedAsync(user, permission))
            {
                return;
            }

            await UserPermissionStore.AddPermissionAsync(user, new PermissionGrantInfo(permission.Name, false));
        }

        public virtual async Task<TUser> FindByNameOrEmailAsync(string userNameOrEmailAddress)
        {
            return await AbpStore.FindByNameOrEmailAsync(userNameOrEmailAddress);
        }

        [UnitOfWork]
        public virtual async Task<AbpLoginResult> LoginAsync(string userNameOrEmailAddress, string plainPassword, string tenancyName = null)
        {

            if (userNameOrEmailAddress.IsNullOrEmpty())
            {
                throw new ArgumentNullException("userNameOrEmailAddress");
            }

            if (plainPassword.IsNullOrEmpty())
            {
                throw new ArgumentNullException("plainPassword");
            }

            //using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            //{

            TTenant tenant = null;
            bool userNeedsToChooseTenant = false;
            bool LoginInHost = false;
            if (!_multiTenancyConfig.IsEnabled)
            {
                tenant = await GetDefaultTenantAsync();
            }
            else
            {
                var tenantNameRequiredAtLogin = SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.TenantManagement.IsTenantNameRequiredWithLogin);
                var hostDisplayName = SettingManager.GetSettingValueForApplicationAsync(AbpZeroSettingNames.TenantManagement.HostDisplayName);
                if (await tenantNameRequiredAtLogin && string.IsNullOrWhiteSpace(tenancyName))
                {
                    return new AbpLoginResult(AbpLoginResultType.NoTenancyNameProvided);
                }

                if ((await hostDisplayName) == tenancyName)
                {
                    //Log in as host user if user cannot login as host, then he cannot login.
                    //tenant = null; // is al null
                    //user = await AbpStore.FindByNameOrEmailAsync(null, userNameOrEmailAddress);                       
                    LoginInHost = true;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(tenancyName))
                    {
                        //Log in as tenant user
                        tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                        if (tenant == null)
                        {
                            return new AbpLoginResult(AbpLoginResultType.InvalidTenancyName);
                        }

                        if (!tenant.IsActive)
                        {
                            return new AbpLoginResult(AbpLoginResultType.TenantIsNotActive);
                        }
                        //user = await AbpStore.FindByNameOrEmailAsync(tenant.Id, userNameOrEmailAddress);
                        //if (user != null)
                        //{
                        //    var userTenant = await UserTenantManager.FindByUserIdAndTenantIdAsync(user.Id, tenant.Id);
                        //    if (userTenant == null || userTenant.IsDeleted)
                        //    {
                        //        return new AbpLoginResult(AbpLoginResultType.UserNotWithTenant);
                        //    }

                        //    if (userTenant == null)
                        //    {
                        //        return new AbpLoginResult(AbpLoginResultType.UserNotActiveWithTenant);
                        //    }
                        //}
                    }
                    else
                    {
                        //no tenant selected, select user and let user schoose tenant
                        //using (_unitOfWorkManager.Current.EnableFilter(AbpDataFilters.MayHaveTenant))
                        //{
                        //user = await FindByNameOrEmailAsync(userNameOrEmailAddress);
                        //if (user == null)
                        //{
                        //    return new AbpLoginResult(AbpLoginResultType.InvalidUserNameOrEmailAddress);
                        //}

                        //if (user.UserInTenants.Count > 1) //if user has multiple tenants let him first choose the tenant
                        //{
                        //    userNeedsToChooseTenant = true;
                        //}
                        //}                            
                    }
                }
            }

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                TUser user;
                if (await TryLoginFromExternalAuthenticationSources(userNameOrEmailAddress, plainPassword, tenant, LoginInHost))
                {
                    if (tenant != null || LoginInHost)
                    {
                        user = await AbpStore.FindByNameOrEmailAsync(tenant == null ? (int?)null : tenant.Id, userNameOrEmailAddress);
                        if (user != null)
                        {

                        }                        
                    }
                    else
                    {
                        user = await FindByNameOrEmailAsync(userNameOrEmailAddress);
                        if (user == null)
                        {
                            return new AbpLoginResult(AbpLoginResultType.InvalidUserNameOrEmailAddress);
                        }
                        if (user != null)
                        {

                        }

                    }
                }
                else
                {
                    user = await AbpStore.FindByNameOrEmailAsync(tenant == null ? (int?)null : tenant.Id, userNameOrEmailAddress);
                    if (user == null)
                    {
                        return new AbpLoginResult(AbpLoginResultType.InvalidUserNameOrEmailAddress);
                    }

                    var verificationResult = new PasswordHasher().VerifyHashedPassword(user.Password, plainPassword);
                    if (verificationResult != PasswordVerificationResult.Success)
                    {
                        return new AbpLoginResult(AbpLoginResultType.InvalidPassword);
                    }
                }

                if (!user.IsActive)
                {
                    return new AbpLoginResult(AbpLoginResultType.UserIsNotActive);
                }
                if (user != null)
                {
                    var userTenant = await UserTenantManager.FindByUserIdAndTenantIdAsync(user.Id, tenant.Id);
                    if (userTenant != null && userTenant.IsDeleted)
                    {
                        return new AbpLoginResult(AbpLoginResultType.UserNotWithTenant);
                    }

                    if (userTenant == null)
                    {
                        return new AbpLoginResult(AbpLoginResultType.UserNotActiveWithTenant);
                    }
                }

                if (tenant != null)
                {
                    if (await IsEmailConfirmationRequiredForLoginAsync(tenant.Id) && !user.IsEmailConfirmed)
                    {
                        return new AbpLoginResult(AbpLoginResultType.UserEmailIsNotConfirmed);
                    }
                }

                if (!userNeedsToChooseTenant)
                {
                    user.LastTenantId = tenant == null? (int?)null : tenant.Id;
                }

                user.LastLoginTime = Clock.Now;
                await Store.UpdateAsync(user);

                await _unitOfWorkManager.Current.SaveChangesAsync();
                if (userNeedsToChooseTenant)
                {
                    return new AbpLoginResult(user);
                }

                //log user in with tenantId
                if (tenant != null)
                {
                    return new AbpLoginResult(user, await CreateIdentityAsyncWithTenant(user, DefaultAuthenticationTypes.ApplicationCookie, tenant.Id));
                }
                else
                {
                    return new AbpLoginResult(user, await CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie));
                }
            }
        }
            

        private async Task<bool> TryLoginFromExternalAuthenticationSources(string userNameOrEmailAddress, string plainPassword, TTenant tenant, bool loginInHost)
        {
            if (!_userManagementConfig.ExternalAuthenticationSources.Any())
                {
                return false;
                }
            if (tenant == null && !loginInHost)
            {
                return false;
            }
            foreach (var sourceType in _userManagementConfig.ExternalAuthenticationSources)
            {
                using (var source = _iocResolver.ResolveAsDisposable<IExternalAuthenticationSource<TTenant, TUser, TUserTenant>>(sourceType))
                {
                   
                    if (await source.Object.TryAuthenticateAsync(userNameOrEmailAddress, plainPassword, tenant, loginInHost))
                    {
                        var tenantId = tenant == null ? (int?) null : tenant.Id;
                       // TUser user;
                        //if (tenant == null && !loginInHost)
                        //{
                        //    user = await AbpStore.FindByNameOrEmailAsync(userNameOrEmailAddress); //zoek gebruiker
                        //    if (user.LastTenantId.HasValue ) // zet de tenantId op de laatste tenant
                        //    {
                        //        tenantId = user.LastTenantId;
                        //        if (await source.Object.TryAuthenticateAsync(userNameOrEmailAddress, plainPassword, tenant, loginInHost))
                        //        {

                        //        }
                        //        else
                        //        {

                        //        }

                        //        user = await AbpStore.FindByNameOrEmailAsync(tenantId, userNameOrEmailAddress);
                        //    }
                        //    else
                        //    {

                        //    }
                        //}
                        //else
                        //{
                        var  user = await AbpStore.FindByNameOrEmailAsync(tenantId, userNameOrEmailAddress);
                        //}
                        
                        if (user == null)
                        {
                            user = await source.Object.CreateUserAsync(userNameOrEmailAddress, tenant);

                            if (tenantId.HasValue)
                                user.LastTenantId = tenantId.Value;

                            user.AuthenticationSource = source.Object.Name;
                            user.Password = new PasswordHasher().HashPassword(Guid.NewGuid().ToString("N").Left(16)); //Setting a random password since it will not be used

                            user.Roles = new List<UserRole>();
                            foreach (var defaultRole in RoleManager.Roles.Where(r => r.TenantId == tenantId && r.IsDefault).ToList())
                            {
                                user.Roles.Add(new UserRole { RoleId = defaultRole.Id });
                            }

                            await Store.CreateAsync(user);
                        }
                        else
                        {
                            await source.Object.UpdateUserAsync(user, tenant);

                            user.AuthenticationSource = source.Object.Name;
                            await Store.UpdateAsync(user);
                        }

                        await _unitOfWorkManager.Current.SaveChangesAsync();

                    }
                }
            }
            return false;
         
            }

        /// <summary>
        /// Gets a user by given id.
        /// Throws exception if no user found with given id.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>User</returns>
        /// <exception cref="AbpException">Throws exception if no user found with given id</exception>
        public virtual async Task<TUser> GetUserByIdAsync(long userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new AbpException("There is no user with id: " + userId);
            }

            return user;
        }

        public async Task<ClaimsIdentity> CreateIdentityAsyncWithTenant(TUser user, string authenticationType, int? tenantId)
        {            
            var userTenant = await UserTenantManager.FindByUserIdAndTenantIdAsync(user.Id, tenantId);
            if (userTenant == null && userTenant.IsDeleted)
            {
                return null;
            }
            var identity = await base.CreateIdentityAsync(user, authenticationType);                
            identity.AddClaim(new Claim(AbpClaimTypes.TenantId, userTenant.Id.ToString(CultureInfo.InvariantCulture)));

            return identity;
        }

        public async override Task<ClaimsIdentity> CreateIdentityAsync(TUser user, string authenticationType)
        {
          return await base.CreateIdentityAsync(user, authenticationType);          
        }

        public async Task<UpdateTenantResult> UpdateTenantAsync(TUser user, int tenantId, ClaimsPrincipal claimsPrincipal)
        {
            var identity = claimsPrincipal.Identity as ClaimsIdentity;
            var claimTenant = identity.Claims.FirstOrDefault(c => c.Type == AbpClaimTypes.TenantId);

            if (tenantId > 0)
            {
                var userTenant = user.UserInTenants.FirstOrDefault(ut => ut.TenantId == tenantId);
                if (userTenant == null)
                {
                    return new UpdateTenantResult(AbpUpdateTenantResultType.InvalidTenancyId);
                }
                else
                {
                    if (!userTenant.IsActive || userTenant.IsDeleted)
                        return new UpdateTenantResult(AbpUpdateTenantResultType.TenantIsNotActive);
                }               
            }
            if (claimTenant != null)
            {
                identity.RemoveClaim(claimTenant);                
            }
            identity.AddClaim(new Claim(AbpClaimTypes.TenantId, tenantId.ToString(CultureInfo.InvariantCulture)));

            return new UpdateTenantResult(user,identity);
        }

        public async override Task<IdentityResult> UpdateAsync(TUser user)
        {
            var result = await CheckDuplicateUsernameOrEmailAddressAsync(user.Id, user.UserName, user.EmailAddress);
            if (!result.Succeeded)
            {
                return result;
            }

            var oldUserName = (await GetUserByIdAsync(user.Id)).UserName;
            if (oldUserName == AbpUser<TTenant, TUser, TUserTenant>.AdminUserName && user.UserName != AbpUser<TTenant, TUser, TUserTenant>.AdminUserName)
            {
                return AbpIdentityResult.Failed(string.Format(L("CanNotRenameAdminUser"), AbpUser<TTenant, TUser,TUserTenant>.AdminUserName));
            }

            return await base.UpdateAsync(user);
        }

        public async override Task<IdentityResult> DeleteAsync(TUser user)
        {
            if (user.UserName == AbpUser<TTenant, TUser,TUserTenant>.AdminUserName)
            {
                return AbpIdentityResult.Failed(string.Format(L("CanNotDeleteAdminUser"), AbpUser<TTenant, TUser, TUserTenant>.AdminUserName));
            }

            return await base.DeleteAsync(user);
        }

        public virtual async Task<IdentityResult> ChangePasswordAsync(TUser user, string newPassword)
        {
            var result = await PasswordValidator.ValidateAsync(newPassword);
            if (!result.Succeeded)
            {
                return result;
            }

            await AbpStore.SetPasswordHashAsync(user, PasswordHasher.HashPassword(newPassword));
            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> CheckDuplicateUsernameOrEmailAddressAsync(long? expectedUserId, string userName, string emailAddress)
        {
            var user = (await FindByNameAsync(userName));
            if (user != null && user.Id != expectedUserId)
            {
                return AbpIdentityResult.Failed(string.Format(L("Identity.DuplicateName"), userName));
            }

            user = (await FindByEmailAsync(emailAddress));
            if (user != null && user.Id != expectedUserId)
            {
                return AbpIdentityResult.Failed(string.Format(L("Identity.DuplicateEmail"), emailAddress));
            }

            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> SetRoles(TUser user, string[] roleNames)
        {
            //Remove from removed roles
            foreach (var userRole in user.Roles.ToList())
            {
                var role = await RoleManager.FindByIdAsync(userRole.RoleId);
                if (roleNames.All(roleName => role.Name != roleName))
                {
                    var result = await RemoveFromRoleAsync(user.Id, role.Name);
                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
            }

            //Add to added roles
            foreach (var roleName in roleNames)
            {
                var role = await RoleManager.GetRoleByNameAsync(roleName);
                if (user.Roles.All(ur => ur.RoleId != role.Id))
                {
                    var result = await AddToRoleAsync(user.Id, roleName);
                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
            }

            return IdentityResult.Success;
        }

        private async Task<bool> IsEmailConfirmationRequiredForLoginAsync(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                return await SettingManager.GetSettingValueForTenantAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin, tenantId.Value);
            }

            return await SettingManager.GetSettingValueForApplicationAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);
        }

        private async Task<TTenant> GetDefaultTenantAsync()
        {
            var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.TenancyName == AbpTenant<TTenant, TUser, TUserTenant>.DefaultTenantName);
            if (tenant == null)
            {
                throw new AbpException("There should be a 'Default' tenant if multi-tenancy is disabled!");
            }

            return tenant;
        }

        private string L(string name)
        {
            return LocalizationManager.GetString(AbpZeroConsts.LocalizationSourceName, name);
        }

        public class AbpLoginResult
        {
            public AbpLoginResultType Result { get; private set; }

            public TUser User { get; private set; }

            public ClaimsIdentity Identity { get; private set; }

            public AbpLoginResult(AbpLoginResultType result)
            {
                Result = result;
            }


            public AbpLoginResult(TUser user)
                : this(AbpLoginResultType.UserNeedsToChooseTenant)
            {
                User = user;
            }

            public AbpLoginResult(TUser user, ClaimsIdentity identity)
                : this(AbpLoginResultType.Success)
            {
                User = user;
                Identity = identity;
            }
        }

        public class UpdateTenantResult
        {
            public AbpUpdateTenantResultType Result { get; private set; }
            public TUser User { get; private set; }
            public ClaimsIdentity Identity { get; private set; }
              public UpdateTenantResult(AbpUpdateTenantResultType result)
            {
                Result = result;
            }

              public UpdateTenantResult(TUser user, ClaimsIdentity identity)
                : this(AbpUpdateTenantResultType.Success)
            {
                User = user;
                Identity = identity;
            }
        }
    }
}