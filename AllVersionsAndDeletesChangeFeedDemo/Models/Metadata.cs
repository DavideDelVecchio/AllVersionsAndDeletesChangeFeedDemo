using Newtonsoft.Json;

namespace AllVersionsAndDeletesChangeFeedDemo.Models
{
    internal class Metadata
    {
        [JsonProperty("operationType")]
        public string OperationType { get; set; }

        [JsonProperty("timeToLiveExpired")]
        public Boolean? TimeToLiveExpired { get; set; }
    }
}