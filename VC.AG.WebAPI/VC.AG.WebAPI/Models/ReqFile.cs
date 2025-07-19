using VC.AG.Models;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqFile
    {
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? FileName { get; set; }
        public int? ParentId { get; set; }
        public string? ParentFormType { get; set; }
        public int? Id { get; set; }
        public DBFile? ToDBFile()
        {
            DBFile? result = new()
            {
                Name = FileName,
                ListName = ListName,
                Site = Site,
                ParentFormType = ParentFormType,
                Id=Id.GetValueOrDefault(),
                ParentId = ParentId
            };

            return result;
        }

    }
}
