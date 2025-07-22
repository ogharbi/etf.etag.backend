using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models.ValuesObject
{
    public class FileModel
    {
        public int Id { get; set; }
        public string? UniqueId { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public DateTime? Created { get; set; }
        public string? Name { get; set; }
        public byte[]? Content { get; set; }
        public Stream? ContentStream { get; set; }
        public IDictionary<string, object>? Values { get; set; }
    }
}
