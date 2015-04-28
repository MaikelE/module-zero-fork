using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abp.Uow
{
    public static class ZeroDataFilters
    {
        /// <summary>
        /// "MayHaveOneOrMoreTenants".
        /// Tenant filter to prevent getting data that is
        /// not belong to current tenant.
        /// </summary>
        public const string MayHaveOneOrMoreTenants = "MayHaveOneOrMoreTenants";

        /// <summary>
        /// "MayHaveOneOrMoreTenants".
        /// Tenant filter to prevent getting data that is
        /// not belong to current tenant.
        /// </summary>
        public const string MustHaveOneOrMoreTenants = "MustHaveOneOrMoreTenants";

        /// <summary>
        /// Standard parameters of ABP.
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// "UserTenants".
            /// </summary>
            public const string UserTenants = "userTenants";
        }
    }
}
