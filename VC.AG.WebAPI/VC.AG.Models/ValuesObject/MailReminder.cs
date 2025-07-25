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
        public int? RequestId { get; set; }
        public string? Title { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName{ get; set; }
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public string? Summury { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public Dictionary<string,object>? Values { get; set; }
    }
}
