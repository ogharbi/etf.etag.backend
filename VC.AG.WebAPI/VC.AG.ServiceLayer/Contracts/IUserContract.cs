using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface IUserContract
    {
        Task<UserEntity?> GetMe(bool force = false);
        Task<UserEntity?> Get(string email, bool force = false);
        Task<UserEntity?> GetById(int? spId);
        Task<IEnumerable<UserEntity>?> Search(string word);
        Task<string?> Assign(AssignAccess assign);
    }
}
