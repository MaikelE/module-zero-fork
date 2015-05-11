using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abp.Authorization.Users
{
    public interface IHasUserId
    {
        long UserId { get; set; }
    }
}
