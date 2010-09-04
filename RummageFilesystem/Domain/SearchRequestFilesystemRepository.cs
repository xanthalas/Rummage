using System;
using RummageCore.Domain;

namespace RummageFilesystem.Domain
{
    public interface ISearchRequestFilesystemRepository
    {
        void Add(ISearchRequest request);

        ISearchRequest GetById(int requestId);
    }
}
