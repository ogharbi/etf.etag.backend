using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.Models.Extensions;
using Microsoft.SharePoint.Client.Search.Query;
using static VC.AG.Models.AppConstants;
using Microsoft.SharePoint.News.DataModel;
namespace VC.AG.ServiceLayer.Services
{
    public class FileService(IUnitOfWork uow, ISiteContract siteSvc, IFormContract formSvc) : IFileService
    {

        public async Task<DBFile?> Get(DBFile? file)
        {
            DBFile? result = null;
            var delegation = file?.Site;
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");

            if (file != null)
            {
                var q = file.ToDbQuery(site.SiteUrl);
                file.SiteId = site.Id;
                file.SiteUrl = site.SiteUrl;
                file.DriveId = site.Drives?.GetStringValue2($"{file.ListName}");
                var wfRequest = await formSvc.Get(q, delegation);
                result = wfRequest == null ? null : await uow.FileRepo.Get(file);
            }
            return result;
        }
        public async Task<DBFile?> Upload(DBFile? file)
        {
            DBFile? result = null;
            var delegation = file?.Site;
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (file != null)
            {
                var q = file.ToDbQuery(site.SiteUrl);
                await formSvc.Get(q, delegation);
                file.SiteId = site.Id;
                file.DriveId = site.Drives?.GetStringValue2($"{file.ListName}");
                result = await uow.FileRepo.Post(file);

            }

            return result;

        }
        public async Task<string?> Delete(DBFile? file)
        {
            string? result = null;
            var delegation = file?.Site;
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (file != null)
            {

                var qFile = new DBQuery()
                {
                    SiteUrl = site.SiteUrl,
                    ListId = site.Lists?.GetStringValue2($"{file.ListName}"),
                    Filter = $"<Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{file.Id}</Value></Eq></Where>"
                };
                var rFiles = uow.DBRepo.GetStream(qFile).Result;
                if (rFiles?.Row != null && rFiles.Row.Count > 0)
                {
                    var pId = rFiles.Row[0]?.GetIntValue2(AppKeys.ParentId);
                    file.ParentId = pId;
                    var q = file.ToDbQuery(site.SiteUrl);
                    await formSvc.Get(q, delegation);

                    var deleteOp = new DBUpdate()
                    {
                        SiteId = site.Id,
                        ListId = site.Lists?.GetStringValue2($"{file.ListName}"),
                        Id = file.Id
                    };
                    result = await uow.DBRepo.Delete(deleteOp);
                }

            }
            return result;
        }


    }
}
