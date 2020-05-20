using Newtonsoft.Json;

namespace DocumentManager.Api.Models
{
    public class Document
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }
        [JsonProperty(PropertyName = "fileSize")]
        public long FileSize { get; set; }
    }
}
