using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.Entities
{
    public class SiteEntity
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? SiteUrl { get; set; }
        public string? RootFolder { get; set; }
        public Dictionary<string, object>? Lists { get; set; }
        public Dictionary<string, SPList>? ListsMeta { get; set; }
        public Dictionary<string, object>? Drives { get; set; }
        public Dictionary<string, object>? Sites { get; set; }
        public IEnumerable<SPList>? Schemas { get; set; }
        public IEnumerable<DBItem>? Settings { get; set; }
        public IEnumerable<DBItem>? Bus { get; set; }
        public IEnumerable<DBItem>? MailTemplates { get; set; }

        /// <summary>
        /// /////////////Sub site props/////////////////
        /// </summary>
        /// 
        public IEnumerable<DBItem>? SiteLinks { get; set; }
       

      
    }
}
