using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.NHibernate.EntityMappings;

namespace Abp.Zero.NHibernate.EntityMappings
{
    public class UserMap<TTenant, TUser, TUserTenant> : EntityMap<TUser, long>
        where TUser : AbpUser<TTenant, TUser, TUserTenant>
        where TTenant : AbpTenant<TTenant, TUser, TUserTenant>
        where TUserTenant : AbpUserTenant<TTenant, TUser, TUserTenant>
    {
        protected UserMap()
            : base("AbpUsers")
        {
           
            Map(x => x.UserName);
            Map(x => x.Name);
            Map(x => x.Surname);
            Map(x => x.EmailAddress);
            Map(x => x.IsEmailConfirmed);
            Map(x => x.EmailConfirmationCode);
            Map(x => x.Password);
            Map(x => x.PasswordResetCode);
            Map(x => x.LastLoginTime);
            Map(x => x.IsDeleted);
            Map(x => x.DeleterUserId);
            Map(x => x.DeletionTime);

            this.MapFullAudited();

            Polymorphism.Explicit();
        }
    }
}
