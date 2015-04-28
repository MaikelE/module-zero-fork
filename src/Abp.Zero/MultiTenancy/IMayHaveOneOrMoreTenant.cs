using Abp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.MultiTenancy
{
    public interface IMayHaveOneOrMoreTenant<TTenant, TUser,TUserTenant>
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        ICollection<TUserTenant> UserTenants { get; set; }


    }
}
