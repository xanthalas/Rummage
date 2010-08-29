using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using RummageFilesystem.Domain;
using NUnit.Framework;

namespace RummageTest
{
    [TestFixture]
    public class GenerateSchema_Fixture
    {
        [Test]
        public void Can_generate_schema()
        {
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(SearchRequestFilesystem).Assembly);

            new SchemaExport(cfg).Execute(false, true, false);
        }
    }
}
