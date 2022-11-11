using Newtonsoft.Json;

namespace AllVersionsAndDeletesChangeFeedDemo.Models
{
    internal class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public double Price { get; set; }

        public string BuyerState { get; set; }
    }
}
