using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.ValuesObject
{
    public class DBFile
    {
        public int Id { get; set; }
        public string? UniqueId { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public DateTime Created { get; set; }
        public string? Name { get; set; }
        public byte[]? Content { get; set; }
        public Stream? ContentStream { get; set; }
        public IDictionary<string, object>? Values { get; set; }
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? SiteId { get; set; }
        public string? SiteUrl { get; set; }
        public string? DriveId { get; set; }
        public int? ParentId { get; set; }
        public bool? SkipContent { get; set; }
        public string? ParentFormType { get; set; }
        public DBFile ToBasicInfo()
        {
            var result = new DBFile() { Name = Name, Url = Url };
            return result;
        }
        public DBQuery ToDbQuery(string? siteUrl)
        {
            object? pid = null;
            if (ParentId.HasValue) pid = ParentId.Value;
            Values?.TryGetValue(AppKeys.ParentId, out pid);
            var q = new DBQuery()
            {
                SiteUrl = siteUrl,
                ListName = ListNameKeys.Interview,
                Filter = $"<Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{pid}</Value></Eq></Where>"
            };
            return q;
        }
    }
}
