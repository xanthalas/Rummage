using System.Data;

namespace RummageCore
{
    /// <summary>
    /// Interface defining how Rummage can converse with a database for storing details of searches.
    /// </summary>
    public interface IDatabaseHandler
    {
        /// <summary>
        /// Gets the current status of this connection
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Opens a connection to a database
        /// </summary>
        /// <param name="connection">Connection string for database</param>
        /// <returns>True if the open was successful, otherwise False</returns>
        bool OpenConnection(string connection);

        /// <summary>
        /// Close the connection to the database
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Execute the command and return a DataSet containing the results
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>DataSet containing the results of the execution</returns>
        DataSet Execute(IDbCommand command);

        /// <summary>
        /// Execute the command which returns a single scalar object.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>Object holding the result of the execution</returns>
        object ExecuteScalar(IDbCommand command);

        /// <summary>
        /// Execute the command which returns no result
        /// </summary>
        /// <param name="command">Command to execute</param>
        void ExecuteNoResult(IDbCommand command);

        /// <summary>
        /// Returns a new command object initialised with the command text passed in
        /// </summary>
        /// <param name="commandText">Command text for the command</param>
        /// <returns>New command object</returns>
        IDbCommand NewCommand(string commandText);

        /// <summary>
        /// Stores a new container in the container table. If the container is already present it won't store it
        /// but will return the id of the existing instance
        /// </summary>
        /// <param name="container">The container to store</param>
        /// <param name="type">The container type</param>
        /// <returns>The id of the container in the database</returns>
        int StoreContainer(string container, string type);

        /// <summary>
        /// Stores a new search request in the search_request table.
        /// </summary>
        /// <param name="requestName">The name of the request</param>
        /// <param name="request">The search request object holding the details of the search</param>
        /// <param name="searchContainerType">The type of container to be searched</param>
        /// <returns>The id of the request just stored in the database</returns>
        int StoreSearchRequest(string requestName, ISearchRequest request, SearchContainerType searchContainerType);

        /// <summary>
        /// Returns the connection string for the database built from the parameters passed in
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="databasePath"></param>
        /// <returns></returns>
        string BuildConnectionString(string server, string user, string password, string databasePath);

        /// <summary>
        /// Links a stored search term to a search request
        /// </summary>
        /// <param name="requestId">Id of the search request to which this term will be attached</param>
        /// <param name="searchTermId">Id of the search term to be stored against this request</param>
        void StoreSearchTermAgainstRequest(int requestId, int searchTermId);
    }
}