using NUnit.Framework;
using RummageCore;

namespace RummageTest
{
    /// <summary>
    /// Unit test class for testing the Match class.
    /// </summary>
    [TestFixture]
    public class MatchTest
    {
        public MatchTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [Test]
        public void TestConstructor()
        {

            Match m = new Match("findme", "you found findme in this line", 1, "testfile");

            Assert.AreEqual("findme", m.MatchString);
            Assert.AreEqual("you found findme in this line", m.MatchLine);
            Assert.AreEqual(1, m.MatchLineNumber);
            Assert.AreEqual("testfile", m.MatchItem);
        }
    }
}
