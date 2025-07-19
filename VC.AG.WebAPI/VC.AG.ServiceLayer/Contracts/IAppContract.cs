using Microsoft.Extensions.Logging;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface IAppContract
    {
        Task<SiteEntity?> GetSite(string delegation = "", bool force = false);
        Task<SiteEntity?> RefreshSite(SiteRefreshTarget target,string delegation = "");
        Task<DBStream?> GetAll(DBQuery query, string? delegation = "");
        Task<IEnumerable<DBItem>?> GetRessource(Ressource resource, string? delegation = "", string? listName = "", string? viewId = "");

        Task<DBItem?> Post(DBCreate item);
        Task<DBItem?> Put(DBUpdate item);
        Task<string> Delete(DBUpdate item);
        Task<string?> PostForm(DBFormData item);
     
    }
}
