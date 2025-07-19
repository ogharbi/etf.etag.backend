using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using System;
using System.Reflection;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.ServiceLayer.Helpers;
using static VC.AG.Models.AppConstants;

namespace VC.AG.ServiceLayer.Services
{
    public class AppService(IUnitOfWork uow, ISiteContract siteSvc, INotifContract notifSvc, IMemoryCache cache, IConfiguration config) : IAppContract
    {
        readonly SpoContext spoContext = new(config, cache);
        readonly JobHelper jobHelper = new(uow, config, cache, siteSvc);
        public async Task<SiteEntity?> GetSite(string delegation = "", bool force = false)
        {
            var result = await siteSvc.Get(delegation, force) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            return result;
        }
        public async Task<SiteEntity?> RefreshSite(SiteRefreshTarget target, string delegation = "")
        {
            var site = await siteSvc.Refresh(target, delegation);
            return site;
        }
        public async Task<DBStream?> GetAll(DBQuery query, string? delegation = "")
        {
            DBStream? result;
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            var q = query;
            q.SiteUrl = site.SiteUrl;
            q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
            result = await uow.DBRepo.GetStream(q);

            return result;

        }
        public async Task<IEnumerable<DBItem>?> GetRessource(Ressource resource, string? delegation = "", string? listName = "", string? viewId = "")
        {
            delegation = $"{delegation}";
            var site = await siteSvc.Get(delegation);
            IEnumerable<DBItem>? result = null;
            if (site != null)
            {
                var rs = new List<DBItem>();
                var listId = string.IsNullOrEmpty(listName) ? null : site.Lists?.GetStringValue2($"{listName}");
                if (string.IsNullOrEmpty(listId)) listId = listName;
                switch (resource)
                {
                    case Ressource.None:
                        break;
                    case Ressource.AppSettings:
                        result = site.Settings;
                        break;
                    case Ressource.Bus:
                        result = site.Bus;
                        break;
                    case Ressource.Views:
                        var q1 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = listId, CatchError = true };
                        result = await uow.DBRepo.GetListViews(q1);
                        break;
                    case Ressource.ContentTypes:
                        var q2 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = listId, CatchError = true };
                        result = await uow.DBRepo.GetListContentTypes(q2);
                        break;
                    case Ressource.Fields:
                        var q3 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = listId, CatchError = true };
                        result = await uow.DBRepo.GetListColumns(q3);
                        break;
                    case Ressource.View:
                        var q4 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = listId, ItemId = viewId, CatchError = true };
                        var view = await uow.DBRepo.GetListView(q4);
                        if (view != null) rs.Add(view);
                        result = rs;
                        break;
                    case Ressource.Lists:
                        var lists = site.ListsMeta?.Select(item => item.Value);
                        GetListsMeta(ref rs, lists);
                        result = rs;
                        break;

                    default:
                        break;
                }
            }
            return result;
        }

        static void GetListsMeta(ref List<DBItem> rs, IEnumerable<SPList>? lists)
        {
            if (lists != null)
            {
                foreach (var list in lists)
                {
                    var r = new DBItem();
                    if (list != null)
                    {
                        r.Id = list.Id.ToString();
                        r.Title = list.Title;
                        var values = new Dictionary<string, object>
                        {
                            ["Title"] = $"{list.Title}",
                            ["Url"] = $"{list.Url}",
                            ["Template"] = $"{list.Template}",
                            ["RootFolder"] = $"{list.RootFolder}"
                        };
                        r.Values = values;
                        rs.Add(r);
                    }

                }
            }
        }

        public async Task<DBItem?> Post(DBCreate item)
        {

            var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
            item.SiteId = site.Id;
            item.ListId = site.Lists?.GetStringValue2($"{item.ListName}");
            DBItem? result = await uow.DBRepo.Post(item);

            return result;
        }

        public async Task<DBItem?> Put(DBUpdate item)
        {
            var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
            item.SiteId = site.Id;
            item.ListId = site.Lists?.GetStringValue2($"{item.ListName}");
            DBItem? result = await uow.DBRepo.Put(item);

            return result;
        }

        public async Task<string> Delete(DBUpdate item)
        {
            var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
            item.SiteId = site.Id;
            item.ListId = site.Lists?.GetStringValue2($"{item.ListName}");
            var result = await uow.DBRepo.Delete(item);
            return result;
        }

        public async Task<string?> PostForm(DBFormData item)
        {
            var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
            item.SiteUrl = site.SiteUrl;
            item.ListId = site.Lists?.GetStringValue2($"{item.ListName}");
            if (item.ListName.EqualsNotNull(ListNameKeys.SiteLinks))
            {
                item = item.ToSiteLink();
            }
            string? result = await uow.DBRepo.PostForm(item);
            return result;
        }

      
        public async Task<string?> SendReminder(ILogger logger)
        {
            var result = await notifSvc.SendReminder(logger);
            return $"{result}";
        }

       
    }
}
