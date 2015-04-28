using Abp.Domain.Entities;
using Abp.EntityFramework;
using Abp.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleZeroSampleProject.EntityFramework.Repositories
{
    public abstract class GlobalRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<GlobalDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected GlobalRepositoryBase(IDbContextProvider<GlobalDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }

    public abstract class GlobalRepositoryBase<TEntity> : GlobalRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected GlobalRepositoryBase(IDbContextProvider<GlobalDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}
