using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using VC.AG.DAO.Contracts;
using VC.AG.DAO.Respository;

namespace VC.AG.DAO.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly IConfiguration config;
        readonly IMemoryCache cache;
        public UnitOfWork(IConfiguration config, IMemoryCache cache)
        {
            this.config = config;
            this.cache = cache;
        }
        public IUserRepository UserRep => new UserRepository(config, cache);
        public IDBRepository DBRepo => new DBRepository(config, cache);
        public IFileRepository FileRepo => new FileRepository(config, cache);
    }
}
