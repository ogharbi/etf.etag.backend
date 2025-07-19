using VC.AG.DAO.Contracts;

namespace VC.AG.DAO.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository UserRep { get; }
        IDBRepository DBRepo { get; }
        IFileRepository FileRepo { get; }
    }
}
