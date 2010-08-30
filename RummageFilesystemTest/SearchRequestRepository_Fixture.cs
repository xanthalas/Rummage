using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using RummageCore.Domain;
using NUnit.Framework;
using RummageCore.Repositories;
using RummageFilesystem.Domain;
using RummageFilesystem.Repositories;
using System.Collections;

namespace RummageTest
{

    /* Taken from "Your first NHibernate based application" from NHForge.org
     * http://nhforge.org/wikis/howtonh/your-first-nhibernate-based-application.aspx
     */
    [TestFixture]
    public class SearchRequestRepository_Fixture
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
                     new SearchRequestFilesystem { Name = "Request the third" },
                     new SearchRequestFilesystem { Name = "Fourth request", SearchType = "ANEWTYPE"}
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
            ISearchRequest request = new SearchRequestFilesystem {Name = "Added by Can_add_new_SearchRequest"};

            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();

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
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            var fromDb = repository.GetById(_requests[2].SearchRequestId);
            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_requests[2].SearchRequestId, fromDb.SearchRequestId);
        }

        [Test]
        public void Can_delete_SearchRequest()
        {
            var request = _requests[0];
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            repository.Remove(request);

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = repository.GetById(request.SearchRequestId);
                Assert.IsNull(fromDb);
            }
        }

        [Test]
        public void Can_update_existing_SearchRequest()
        {
            var request = _requests[1];
            request.Name = "+++ Second request - updated";
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            repository.Update(request);

            // use session to try to load the request
            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = repository.GetById(request.SearchRequestId);
                Assert.AreEqual(request.Name, fromDb.Name);
            }
        }

        [Test]
        public void Can_get_existing_SearchRequest_by_name()
        {
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            var fromDb = repository.GetByName(_requests[2].Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_requests[2], fromDb);
            Assert.AreEqual(_requests[2].SearchRequestId, fromDb.SearchRequestId);
        }

        [Test]
        public void Can_get_existing_SearchRequests_by_type()
        {
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            var fromDb = repository.GetBySearchType("FILESYSTEM");

            Assert.AreEqual(3, fromDb.Count);

            fromDb = repository.GetBySearchType("ANEWTYPE");

            Assert.AreEqual(1, fromDb.Count);
        }


        [Test]
        public void Can_get_existing_SearchRequests_by_matching_name()
        {
            ISearchRequestRepository repository = new SearchRequestFilesystemRepository();
            var fromDb = repository.GetByMatchingName("%th%");

            Assert.AreEqual(2, fromDb.Count);
        }


    }
}