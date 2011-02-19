using System;
using System.Data;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;
using RummageCore;

namespace RummageIO
{
    public class DatabaseHandlerFirebird : DatabaseHandler
    {
        #region Database command constants

        #endregion
        private string _status = "";

        /// <summary>
        /// Gets the current status of this connection
        /// </summary>
        public override string Status 
        { 
            get 
            {
                return _connection != null ? _connection.State.ToString() : null;
            }
            protected set
            {
                _status = value;
            }
        }

        /// <summary>
        /// Holds the current connection to the database
        /// </summary>
        private FbConnection _connection;

        /// <summary>
        /// Create a new database handler and open a connection to the database passed in
        /// </summary>
        /// <param name="connection">Connection string for the database</param>
        public DatabaseHandlerFirebird()
        {
        }

        /// <summary>
        /// Returns the connection string for the database built from the parameters passed in
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="databasePath"></param>
        /// <returns></returns>
        public override string BuildConnectionString(string server, string user, string password, string databasePath)
        {
            string connectionString = String.Format(@"Server={0};User={1};Password={2};Database={3}", server, user,password, databasePath);

            return connectionString;
        }

        #region SQL-specific methods (as opposed to Rummage-specific methods)

        /// <summary>
        /// Opens a connection to a database
        /// </summary>
        /// <param name="connection">Connection string for database</param>
        /// <returns>True if the open was successful, otherwise False</returns>
        public override bool OpenConnection(string connection)
        {
            _connection = new FbConnection(connection);

            try
            {
                _connection.Open();
                Status = "Open";
            }
            catch (Exception)
            {
                Status = "Connection failed. Connection string: " + connection;
            }

            return true;
        }

        /// <summary>
        /// Close the connection to the database
        /// </summary>
        public override void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                try
                {
                    _connection.Close();
                    Status = "Closed";
                }
                catch (Exception)
                {
                    //Do nothing for now.
                }
            }
        }

        /// <summary>
        /// Execute the command and return a DataSet containing the results
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>DataSet containing the results of the execution</returns>
        public override DataSet Execute(IDbCommand command)
        {
            FbCommand cmd = BuildCmd(command);

            DataSet ds = new DataSet();
            FbDataAdapter da = new FbDataAdapter(cmd);
            da.Fill(ds);

            return ds;
        }

        /// <summary>
        /// Execute the command which returns a single scalar object.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>Object holding the result of the execution</returns>
        public override object ExecuteScalar(IDbCommand command)
        {
            FbCommand cmd = BuildCmd(command);

            var result = cmd.ExecuteScalar();

            return result;
        }

        /// <summary>
        /// Execute the command which returns no result
        /// </summary>
        /// <param name="command">Command to execute</param>
        public override void ExecuteNoResult(IDbCommand command)
        {
            FbCommand cmd = BuildCmd(command);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns a new command object initialised with the command text passed in
        /// </summary>
        /// <param name="commandText">Command text for the command</param>
        /// <returns>New command object</returns>
        public override IDbCommand NewCommand(string commandText)
        {
            return new FbCommand(commandText);
        }

        #endregion

        /// <summary>
        /// Pushes a new container to the containers table. If the container is already present it won't store it
        /// but will return the id of the existing instance
        /// </summary>
        /// <param name="container">The container to store</param>
        /// <param name="type">The container type</param>
        /// <returns>The id of the container in the database</returns>
        public override int StoreContainer(string container, string type)
        {
            FbCommand cmd = new FbCommand("INSERTCONTAINER") { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.Add("containerURL", container);
            cmd.Parameters.Add("containerTypeCode", type);

            int idOfAddedContainer = (int)this.ExecuteScalar(cmd);

            return idOfAddedContainer;
        }

        /// <summary>
        /// Stores a search request.
        /// </summary>
        /// <param name="name">The name of the request</param>
        /// <param name="request">The search request object holding the details of the search</param>
        /// <param name="searchContainerType">The type of container to be searched</param>
        /// <returns>The id of the search request just stored in the database</returns>
        public override int StoreSearchRequest(string name, ISearchRequest request, SearchContainerType searchContainerType)
        {
            string type;

            if (searchContainerType == SearchContainerType.Database)
            {
                type = "SQL";
            }
            else
            {
                type = "DIR";
            }

            FbCommand cmd = new FbCommand("INSERTSEARCHREQUEST") { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.Add("GUID", request.SearchRequestId.ToString());
            cmd.Parameters.Add("NAME", name);
            cmd.Parameters.Add("CASESENSITIVE", request.CaseSensitive);
            cmd.Parameters.Add("SEARCHHIDDEN", request.SearchHidden);
            cmd.Parameters.Add("SEARCHBINARIES", request.SearchBinaries);
            cmd.Parameters.Add("RECURSE", request.Recurse);

            int idOfAddedSearch = (int)this.ExecuteScalar(cmd);

            //Now store the containers against this search
            storeContainers(idOfAddedSearch, request, type);

            return idOfAddedSearch;
        }

        /// <summary>
        /// Store the containers which will be searched by this search request
        /// </summary>
        /// <param name="idOfAddedSearch">The Id of the newly added search request</param>
        /// <param name="request">The search request we are storing</param>
        private void storeContainers(int idOfAddedSearch, ISearchRequest request, string containerType)
        {
            using (FbCommand cmd = new FbCommand("INSERTSEARCHREQUESTCONTAINER") { CommandType = CommandType.StoredProcedure })
            {
                foreach (var container in request.SearchContainers)
                {
                    int containerId = StoreContainer(container, containerType);

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("SEARCHREQUESTID", idOfAddedSearch);
                    cmd.Parameters.Add("SEARCHCONTAINERID", containerId);

                    this.ExecuteNoResult(cmd);
                }
            }

        }


        /// <summary>
        /// Stores a new search term against a search request.
        /// </summary>
        /// <param name="requestId">Id of the search request to store the term against</param>
        /// <param name="searchTerm">The search term to store</param>
        public override void StoreSearchTerm(int requestId, string searchTerm)
        {
            FbCommand cmd = new FbCommand("INSERTSEARCHTERM") { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.Add("searchRequestId", requestId);
            cmd.Parameters.Add("searchTerm", searchTerm);

            this.ExecuteNoResult(cmd);
        }

        #region Private methods
        /// <summary>
        /// Build a Firebird Command object initialised from the IDbCommand object passed in.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private FbCommand BuildCmd(IDbCommand command)
        {
            FbCommand cmd = new FbCommand(command.CommandText)
            {
                CommandType = command.CommandType,
                CommandTimeout = command.CommandTimeout
            };

            foreach (IDbDataParameter parm in command.Parameters)
            {
                cmd.Parameters.Add(new FbParameter(parm.ParameterName, parm.Value));
            }

            cmd.Connection = _connection;

            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new ApplicationException("Execution cancelled. The database is closed");
            }

            return cmd;
        }
        #endregion
    }
}
