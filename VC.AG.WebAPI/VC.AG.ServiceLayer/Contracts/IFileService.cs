using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Contracts
{
    public interface IFileService
    {
        Task<DBFile?> Get(DBFile? file);
        Task<DBFile?> Upload(DBFile? file);
        Task<string?> Delete(DBFile? file);
    }
}
