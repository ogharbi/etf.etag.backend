using Microsoft.SharePoint.Client;
using VC.AG.Models.ValuesObject;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.Entities
{
    public class UserEntity
    {
        public string? ADId { get; set; }
        public string? DisplayName { get; set; }
        public string? AccountName { get; set; }
        public string? Email { get; set; }
        public int SPId { get; set; }
        public int ProfileId { get; set; }
        public List<string>? Groups { get; set; }
        public bool? IsSiteAdmin { get; set; }
        public IEnumerable<Access>? Access { get; set; }
        public UserEntity()
        {

        }
        public UserEntity(User spUser)
        {
            if (spUser == null) return;
            var groups = spUser.Groups.Select(m => m.Title).ToList();
            SPId = spUser.Id;
            AccountName = spUser.LoginName;
            Email = spUser.Email;
            DisplayName = spUser.Title;
            IsSiteAdmin = spUser.IsSiteAdmin;
            Groups = groups;
            if (Groups.Count > 0)
            {
                var isLocalAdmin = Groups.Exists(a => a.Equals(SiteGroups.Admins, StringComparison.InvariantCultureIgnoreCase));
                IsSiteAdmin = IsSiteAdmin.Value || isLocalAdmin;
            }
        }



    }


}
