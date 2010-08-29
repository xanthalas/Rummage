using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using RummageFilesystem.Domain;
using domain = RummageFilesystem.Domain;
using RummageCore.Domain;

namespace RummageFilesystem.Repositories
{
    public class ISearchRequestRepository : domain.ISearchRequestFilesystemRepository
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



    }
}
