namespace DocumentManager.Api.Options
{
    /// <summary>
    /// Configuration options for connecting to Azure Blob Storage
    /// </summary>
    public class AzureBlobOptions
    {
        /// <summary>
        /// Account name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Container name
        /// </summary>
        public string Container { get; set; }
    }
}
