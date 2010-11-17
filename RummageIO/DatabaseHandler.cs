using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using FirebirdSql;
using FirebirdSql.Data;
using FirebirdSql.Data.FirebirdClient;


namespace RummageIO
{
    public abstract class DatabaseHandler
    {
        /// <summary>
        /// Gets the current status of this connection
        /// </summary>
        public abstract string Status { get; protected set; }

        #region SQL-specific methods (as opposed to Rummage-specific methods)

        /// <summary>
        /// Opens a connection to a database
        /// </summary>
        /// <param name="connection">Connection string for database</param>
        /// <returns>True if the open was successful, otherwise False</returns>
        public abstract bool OpenConnection(string connection);

        /// <summary>
        /// Close the connection to the database
        /// </summary>
        public abstract void CloseConnection();

        /// <summary>
        /// Execute the command and return a DataSet containing the results
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>DataSet containing the results of the execution</returns>
        public abstract DataSet Execute(IDbCommand command);

        /// <summary>
        /// Execute the command which returns a single scalar object.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>Object holding the result of the execution</returns>
        public abstract object ExecuteScalar(IDbCommand command);

        /// <summary>
        /// Execute the command which returns no result
        /// </summary>
        /// <param name="command">Command to execute</param>
        public abstract void ExecuteNoResult(IDbCommand command);

        /// <summary>
        /// Returns a new command object initialised with the command text passed in
        /// </summary>
        /// <param name="commandText">Command text for the command</param>
        /// <returns>New command object</returns>
        public abstract IDbCommand NewCommand(string commandText);

        #endregion

        /// <summary>
        /// Pushes a new container to the containers table. If the container is already present it won't store it
        /// but will return the id of the existing instance
        /// </summary>
        /// <param name="container">The container to store</param>
        /// <param name="type">The container type</param>
        /// <returns>The id of the container in the database</returns>
        public abstract int StoreContainer(string container, string type);

        #region Static methods
        /// <summary>
        /// Gets the appropriate database handler for the database type passed in.
        /// </summary>
        /// <param name="databaseEngine">String identifying the database to use</param>
        /// <returns>Database handler of the appropriate type</returns>
        public static DatabaseHandler GetHandler(string databaseEngine)
        {
            if (databaseEngine == "Firebird")
            {
                return new DatabaseHandlerFirebird();
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
