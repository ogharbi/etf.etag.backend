using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface IFormContract
    {
      
        Task<DBStream?> GetAll(DBQuery query, string? delegation = "");
        Task<DBStream?> GetAll(FormQuery query, string? delegation = "");
        Task<List<string>?> GetFilterValues(FormQuery query, string? delegation = "");
        Task<WfRequest?> Get(DBQuery query, string? delegation = "");
        Task<DBItem?> Post(DBCreate item);
        Task<DBItem?> Put(DBUpdate item);
        Task<string> Delete(DBUpdate item);
        Task<DBStream?> Ressources(FormQuery query, string? delegation = "");
    
      
    }
}
