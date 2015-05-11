﻿using Abp.Authorization.Roles;
using ModuleZeroSampleProject.MultiTenancy;
using ModuleZeroSampleProject.Users;

namespace ModuleZeroSampleProject.Authorization
{
    public class Role : AbpRole<Tenant, User, UserTenant>
    {
        public Role()
        {

        }

        public Role(int? tenantId, string name, string displayName)
            : base(tenantId, name, displayName)
        {

        }
    }
}