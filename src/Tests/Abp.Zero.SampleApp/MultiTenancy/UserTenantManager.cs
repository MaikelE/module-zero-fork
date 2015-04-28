using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp.Zero.SampleApp.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.Zero.SampleApp.MultiTenancy
{
    public class UserTenantManager : AbpUserTenantManager<Tenant,  User,UserTenant>
    {
        public UserTenantManager(IRepository<UserTenant> userTenantRepository
            )
            : base(userTenantRepository)
        {
        }
    }
}
