using Newtonsoft.Json;

namespace VC.AG.Models.ValuesObject
{
    public class DBCreate
    {
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? SiteId { get; set; }
        public string? ListId { get; set; }
        public dynamic? Data { get; set; }
        public int? ID{ get; set; }

        public DBUpdate ToDBDUpdate()
        {
            var item = new DBUpdate()
            {
                Site = Site,
                ListName = ListName,
                Data = Data
            };
         
            return item;
        }
    }
}
