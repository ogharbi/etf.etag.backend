namespace VC.AG.Models.ValuesObject
{
    public class SPList
    {
        public string? Title { get; set; }
        public string? ListId { get; set; }
        public string? RootFolder { get; set; }
        public string? Url { get; set; }
        public string? RelativeUrl { get; set; }
        public string? Template { get; set; }
        public Guid Id { get; set; }
        public IEnumerable<DBItem>? Columns { get; set; }
        public IEnumerable<DBItem>? ContentTypes { get; set; }
        public IEnumerable<DBItem>? Views { get; set; }
    }
    public class SPField
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? StaticName { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public string? Format { get; set; }
        public string? SchemaXml { get; set; }
        public string? DefaultValue { get; set; }
        public List<string>? Values { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public bool Indexed { get; set; }
    }
    public class SPView
    {
        public string? Title { get; set; }
        public string? ServerRelativeUrl { get; set; }
        public List<string>? Fields { get; set; }
        public bool? IsDefault { get; set; }
    }
}
