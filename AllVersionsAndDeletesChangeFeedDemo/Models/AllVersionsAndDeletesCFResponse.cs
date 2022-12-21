using Newtonsoft.Json;

namespace AllVersionsAndDeletesChangeFeedDemo.Models
{
    internal class AllVersionsAndDeletesCFRepsonse
    {
        [JsonProperty("current")]
        public Item Current { get; set; }

        [JsonProperty("previous")]
        public Item Previous { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }
}
