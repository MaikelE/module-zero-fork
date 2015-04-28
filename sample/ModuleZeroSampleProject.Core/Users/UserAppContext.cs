using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleZeroSampleProject.Users
{
    public class User_Ref : FullAuditedEntity<long, User_Ref>
    {
        public override string ToString()
        {
            return string.Format("[User_Ref {0}] {1}", Id);
        }
    }
}
