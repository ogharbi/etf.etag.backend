using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class DBFormData
    {
        public string? Site{ get; set; }
        public string? SiteUrl { get; set; }
        public string? ListName { get; set; }
        public string? ListId { get; set; }
        public string? Id { get; set; }
        public string? Data { get; set; }
        public Dictionary<string, object>? Values { get; set; }
      
    }
    
}
