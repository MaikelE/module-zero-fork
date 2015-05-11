using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.MultiTenancy
{
    [Table("AbpUsersTenants")]
    public class AbpUserTenant<TTenant, TUser, TUserTenant> : FullAuditedEntity<int, TUser>,  IPassivable, IHasUserId
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUser : AbpUser<TTenant, TUser,TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        [ForeignKey("UserId")]
        public virtual TUser User { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("TenantId")]
        public virtual TTenant Tenant { get; set; }

        public virtual int? TenantId { get; set; }

        public virtual bool IsActive { get; set; }

        public AbpUserTenant()
        {
            IsActive = true;
        }

    }
}
