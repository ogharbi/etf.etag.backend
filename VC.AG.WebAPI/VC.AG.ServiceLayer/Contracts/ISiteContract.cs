using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface ISiteContract
    {
        Task<SiteEntity?> Get(string? delegation = "", bool force = false,bool content=true);
        Task<SiteEntity?> Refresh(SiteRefreshTarget target, string? delegation = "");
        Task<IEnumerable<Access>?> GetUserAccess(UserEntity user, bool force = false);
    }
}
