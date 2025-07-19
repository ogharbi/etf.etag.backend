using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.ValuesObject;

namespace VC.AG.DAO.Contracts
{
    public interface IFileRepository
    {
        Task<DBFile?> Get(DBFile? dBFile,bool? load = false);
        Task<DBFile?> Post(DBFile? item);
        Task<string> Delete(DBFile item);
    }
}
