using VC.AG.Models;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;
using static VC.AG.Models.AppConstants;

namespace VC.AG.WebAPI.Models
{
    public class ReqFile
    {
        public AppTarget? AppTarget { get; set; }
        public string? Site { get; set; }
        public string? FileName { get; set; }
        public int? ParentId { get; set; }
        public string? ParentFormType { get; set; }
        public int? Id { get; set; }
        public DBFile? ToDBFile()
        {
            var targetList = AppTarget == AG.Models.Enums.AppTarget.CT ? ListNameKeys.CTAttachments : ListNameKeys.AGAttachments;
            DBFile? result = new()
            {
                Name = FileName,
                ListName = targetList,
                Site = Site,
                ParentFormType = ParentFormType,
                Id=Id.GetValueOrDefault(),
                ParentId = ParentId
            };

            return result;
        }

    }
}
