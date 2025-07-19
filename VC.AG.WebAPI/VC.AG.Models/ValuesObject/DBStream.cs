using Newtonsoft.Json;

namespace VC.AG.Models.ValuesObject
{
    public class DBStream
    {
        public string? NextHref { get; set; }
        public List<Dictionary<string, object>>? Row { get; set; }
    }
    public class StreamLookup
    {
        [JsonProperty("lookupId")]
        public int LookupId { get; set; }
        [JsonProperty("lookupValue")]
        public string? LookupValue { get; set; }
    }
    public class StreamUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string? Title { get; set; }
        [JsonProperty("email")]
        public string? Email { get; set; }
        [JsonProperty("sip")]
        public string? Sip { get; set; }
    }
}
