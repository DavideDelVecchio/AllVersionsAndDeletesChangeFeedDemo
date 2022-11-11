using Newtonsoft.Json;

namespace AllVersionsAndDeletesChangeFeedDemo.Models
{
    internal class Metadata
    {
        [JsonProperty("operationType")]
        public string operationType { get; set; }

        [JsonProperty("timeToLiveExpired")]
        public Boolean timeToLiveExpired { get; set; }

        [JsonProperty("previousImage")]
        public Item previousImage { get; set; }
    }
}
