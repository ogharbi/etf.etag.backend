using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqQuery
    {
        public int? Id { get; set; }
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? ItemId { get; set; }
        public string? Filter { get; set; }
        public string? Expand { get; set; }
        public string? Select { get; set; }
        public List<string>? Fields { get; set; }
        public string? OrderBy { get; set; }
        public int? Top { get; set; }
        public int? Skip { get; set; }
        public string? SearchTerm { get; set; }
        public string? AppendQuery { get; set; }
        public string? NextHref { get; set; }
        public DBQuery ToDGQuery()
        {
            var q = new DBQuery()
            {
                ItemId = ItemId,
                ListName = ListName,
                Filter = Filter,
                Expand = Expand,
                Select = Select,
                Fields = Fields,
                OrderBy = OrderBy,
                Top = Top,
                Skip = Skip,
                SearchTerm = SearchTerm,
                AppendQuery = AppendQuery,
                NextHref = string.IsNullOrEmpty(NextHref) ? null : NextHref[1..]
            };
            return q;
        }
    }

}
