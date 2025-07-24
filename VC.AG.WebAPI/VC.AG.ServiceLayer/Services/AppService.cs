using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Extensions;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.ServiceLayer.Helpers;
using Wkhtmltopdf.NetCore;
using static OfficeOpenXml.ExcelErrorValue;
using static VC.AG.Models.AppConstants;

namespace VC.AG.ServiceLayer.Services
{
    public class AppService(IUnitOfWork uow, ISiteContract siteSvc,IMemoryCache cache, IConfiguration config) : IAppContract
    {
        readonly SpoContext spoContext = new(config, cache);
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
                    case Ressource.SiteLinks:
                        result = site.SiteLinks;
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


        public async Task<string?> SendReminder(DateTime? startDate,DateTime? endDate)
        {
            //var result = await notifSvc.SendReminder(startDate,endDate);
            //return $"{result}";
            return null;
        }

        public async Task<FileModel?> GetPdf(IGeneratePdf generatePdf, DBQuery qp)
        {
            FileModel? result = null;
            IDictionary<string, Object> itemPdf = new Dictionary<string, Object>();
            var qInterviews = new List<dynamic>();
            var actions = new List<dynamic>();
            var force = false;
            var site = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the site");

            var dbFile = new DBFile()
            {
                SiteId = site.Id,
                DriveId = site.Drives?[ListNameKeys.DocTemplates] as string,
                Name = "Carnet.html"
            };
            DBFile? file = await uow.FileRepo.Get(dbFile, true);
            if (file != null && file.ContentStream != null)
            {
                string htmlCacheKey = $"app-html-template-carnet";
                cache.TryGetValue(htmlCacheKey, out string? html);
                if (string.IsNullOrEmpty(html))
                {
                    var buffer = file.ContentStream.ReadAllBytes();
                    html = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    // cache.Set(htmlCacheKey, html);
                }
                IDictionary<string, Object> mValues = new Dictionary<string, Object>();
                var dbQuery = new DBQuery()
                {
                    ListName = ListNameKeys.Interview,
                    Filter = $"<Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{qp.Id}</Value></Eq></Where>"
                };
                var mainItem = await GetAll(dbQuery);
                if (mainItem != null)
                {
                    var values = mainItem.Row?.FirstOrDefault();
                    if (values != null)
                    {
                        foreach (KeyValuePair<string, object> it in values)
                        {
                            if (!itemPdf.ContainsKey(it.Key)) itemPdf.Add(it.Key, it.Value);
                        }
                    }
                    dbQuery = new DBQuery()
                    {
                        ListName = ListNameKeys.QInterview,
                        Filter = $"<Where><Eq><FieldRef Name='Col_Lk_Request' LookupId='True'/><Value Type='Lookup'>{qp.Id}</Value></Eq></Where>"
                    };
                    var qItems = await GetAll(dbQuery);

                    foreach (var sub in qItems.Row)
                    {
                        var qValues = new Dictionary<string, Object>();
                        var title = "" + sub["Title"];
                        foreach (KeyValuePair<string, object> it in sub)
                        {
                            if (!qValues.ContainsKey(it.Key)) qValues.Add(it.Key, it.Value);
                        }
                        qInterviews.Add(qValues);
                    }

                    dbQuery = new DBQuery()
                    {
                        ListName = ListNameKeys.Actions,
                        Filter = $"<Where><Eq><FieldRef Name='Col_Lk_Request' LookupId='True'/><Value Type='Lookup'>{qp.Id}</Value></Eq></Where>"
                    };
                    var aItems = await GetAll(dbQuery);

                    foreach (var sub in aItems.Row)
                    {
                        var aValues = new Dictionary<string, Object>();
                        foreach (KeyValuePair<string, object> it in sub)
                        {
                            if (!aValues.ContainsKey(it.Key)) aValues.Add(it.Key, it.Value);
                        }
                        actions.Add(aValues);
                    }
                    var itemPdfGuid = itemPdf.ContainsKey("Col_Guid") ? "" + itemPdf["Col_Guid"] : string.Empty;
                    if (!string.IsNullOrEmpty(itemPdfGuid))
                    {
                        var subItems = actions.Where(a => "" + a["Col_Guid"] == itemPdfGuid).ToList();
                        itemPdf.Add("Objectifs", subItems);
                    }
                    var i = 1;
                    foreach (var qItem in qInterviews)
                    {
                        var guid = qItem.ContainsKey("Col_Guid") ? "" + qItem["Col_Guid"] : string.Empty;
                        if (!string.IsNullOrEmpty(guid))
                        {
                            var subItems = actions.Where(a => ("" + a["Title"]).Equals(ActionType.MissionRealisee.ToString(), StringComparison.OrdinalIgnoreCase) && "" + a["Col_Guid"] == guid).ToList();
                            qItem.Add("Missions", subItems.AsEnumerable().OrderBy(x => x["Col_Order."]));
                            subItems = actions.Where(a => ("" + a["Title"]).Equals(ActionType.AxeProgres.ToString(), StringComparison.OrdinalIgnoreCase) && "" + a["Col_Guid"] == guid).ToList();
                            qItem.Add("Axes", subItems.AsEnumerable().OrderBy(x => x["Col_Order."]));
                            subItems = actions.Where(a => ("" + a["Title"]).Equals(ActionType.Engagement.ToString(), StringComparison.OrdinalIgnoreCase) && "" + a["Col_Guid"] == guid).ToList();
                            qItem.Add("Engagements", subItems.AsEnumerable().OrderBy(x => x["Col_Order."]));
                            qItem["index"] = i;
                            qItem["qindex"] = i * 3;
                            i++;
                        }
                    }
                    itemPdf.Add("QItems", qInterviews.AsEnumerable().OrderBy(x => x["Col_Order."]));
                    var stream = AppHelper.GetPdfStream(generatePdf, html, itemPdf, Wkhtmltopdf.NetCore.Options.Orientation.Portrait);
                    result = new FileModel() { Title = $"{itemPdf["Title"]}.pdf", ContentStream = stream };

                }
            }
            return result;
        }
    }
}
