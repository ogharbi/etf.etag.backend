using System.IO;
using VC.AG.Models;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;

namespace VC.AG.WebAPI.Models
{
    public class FileCreate
    {
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

                var properties = new Dictionary<string, object>
                        {
                            { "Title", string.Format("VC-{0}", name) },
                            { AppConstants.AppKeys.ParentId, $"{ParentId}" },
                            { AppKeys.Code, $"{Code}" },
                            { AppConstants.AppKeys.Comment, $"{Comment}" },
                            { $"{AppConstants.AppKeys.Lk_Request}LookupId", $"{ParentId}" },

                        };
                var enabledAuthorLists = new string[] { AppConstants.ListNameKeys.Interview, AppConstants.ListNameKeys.RequestAttachments, AppConstants.ListNameKeys.Comment };
                if (enabledAuthorLists.Contains(ListName?.ToLower()))
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
                    ListName = ListName
                };
            }
            return result;
        }
    }
}
