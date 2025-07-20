using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class WfUpdate
    {
        public string? Site { get; set; }
        public string? List { get; set; }
        public int? Id { get; set; }
        public bool? Force { get; set; }
        public string? Comment { get; set; }
        public DBUpdate ToDBUpdate()
        {
            var item = new DBUpdate()
            {
                Site = Site,
                ListName = List,
                Id = Id
            };
            return item;
        }
    }
}
