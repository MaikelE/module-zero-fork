using System.Data.Common;
using System.Data.Entity;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.EntityFramework;
using Abp.MultiTenancy;
using EntityFramework.DynamicFilters;
using System.Linq;
using Abp.Domain.Uow;
using Abp.Uow;

namespace Abp.Zero.EntityFramework
{
    /// <summary>
    /// DbContext for ABP zero.
    /// </summary>
    public abstract class AbpZeroDbContext<TTenant, TRole, TUser, TUserTenant> : AbpDbContext
        where TRole : AbpRole<TTenant, TUser,TUserTenant>
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        /// <summary>
        /// Tenants
        /// </summary>
        public virtual IDbSet<TTenant> Tenants { get; set; }

        /// <summary>
        /// Roles.
        /// </summary>
        public virtual IDbSet<TRole> Roles { get; set; }

        /// <summary>
        /// Users.
        /// </summary>
        public virtual IDbSet<TUser> Users { get; set; }

        /// <summary>
        /// User logins.
        /// </summary>
        public virtual IDbSet<UserLogin> UserLogins { get; set; }

        /// <summary>
        /// User roles.
        /// </summary>
        public virtual IDbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Permissions.
        /// </summary>
        public virtual IDbSet<PermissionSetting> Permissions { get; set; }

        /// <summary>
        /// Role permissions.
        /// </summary>
        public virtual IDbSet<RolePermissionSetting> RolePermissions { get; set; }
        
        /// <summary>
        /// User permissions.
        /// </summary>
        public virtual IDbSet<UserPermissionSetting> UserPermissions { get; set; }

        /// <summary>
        /// Settings.
        /// </summary>
        public virtual IDbSet<Setting> Settings { get; set; }

        /// <summary>
        /// Audit logs.
        /// </summary>
        public virtual IDbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Tenants
        /// </summary>
        public virtual IDbSet<TUserTenant> UserTenants { get; set; }

        /// <summary>
        /// Default constructor.
        /// Do not directly instantiate this class. Instead, use dependency injection!
        /// </summary>
        protected AbpZeroDbContext()
        {

        }

        /// <summary>
        /// Constructor with connection string parameter.
        /// </summary>
        /// <param name="nameOrConnectionString">Connection string or a name in connection strings in configuration file</param>
        protected AbpZeroDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        /// <summary>
        /// This constructor can be used for unit tests.
        /// </summary>
        protected AbpZeroDbContext(DbConnection dbConnection, bool contextOwnsConnection)
            : base(dbConnection, contextOwnsConnection)
        {
            
        }

        public override void Initialize()
        {
            base.Initialize();
            Database.Initialize(false);

            //this.SetFilterScopedParameterValue(ZeroDataFilters.MayHaveOneOrMoreTenants, AbpDataFilters.Parameters.TenantId, AbpSession.TenantId ?? 0);
            //this.SetFilterScopedParameterValue(ZeroDataFilters.MustHaveOneOrMoreTenants, AbpDataFilters.Parameters.TenantId, AbpSession.TenantId);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           // modelBuilder.Filter(ZeroDataFilters.MayHaveOneOrMoreTenants, (IMayHaveOneOrMoreTenant<TTenant, TUser> mut, int? tenantId) => mut.UserTenants.Any(ut => ut.TenantId == tenantId), 0);
           // modelBuilder.Filter(ZeroDataFilters.MustHaveOneOrMoreTenants, (IMustHaveOneOrMoreTenant<TTenant, TUser> mut, int tenantId) => mut.UserTenants.Any(ut => ut.TenantId == tenantId), 0);

        }
    }
}
