using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleZeroSampleProject.Users
{
    public class User_Ref<TUser> : FullAuditedEntity<long, User_Ref<TUser>>
        where TUser : User
    {
        public override string ToString()
        {
            return string.Format("[User_Ref {0}] {1}", Id);
        }

        public User_Ref(TUser user)
        {
            this.Id = user.Id;
        }
    }

    public class User_Ref : User_Ref<User>
    {
        public User_Ref(User user)
            : base(user)
        {

        }
    }
}
