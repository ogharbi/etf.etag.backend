using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class SiteLink
    {
        public string? Title { get; set; }
        public string? Target { get; set; }
        public string? Url { get; set; }
        public bool? NewTab { get; set; }
        public int? Order { get; set; }

    }
}
