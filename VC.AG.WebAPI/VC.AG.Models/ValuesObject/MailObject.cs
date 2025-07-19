using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class MailObject
    {
        public List<string>? MailTo { get; set; }
        public List<string>? Cc { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<Attachment>? Attachments { get; set; }
    }
}
