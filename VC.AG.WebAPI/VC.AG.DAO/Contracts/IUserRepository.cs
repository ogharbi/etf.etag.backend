using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;

namespace VC.AG.DAO.Contracts
{
    public interface IUserRepository
    {
        Task<UserEntity?> Get(string? email = null, int? id = null);
        Task<IEnumerable<UserEntity>?> Search(string word);
        Task<string?> UpdateAssignment(AssignAccess assign);
    }
}
