namespace VC.AG.WebAPI.Models
{
    public class ReqUpdates
    {
        public ReqUpdate[]? Data { get; set; }
        public string? Site { get; set; }
        public string? List { get; set; }
    }
    public class ReqCreates
    {
        public ReqCreate[]? Data { get; set; }
        public string? Site { get; set; }
        public string? List { get; set; }
    }
}
