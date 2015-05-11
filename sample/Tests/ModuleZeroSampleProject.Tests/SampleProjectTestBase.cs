using System;
using System.Data.Common;
using System.Linq;
using Abp.Collections;
using Abp.Modules;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using ModuleZeroSampleProject.EntityFramework;
using ModuleZeroSampleProject.Migrations.Data;

namespace ModuleZeroSampleProject.Tests
{
    public abstract class SampleProjectTestBase : AbpIntegratedTestBase
    {
        protected SampleProjectTestBase()
        {
            //Fake DbConnection using Effort!
            LocalIocManager.IocContainer.Register(
                Component.For<DbConnection>()
                    .UsingFactoryMethod(Effort.DbConnectionFactory.CreateTransient)
                    .LifestyleSingleton()
                );

            LocalIocManager.IocContainer.Register(
                Component.For<DbConnection>()
                    .UsingFactoryMethod(Effort.DbConnectionFactory.CreateTransient)
                    .Named("DbConnection.App")
                    .LifestyleSingleton()
                );
                        
            //LocalIocManager.IocContainer.Release(LocalIocManager.IocContainer.Resolve<ModuleZeroSampleProjectDbContext>());
            //register moduleZeroTestDbContext again with named connection...
            LocalIocManager.IocContainer.Register(
            Component.For<ModuleZeroSampleProjectDbContext>()
            .Named("ModuleZeroTestDbContext")
            .DependsOn(Dependency.OnComponent(
    typeof(DbConnection),
    "DbConnection.App"))                    
                    .LifestyleTransient().IsDefault()
    );


            //Creating initial data
            UsingGlobalDbContext(context => new InitialDataBuilder().Build(context));
            UsingAppDbContext(context => new InitialDataBuilder().BuildAppContext(context));
            
            AbpSession.TenantId = 1;
        }

        protected override void AddModules(ITypeList<AbpModule> modules)
        {
            base.AddModules(modules);
            modules.Add<ModuleZeroSampleProjectApplicationModule>();
            modules.Add<ModuleZeroSampleProjectDataModule>();
        }

        public void UsingGlobalDbContext(Action<GlobalDbContext> action)
        {
            using (var context = LocalIocManager.Resolve<GlobalDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }

        public T UsingGlobalDbContext<T>(Func<GlobalDbContext, T> func)
        {
            T result;

            using (var context = LocalIocManager.Resolve<GlobalDbContext>())
            {
                result = func(context);
                context.SaveChanges();
            }

            return result;
        }

        public void UsingAppDbContext(Action<ModuleZeroSampleProjectDbContext> action)
        {
            using (var context = LocalIocManager.Resolve<ModuleZeroSampleProjectDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }

        public T UsingAppDbContext<T>(Func<ModuleZeroSampleProjectDbContext, T> func)
        {
            T result;

            using (var context = LocalIocManager.Resolve<ModuleZeroSampleProjectDbContext>())
            {
                result = func(context);
                context.SaveChanges();
            }

            return result;
        }

    }
}
