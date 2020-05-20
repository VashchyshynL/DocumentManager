namespace DocumentManager.Api.Options
{
    /// <summary>
    /// DataBase configuration options
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// DataBase endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Access key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Container name
        /// </summary>
        public string Container { get; set; }
    }
}
