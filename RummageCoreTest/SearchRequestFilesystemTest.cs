using RummageCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RummageCoreTest
{
    
    
    /// <summary>
    ///This is a test class for SearchRequestFilesystemTest and is intended
    ///to contain all SearchRequestFilesystemTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SearchRequestFilesystemTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Prepare
        ///</summary>
        [TestMethod()]
        public void PrepareTestNoParms()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
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
        [TestMethod()]
        public void PrepareTestDirectoryExclude()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.ExcludeDirectoryStrings.Add("sub.*");

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(4, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\testfile2", srf.URL[2]);

        }

        /// <summary>
        ///A test for Prepare with an include file spec
        ///</summary>
        [TestMethod()]
        public void PrepareTestFileInclude()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeFileStrings.Add(".*test.*");

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(4, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.URL[2]);

        }

        /// <summary>
        ///A test for Prepare with a file include and a file exclude spec
        ///</summary>
        [TestMethod()]
        public void PrepareTestFileIncludeAndExclude()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeFileStrings.Add(".*test.*");
            srf.ExcludeFileStrings.Add("2$");

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
        [TestMethod()]
        public void PrepareTestFileIncludeAndExclude2()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeFileStrings.Add(".*test.*");
            srf.ExcludeFileStrings.Add(@"\d$");

            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(0, srf.URL.Count);

        }

        /// <summary>
        ///Find the Simpsons file only.
        ///</summary>
        [TestMethod()]
        public void PrepareTestFindSimpsonsFile()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeFileStrings.Add(@"simp.*\.txt");


            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(1, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.URL[0]);
        }

        /// <summary>
        ///Find the Simpsons file only.
        ///</summary>
        [TestMethod()]
        public void PrepareTestFindSimpsonsAndFileEndingIn3()
        {
            SearchRequestFilesystem srf = new SearchRequestFilesystem();
            srf.SearchDirectories.Add(@"D:\code\Rummage\testdata");
            srf.SearchStrings.Add("a");
            srf.IncludeFileStrings.Add(@"simp.*\.txt");
            srf.IncludeFileStrings.Add(@"3$");


            bool actual = srf.Prepare();
            Assert.AreEqual(true, actual);
            Assert.AreEqual(2, srf.URL.Count);
            Assert.AreEqual(@"D:\code\Rummage\testdata\seconddir\simpsons.txt", srf.URL[0]);
            Assert.AreEqual(@"D:\code\Rummage\testdata\subfolder1\testfile3", srf.URL[1]);
        }

    }
}
