using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModuleZeroSampleProject.EntityFramework;
using ModuleZeroSampleProject.Migrations.Data;

namespace ModuleZeroSampleProject.Migrations.App
{
    using System.Data.Entity.Migrations;

    internal sealed class ConfigurationModuleZeroSample : DbMigrationsConfiguration<ModuleZeroSampleProjectDbContext>
    {
        public ConfigurationModuleZeroSample()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ModuleZeroSampleProject";
        }

        protected override void Seed(ModuleZeroSampleProjectDbContext context)
        {
            new InitialDataBuilder().BuildAppContext(context);
        }
    }
}
