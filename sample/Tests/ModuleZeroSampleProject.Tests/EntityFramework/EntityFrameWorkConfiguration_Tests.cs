using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ModuleZeroSampleProject.EntityFramework;
using Shouldly;

namespace ModuleZeroSampleProject.Tests.EntityFramework
{
    public class EntityFrameWorkConfiguration_Tests : SampleProjectTestBase
    {
        [Fact]
        public void ShouldHaveDifferentConnectionStrings()
        {
            string connection_1 = null;
            string connection_2 = null;
            var globalDbContext = LocalIocManager.Resolve<GlobalDbContext>();
            var appDbContext = LocalIocManager.Resolve<ModuleZeroSampleProjectDbContext>();
            connection_1 = globalDbContext.Database.Connection.ConnectionString;            
            connection_2 = appDbContext.Database.Connection.ConnectionString;
             
            connection_1.ShouldNotBe(connection_2);
        }
    }
}
