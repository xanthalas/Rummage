using System;
using System.Data;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;


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
            FbCommand cmd = new FbCommand("INSERTCONTAINER");
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("containerURL", container.TrimEnd());
            cmd.Parameters.Add("containerTypeCode", "DIR");

            int idOfAddedContainer = (int) this.ExecuteScalar(cmd);

            return idOfAddedContainer;
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
