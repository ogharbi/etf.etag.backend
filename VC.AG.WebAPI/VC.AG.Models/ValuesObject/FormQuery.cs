using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Enums;

namespace VC.AG.Models.ValuesObject
{
    public class FormQuery : DBQuery
    {
        public string? Site { get; set; }
        public RequestStatus? Status { get; set; }
        public RequestScope? Scope { get; set; }
        public TaskTarget? TaskTarget { get; set; }
        public string? ContentTypeId { get; set; }
        public string? FormType { get; set; }
        public bool? InlineQuery { get; set; }
    }
}
