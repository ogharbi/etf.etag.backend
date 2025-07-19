namespace VC.AG.Models.ValuesObject
{
    public class PeoplePicker
    {
        public string? Key { get; set; }
        public string? DisplayText { get; set; }
        public PPEntityData? EntityData { get; set; }
    }
    public class PPEntityData
    {
        public string? Title { get; set; }
        public string? Email { get; set; }

    }
}
