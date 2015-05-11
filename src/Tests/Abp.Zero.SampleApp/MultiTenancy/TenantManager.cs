﻿using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Zero.SampleApp.Roles;
using Abp.Zero.SampleApp.Users;

namespace Abp.Zero.SampleApp.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, Role, User, UserTenant>
    {
        public TenantManager(IRepository<Tenant> tenantRepository,
            ISettingManager settingManager)
            : base(tenantRepository,
            settingManager)
        {
        }
    }
}
