using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using RummageCore.Repositories;
using RummageFilesystem.Domain;
using RummageCore.Domain;

namespace RummageFilesystem.Repositories
{
    public class SearchRequestFilesystemRepository : ISearchRequestRepository
    {
        public void Add(ISearchRequest request)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Save(request);
                    transaction.Commit();
                }
            }
        }

        public ISearchRequest GetById(int requestId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.Get<SearchRequestFilesystem>(requestId);
        }

        public void Update(ISearchRequest request)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(request);
                transaction.Commit();
            }
        }

        public void Remove(ISearchRequest request)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(request);
                transaction.Commit();
            }
        }

        public ISearchRequest GetByName(string name)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ISearchRequest product = session
                    .CreateCriteria(typeof(ISearchRequest))
                    .Add(Restrictions.Eq("Name", name))
                    .UniqueResult<ISearchRequest>();
                return product;
            }
        }

        public ICollection<ISearchRequest> GetBySearchType(string searchType)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var products = session
                    .CreateCriteria(typeof(ISearchRequest))
                    .Add(Restrictions.Eq("SearchType", searchType))
                    .List<ISearchRequest>();
                return products;
            }
        }

        public ICollection<ISearchRequest> GetByMatchingName(string matchString)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var products = session
                    .CreateCriteria(typeof(ISearchRequest))
                    .Add(Restrictions.InsensitiveLike("Name", matchString))
                    .List<ISearchRequest>();
                return products;
            }
        }
    }
}
