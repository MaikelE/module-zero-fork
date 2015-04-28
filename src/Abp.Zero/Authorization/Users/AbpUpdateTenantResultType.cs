using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.Authorization.Users
{
    public enum AbpUpdateTenantResultType
    {
        Success = 1,
        InvalidTenancyId,
        TenantIsNotActive
    }
}
