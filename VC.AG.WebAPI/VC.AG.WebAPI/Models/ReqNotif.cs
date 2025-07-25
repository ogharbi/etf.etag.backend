using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqNotif
    {
        public int Id { get; set; }
        public NotifType? Type { get; set; }
        public string? ListName { get; set; }
        public string? Comment { get; set; }
        public NotifQuery ToNotifQuery()
        {
            return new NotifQuery
            {
                Id = Id,
                Type = Type,
                Comment = Comment,
                ListName = ListName
            };
        }
    }
}
