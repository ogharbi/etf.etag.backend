using VC.AG.Models.Entities;

namespace VC.AG.Models.ValuesObject
{
    public class DBQuery
    {
        public int Id { get; set; }
        public string? SiteUrl { get; set; }
        public string? SiteId { get; set; }
        public string? ListId { get; set; }
        public string? ListUrl { get; set; }
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
        public bool? CatchError { get; set; }
        public UserEntity? User { get; set; }
        public bool? Force { get; set; }

    }
}
