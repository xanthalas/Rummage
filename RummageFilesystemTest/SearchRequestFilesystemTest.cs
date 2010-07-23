using RummageCore;
using System;
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
        public void PrepareTestNoParms()
        {
            ISearchRequest srf = new SearchRequestFilesystem();
            srf.SearchContainers.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(6, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile1", srf.URL[1]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.URL[3]);

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

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(4, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile2", srf.URL[2]);

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

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(4, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.URL[2]);

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

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(3, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile1", srf.URL[0]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.URL[1]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile4", srf.URL[2]);
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

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(0, srf.URL.Count);

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


            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(1, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.URL[0]);
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


            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(2, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.URL[0]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.URL[1]);
        }

    }
}