using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.News.DataModel;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.Extensions
{
    public static class SiteInfoExtensions
    {
        public static SiteEntity GetBasicInfo(this SiteEntity site)
        {
            var result = new SiteEntity()
            {
                Id = site.Id,
                SiteUrl = site.SiteUrl,
                RootFolder = site.RootFolder,
                Title = site.Title
            };
            return result;
        }

    }
}

