using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Enums;

namespace VC.AG.Models.ValuesObject
{
    public class NotifQuery
    {
        public int Id { get; set; }
        public NotifType? Type { get; set; }
        public string? ListName { get; set; }
        public string? Comment { get; set; }
    }
}
