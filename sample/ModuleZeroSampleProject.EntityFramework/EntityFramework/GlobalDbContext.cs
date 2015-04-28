using Abp.Zero.EntityFramework;
using ModuleZeroSampleProject.Authorization;
using ModuleZeroSampleProject.MultiTenancy;
using ModuleZeroSampleProject.Users;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleZeroSampleProject.EntityFramework
{
    public class GlobalDbContext : AbpZeroDbContext<Tenant, Role, User, UserTenant>
    {
        // /* NOTE: 
        // *   Setting "Default" to base class helps us when working migration commands on Package Manager Console.
        // *   But it may cause problems when working Migrate.exe of EF. If you will apply migrations on command line, do not
        // *   pass connection string name to base classes. ABP works either way.
        // */
        public GlobalDbContext()
        {

        }

        /* NOTE:
         *   This constructor is used by ABP to pass connection string defined in ModuleZeroSampleProjectDataModule.PreInitialize.
         *   Notice that, actually you will not directly create an instance of ModuleZeroSampleProjectDbContext since ABP automatically handles it.
         */
        public GlobalDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        public GlobalDbContext(DbConnection connection)
            : base(connection, true)
        {

        }
    }
}
