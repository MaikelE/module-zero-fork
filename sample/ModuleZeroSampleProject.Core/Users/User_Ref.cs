using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleZeroSampleProject.Users
{
    public class User_Ref<TUser> : FullAuditedEntity<long, User_Ref>
        where TUser : AbpUser 
    {
        public override string ToString()
        {
            return string.Format("[User_Ref {0}] {1}", Id);
        }

        public User_Ref(TUser)
        {

        }
    }
}
