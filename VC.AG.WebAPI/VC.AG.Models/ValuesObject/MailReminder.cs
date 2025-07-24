using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class MailReminder
    {
        public string? Site { get; set; }
        public string? SiteName { get; set; }
        public string? SiteUrl { get; set; }
        public string? ListId { get; set; }
        public int? itemId { get; set; }
        public string? To { get; set; }
        public string? Status { get; set; }
        public string? Summury { get; set; }
        public DateTime? Date { get; set; }
        public int? ReminderFeq { get; set; }
        public Dictionary<string,object>? Values { get; set; }
    }
}
