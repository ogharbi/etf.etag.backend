using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqAssignAccess
    {
        public string? Site { get; set; }
        public int? UserId { get; set; }
        public string? Role { get; set; }
        public string? Action { get; set; }
        public AssignAccess GetAssignAccess()
        {
            var rsucess = Enum.TryParse(Role, out UserRole role);
            if (!rsucess) role = UserRole.None;
            var asuccess = Enum.TryParse(Action, out AssignAction action);
            if (!asuccess) action = AssignAction.None;
            var r = new AssignAccess() { Site = Site, UserId = UserId, Role = role, Action = action };
            return r;
        }
    }

}
