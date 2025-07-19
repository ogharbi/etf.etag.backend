using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;

namespace VC.AG.DAO.Contracts
{
    public interface IDBRepository
    {
        Task<SiteEntity?> GetSite(string delegation = "");
        Task<IEnumerable<DBItem>?> GetAll(DBQuery query);
        Task<DBStream?> GetStream(DBQuery query,bool? all=false);
        Task<string?> GetFilterValues(DBQuery query);
        Task<IEnumerable<DBItem>?> GetListViews(DBQuery query);
        Task<IEnumerable<DBItem>?> GetListContentTypes(DBQuery query);
        Task<IEnumerable<DBItem>?> GetListColumns(DBQuery query);
        Task<DBItem?> GetListView(DBQuery query);
        Task<DBItem?> Post(DBCreate item);
        Task<DBItem?> Put(DBUpdate item);
        Task<string?> PostForm(DBFormData item);
        Task<string> Delete(DBUpdate item);
    }
}
