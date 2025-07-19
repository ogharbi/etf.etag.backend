using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.ValuesObject.SPContext;
using VC.AG.ServiceLayer.Contracts;

namespace VC.AG.ServiceLayer.Helpers
{
    public class JobHelper(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, ISiteContract siteSvc)
    {
        private const string title = "Title";

        readonly IUnitOfWork uow = uow;
        readonly ISiteContract siteSvc = siteSvc;
        readonly IConfiguration config = config;
        readonly SpoContext spoContext = new(config, cache);
        readonly GraphContext graphContext = new(config, cache);
    }
}
