using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Enums;

namespace VC.AG.Models.ValuesObject
{
    public class MailLog
    {
        public string? Site { get; set; }
        public string? AppUrl { get; set; }
        public MailType? Type { get; set; }
        public WfRequest? Request { get; set; }
        public string? Result { get; set; }
    }
}
