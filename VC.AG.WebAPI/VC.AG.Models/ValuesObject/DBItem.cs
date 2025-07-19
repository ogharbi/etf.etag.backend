namespace VC.AG.Models.ValuesObject
{
    public class DBItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? UniqueId { get; set; }
        public DateTime? Created { get; set; }
        public IDictionary<string, object>? Values { get; set; }

    }
}
