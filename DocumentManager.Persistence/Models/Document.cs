using Newtonsoft.Json;

namespace DocumentManager.Persistence.Models
{
    /// <summary>
    /// Class for representing document files
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Document identifier
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Partition key
        /// </summary>
        [JsonProperty(PropertyName = "partition")]
        public string Partition { get; set; }

        /// <summary>
        /// Document name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Location where document file is stored
        /// </summary>
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Size of the document file
        /// </summary>
        [JsonProperty(PropertyName = "fileSize")]
        public long FileSize { get; set; }

        /// <summary>
        /// Position in ordered collection
        /// </summary>
        [JsonProperty(PropertyName = "position")]
        public int Position { get; set; }
    }
}
