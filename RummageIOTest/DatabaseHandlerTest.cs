using NUnit.Framework;
using RummageIO;
using System.Data;

namespace RummageIOTest
{
    /// <summary>
    /// Unit test class for testing the Firebird Database Handler.
    /// </summary>
    /// <remarks>
    /// To run successfully this test relies upon a database being present in the output directory with the contents loaded from the Initialise.sql file
    /// </remarks>
    [TestFixture]
    public class DatabaseHandlerTest
    {
        private const string CONNECTION_STRING =
            @"Server=localhost;User=xanthalas;Password=rummage;Database={0}\RUMMAGE.FDB";

        private const string TEST_DATASET_READER_COMMAND = @"select * from container_type";
        private const string TEST_SCALAR_READER_COMMAND = @"select count(*) from container_type";
        private const string GET_MAX_CONTAINER_ID = @"select coalesce(max(container_id), 0) from containers";

        private string constructedConnectionString;


        [SetUp]
        private void setup()
        {
            constructedConnectionString = string.Format(CONNECTION_STRING, System.Environment.CurrentDirectory);
        }

        [Test]
        public void TestConstructor()
        {
            DatabaseHandler handler = DatabaseHandler.GetHandler("Firebird");
            handler.OpenConnection(constructedConnectionString);
            
            Assert.AreEqual("Open", handler.Status);

            handler.CloseConnection();
            Assert.AreEqual("Closed", handler.Status);
        }

        [Test]
        public void TestDataSetReader()
        {
            DatabaseHandler handler = DatabaseHandler.GetHandler("Firebird");
            handler.OpenConnection(constructedConnectionString);

            Assert.AreEqual("Open", handler.Status);

            DataSet ds = handler.Execute(handler.NewCommand(TEST_DATASET_READER_COMMAND));

            handler.CloseConnection();
            Assert.AreEqual("Closed", handler.Status);
            
            Assert.AreEqual(1, ds.Tables.Count);
            Assert.AreEqual(2, ds.Tables[0].Rows.Count);
        }

        [Test]
        public void TestScalarReader()
        {
            DatabaseHandler handler = DatabaseHandler.GetHandler("Firebird");
            handler.OpenConnection(constructedConnectionString);

            Assert.AreEqual("Open", handler.Status);

            var result = handler.ExecuteScalar(handler.NewCommand(TEST_SCALAR_READER_COMMAND));

            handler.CloseConnection();
            Assert.AreEqual("Closed", handler.Status);

            Assert.AreEqual("System.Int", result.GetType().ToString().Substring(0, 10));
            Assert.AreEqual(2, (int)result);
        }

        [Test]
        public void TestStoreContainer()
        {
            DatabaseHandler handler = DatabaseHandler.GetHandler("Firebird");
            handler.OpenConnection(constructedConnectionString);

            Assert.AreEqual("Open", handler.Status);

            IDbCommand getmaxCmd = handler.NewCommand(GET_MAX_CONTAINER_ID);

            int currentMax = (int)handler.ExecuteScalar(getmaxCmd);

            //Add the first container
            var idOfAddedContainer = handler.StoreContainer("Container 1", "DIR");
            Assert.AreEqual(1, idOfAddedContainer);

            //Add a second container
            idOfAddedContainer = handler.StoreContainer("Container 2", "DIR");
            Assert.AreEqual(2, idOfAddedContainer);

            //Now add the first again - it should return the id of the first one and not do a second insert
            idOfAddedContainer = handler.StoreContainer("Container 1", "DIR");
            Assert.AreEqual(1, idOfAddedContainer);

            handler.CloseConnection();


        }

    }
}
