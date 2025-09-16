using System.IO;
using VC.AG.Models;
using VC.AG.Models.Enums;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;

namespace VC.AG.WebAPI.Models
{
    public class FileCreate
    {
        public AppTarget? AppTarget { get; set; }
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? ParentId { get; set; }
        public string? Code { get; set; }
        public string? Comment { get; set; }
      
        public DBFile? ToDBFile(IFormFile? file, IUserContract userSvc)
        {
            DBFile? result = null;
            if (file != null)
            {

                using Stream stream = file.OpenReadStream();
                using var binaryReader = new BinaryReader(stream);
                var name = file.FileName.Replace("'", "").Replace("+", "-");
                var targetList = AppTarget == AG.Models.Enums.AppTarget.CT ? ListNameKeys.CTAttachments : ListNameKeys.AGAttachments;
                var lkField = AppTarget == AG.Models.Enums.AppTarget.CT ? AppKeys.Lk_Contract: AppKeys.Lk_Request;

                var properties = new Dictionary<string, object>
                        {
                            { "Title", string.Format("ETF-{0}", name) },
                            { AppConstants.AppKeys.ParentId, $"{ParentId}" },
                            { AppKeys.Code, $"{Code}" },
                            { AppConstants.AppKeys.Comment, $"{Comment}" },
                            { $"{lkField}LookupId", $"{ParentId}" },

                        };
                var enabledAuthorLists = new string[] { AppConstants.ListNameKeys.Interview, AppConstants.ListNameKeys.Contract, AppConstants.ListNameKeys.AGAttachments, AppConstants.ListNameKeys.CTAttachments, AppConstants.ListNameKeys.Comment };
                if (enabledAuthorLists.Contains(targetList?.ToLower()))
                {
                    var user = userSvc.GetMe().Result;
                    if (user != null)
                    {
                        properties.Add($"{AppConstants.AppKeys.Author}LookupId", user.SPId);
                        properties.Add($"{AppConstants.AppKeys.Editor}LookupId", user.SPId);
                    }
                }
                result = new()
                {
                    Content = binaryReader.ReadBytes((int)file.Length),
                    Created = DateTime.Now,
                    Name = $"VC-{Site}-{Code}-{ParentId}-{name}",
                    Values = properties,
                    Site = Site,
                    ListName = targetList
                };
            }
            return result;
        }
    }
}
