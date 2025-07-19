using VC.AG.Models.Enums;

namespace VC.AG.Models.ValuesObject
{
    public class AssignAccess
    {
        public string? Site { get; set; }
        public string? SiteUrl { get; set; }
        public int? UserId { get; set; }
        public UserRole? Role { get; set; }
        public AssignAction? Action { get; set; }
    }
}
