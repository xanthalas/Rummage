using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using RummageCore.Domain;
using NUnit.Framework;
using RummageFilesystem.Domain;
using RummageFilesystem.Repositories;

namespace RummageTest
{

    /* Taken from "Your first NHibernate based application" from NHForge.org
     * http://nhforge.org/wikis/howtonh/your-first-nhibernate-based-application.aspx
     */
    [TestFixture]
    public class ProductRepository_Fixture
    {
        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        [SetUp]
        public void SetupContext()
        {

        }


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.AddAssembly(typeof(SearchRequestFilesystem).Assembly).Configure();
            _sessionFactory = _configuration.BuildSessionFactory();

            new SchemaExport(_configuration).Execute(false, true, false);

            CreateInitialData();
        }
        
        private readonly SearchRequestFilesystem[] _requests = new[]
                 {
                     new SearchRequestFilesystem { Name = "Request One" },
                     new SearchRequestFilesystem { Name = "Second request" },
                     new SearchRequestFilesystem { Name = "Request the third" }
                 };

        private void CreateInitialData()
        {

            using (ISession session = _sessionFactory.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var request in _requests)
                    session.Save(request);
                transaction.Commit();
            }
        }
        
        #region Tests for Product (Remove once everything works ok)
        /*
        [Test]
        public void Can_add_new_product()
        {
            var product = new Product { Name = "Apple", Category = "Fruits" };
            IProductRepository repository = new ProductRepository();
            repository.Add(product);

            // use session to try to load the product
            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                // Test that the product was successfully inserted
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(product, fromDb);
                Assert.AreEqual(product.Name, fromDb.Name);
                Assert.AreEqual(product.Category, fromDb.Category);
            }
        }

        [Test]
        public void Can_update_existing_product()
        {
            var product = _products[0];
            product.Name = "Yellow Pear";
            IProductRepository repository = new ProductRepository();
            repository.Update(product);

            // use session to try to load the product
            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                Assert.AreEqual(product.Name, fromDb.Name);
            }
        }

        [Test]
        public void Can_remove_existing_product()
        {
            var product = _products[0];
            IProductRepository repository = new ProductRepository();
            repository.Remove(product);

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                Assert.IsNull(fromDb);
            }
        }

        [Test]
        public void Can_get_existing_product_by_id()
        {
            IProductRepository repository = new ProductRepository();
            var fromDb = repository.GetById(_products[1].Id);
            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_products[1], fromDb);
            Assert.AreEqual(_products[1].Name, fromDb.Name);
        }

        [Test]
        public void Can_get_existing_product_by_name()
        {
            IProductRepository repository = new ProductRepository();
            var fromDb = repository.GetByName(_products[1].Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_products[1], fromDb);
            Assert.AreEqual(_products[1].Id, fromDb.Id);
        }

        [Test]
        public void Can_get_existing_products_by_category()
        {
            IProductRepository repository = new ProductRepository();
            var fromDb = repository.GetByCategory("Fruits");

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_products[0], fromDb));
            Assert.IsTrue(IsInCollection(_products[1], fromDb));
        }

        private bool IsInCollection(Product product, ICollection<Product> fromDb)
        {
            foreach (var item in fromDb)
                if (product.Id == item.Id)
                    return true;
            return false;
        }
         * */
#endregion
        [Test]
        public void Can_add_new_SearchRequest()
        {
            ISearchRequest request = new SearchRequestFilesystem();
            request.Name = "Added by Can_add_new_SearchRequest";

            ISearchRequestRepository repository = new RummageFilesystem.Repositories.ISearchRequestRepository();

            repository.Add(request);

            int idOfAddedSearchRequest = request.SearchRequestId;

            // use session to try to load the product

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = repository.GetById(idOfAddedSearchRequest);
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(idOfAddedSearchRequest, fromDb.SearchRequestId);
            }
        }

        [Test]
        public void Can_get_existing_SearchRequest_by_id()
        {
            ISearchRequestRepository repository = new RummageFilesystem.Repositories.ISearchRequestRepository();
            var fromDb = repository.GetById(32768);
            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(32768, fromDb.SearchRequestId);
        }


    }
}