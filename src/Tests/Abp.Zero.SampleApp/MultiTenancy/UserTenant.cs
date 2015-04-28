using Abp.MultiTenancy;
using Abp.Zero.SampleApp.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.Zero.SampleApp.MultiTenancy
{
    public class UserTenant : AbpUserTenant<Tenant, User, UserTenant>
    {
        public UserTenant()

        {

        }
    }
}
