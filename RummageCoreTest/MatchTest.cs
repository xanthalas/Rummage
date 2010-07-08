using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RummageCore;

namespace RummageCoreTest
{
    /// <summary>
    /// Unit test class for testing the Match class.
    /// </summary>
    [TestClass]
    public class MatchTest
    {
        public MatchTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestConstructor()
        {

            Match m = new Match("findme", "you found findme in this line", 1, "testfile");

            Assert.AreEqual("findme", m.MatchString);
            Assert.AreEqual("you found findme in this line", m.MatchLine);
            Assert.AreEqual(1, m.MatchLineNumber);
            Assert.AreEqual("testfile", m.MatchFile);
        }
    }
}
