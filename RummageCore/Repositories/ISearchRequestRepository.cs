using System.Collections.Generic;
using RummageCore.Domain;

namespace RummageCore.Repositories
{
    public interface ISearchRequestRepository
    {
        void Add(ISearchRequest request);

        ISearchRequest GetById(int requestId);

        void Update(ISearchRequest request);

        void Remove(ISearchRequest request);

        ISearchRequest GetByName(string name);

        ICollection<ISearchRequest> GetBySearchType(string searchType);

        ICollection<ISearchRequest> GetByMatchingName(string matchString);
    }
}
