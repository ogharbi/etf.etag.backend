using Microsoft.BusinessData.MetadataModel;
using Microsoft.SharePoint.Client.Search.Query;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Helpers;

namespace VC.AG.Models.ValuesObject
{
    public class DBUpdate
    {
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? SiteId { get; set; }
        public string? ListId { get; set; }
        public string? Comment { get; set; }
        public string? FormType { get; set; }
        public int? Id { get; set; }
        public dynamic? Data { get; set; }
        public DBQuery ToDBQuery()
        {
            var dbQuery = new DBQuery()
            {
                ListName = ListName,
                SiteId = SiteId,
                ListId = ListId,
                ItemId = $"{Id}",
                Filter = CamlHelper.BuildCondition("ID", $"{Id}", SPFieldType.Number, CamlOp.Eq, true)
            };
            return dbQuery;
        }

    }
}
