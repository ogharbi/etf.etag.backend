using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class VPRessourceModel
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? ParentId { get; set; }
    }
    public class VPResponse
    {
        [JsonProperty("totalItems")]
        public string? Name { get; set; }
        [JsonProperty("entities")]
        public List<List<VPEntity>>? Entities { get; set; }
    }
    public class VPEntity
    {
        [JsonProperty("entityName")]
        public string? EntityName { get; set; }
        [JsonProperty("entityValue")]
        public string? EntityValue { get; set; }
    }
}
