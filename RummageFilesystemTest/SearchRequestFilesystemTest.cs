using RummageCore;
using RummageCore.Domain;
using RummageFilesystem.Domain;
using NUnit.Framework;

namespace RummageTest
{
    /// <summary>
    ///This is a test class for SearchRequestFilesystemTest and is intended
    ///to contain all SearchRequestFilesystemTest Unit Tests
    ///</summary>
    [TestFixture]
    public class SearchRequestFilesystemTest
    {

        /// <summary>
        ///A test for Prepare
        ///</summary>
        [Test]
        public void PersistTest()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.Recurse = true;
            srf.SearchBinaries = true;

            int actual = srf.Prepare();
            Assert.AreEqual(6, actual);
            Assert.AreEqual(6, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile1", srf.Urls[1]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.Urls[3]);

        }

        /// <summary>
        ///A test for Prepare
        ///</summary>
        [Test]
        public void NoRecurseTest()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.SearchBinaries = true;
            srf.Recurse = false;

            int actual = srf.Prepare();
            Assert.AreEqual(3, actual);
            Assert.AreEqual(3, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile1", srf.Urls[1]);

        }

        /// <summary>
        ///A test for Prepare with a directory exclusion spec
        ///</summary>
        [Test]
        public void PrepareTestDirectoryExclude()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.ExcludeContainerStrings.Add("sub.*");
            srf.SearchBinaries = true;
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(4, actual);
            Assert.AreEqual(4, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile2", srf.Urls[2]);

        }

        /// <summary>
        ///A test for Prepare with a directory inclusion spec
        ///</summary>
        [Test]
        public void PrepareTestDirectoryInclude()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("at");
            srf.IncludeContainerStrings.Add("sub.*");
            srf.SearchBinaries = false;
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(4, actual);
            Assert.AreEqual(4, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile2", srf.Urls[1]);

        }

        /// <summary>
        ///A test for Prepare with an include file spec
        ///</summary>
        [Test]
        public void PrepareTestFileInclude()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeItemStrings.Add(".*test.*");
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(4, actual);
            Assert.AreEqual(4, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.Urls[2]);

        }

        /// <summary>
        ///A test for Prepare with a file include and a file exclude spec
        ///</summary>
        [Test]
        public void PrepareTestFileIncludeAndExclude()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeItemStrings.Add(".*test.*");
            srf.ExcludeItemStrings.Add("2$");
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(3, actual);
            Assert.AreEqual(3, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile1", srf.Urls[0]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.Urls[1]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile4", srf.Urls[2]);
        }


        /// <summary>
        ///Another test for Prepare with a file include and a file exclude spec.
        ///</summary>
        [Test]
        public void PrepareTestFileIncludeAndExclude2()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeItemStrings.Add(".*test.*");
            srf.ExcludeItemStrings.Add(@"\d$");
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(0, actual);
            Assert.AreEqual(0, srf.Urls.Count);

        }

        /// <summary>
        ///Find the Simpsons file only.
        ///</summary>
        [Test]
        public void PrepareTestFindSimpsonsFile()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeItemStrings.Add(@"simp.*\.txt");
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(1, actual);
            Assert.AreEqual(1, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.Urls[0]);
        }

        /// <summary>
        ///Find the Simpsons file only.
        ///</summary>
        [Test]
        public void PrepareTestFindSimpsonsAndFileEndingIn3()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeItemStrings.Add(@"simp.*\.txt");
            srf.IncludeItemStrings.Add(@"3$");
            srf.Recurse = true;

            int actual = srf.Prepare();
            Assert.AreEqual(2, actual);
            Assert.AreEqual(2, srf.Urls.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.Urls[0]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.Urls[1]);
        }


        /// <summary>
        ///Perform a simple search
        ///</summary>
        [Test]
        public void SimpleSearch()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("Bart");
            srf.IncludeItemStrings.Add(@"simp.*\.txt");
            srf.Recurse = true;
            srf.Prepare();

            ISearch search = new SearchFilesystem();
            search.SearchRequest = srf;
            search.Search(true);
            Assert.AreEqual(1, search.Matches.Count);
        }

        /// <summary>
        ///Perform a simple search
        ///</summary>
        [Test]
        public void SearchMultipleFilesForTwoRegexes()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("Krusty");
            srf.SearchStrings.Add("^T");
            srf.Recurse = true;
            srf.Prepare();

            ISearch search = new SearchFilesystem();
            search.SearchRequest = srf;
            search.Search(true);
            Assert.AreEqual(3, search.Matches.Count);
        }

        /// <summary>
        ///Perform a simple search
        ///</summary>
        [Test]
        public void SearchMultipleFilesCaseSensitive()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("brown");
            srf.CaseSensitive = true;
            srf.Recurse = true;
            srf.Prepare();

            ISearch search = new SearchFilesystem();
            search.SearchRequest = srf;
            search.Search(true);
            Assert.AreEqual(2, search.Matches.Count);

            //Now let's redo the search but case insensitive
            srf.CaseSensitive = false;
            search.SearchRequest = srf;
            search.Search(true);
            Assert.AreEqual(3, search.Matches.Count);

        }


        /// <summary>
        ///Perform a simple search
        ///</summary>
        [Test]
        public void BinaryTest()
        {
            //First perform a non-binaries search
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("Microsoft");
            srf.SearchBinaries = false;
            srf.Recurse = true;
            srf.Prepare();

            ISearch search = new SearchFilesystem();
            search.SearchRequest = srf;
            search.Search(true);
            Assert.AreEqual(0, search.Matches.Count);

            //Now include binaries in the search
            ISearchRequest srf2 = new SearchRequestFilesystem();
            srf2.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf2.SearchStrings.Add("Microsoft");
            srf2.SearchBinaries = true;
            srf.Recurse = true;
            srf2.Prepare();

            ISearch search2 = new SearchFilesystem();
            search2.SearchRequest = srf2;
            search2.Search(true);
            Assert.AreEqual(1, search2.Matches.Count);
        }
    }
}