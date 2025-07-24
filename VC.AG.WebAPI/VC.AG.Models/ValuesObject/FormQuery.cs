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
        public DashMode? Mode { get; set; }
        public RequestStatus? Status { get; set; }
        public RequestScope? Scope { get; set; }
        public string? Data { get; set; }
        public string? ContentTypeId { get; set; }
        public string? AigField { get; set; }
        public bool? InlineQuery { get; set; }
        public string? DateField { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

    }
}
