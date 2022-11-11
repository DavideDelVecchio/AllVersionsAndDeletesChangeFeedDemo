using Newtonsoft.Json;

namespace AllVersionsAndDeletesChangeFeedDemo.Models
{
    internal class ItemWithMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public double Price { get; set; }

        public string BuyerState { get; set; }

        [JsonProperty("_metadata")]
        public Metadata metadata { get; set; }
    }
}
