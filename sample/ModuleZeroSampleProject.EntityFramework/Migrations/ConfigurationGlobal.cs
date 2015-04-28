using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModuleZeroSampleProject.EntityFramework;
using ModuleZeroSampleProject.Migrations.Data;

namespace ModuleZeroSampleProject.Migrations.GlobalDb
{
    using System.Data.Entity.Migrations;

    internal sealed class ConfigurationGlobal : DbMigrationsConfiguration<GlobalDbContext>
    {
        public ConfigurationGlobal()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Global";
        }

        protected override void Seed(GlobalDbContext context)
        {
            new InitialDataBuilder().Build(context);
        }
    }
}
