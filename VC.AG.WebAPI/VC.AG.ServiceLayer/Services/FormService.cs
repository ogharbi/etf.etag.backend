using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.Models.Extensions;
using static VC.AG.Models.AppConstants;
using VC.AG.Models.Helpers;
using Microsoft.SharePoint.News.DataModel;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using VC.AG.Models;
using VC.AG.ServiceLayer.Helpers;
using VC.AG.Models.ValuesObject.SPContext;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Microsoft.Office.SharePoint.Tools;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.AspNetCore.Http.HttpResults;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace VC.AG.ServiceLayer.Services
{
    public class FormService(IUnitOfWork uow, IMemoryCache cache, IConfiguration config, ISiteContract siteSvc, IUserContract userSvc) : IFormContract
    {
        readonly SpoContext spoContext = new(config, cache);
        // readonly JobHelper jobHelper = new(uow, config, cache, siteSvc);

        public async Task<WfRequest?> Get(DBQuery query, string? delegation = "")
        {
            var user = await userSvc.GetMe();
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            var q = query;
            q.SiteUrl = site.SiteUrl;
            q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
            var stream = await uow.DBRepo.GetStream(q);
            if (stream != null && stream.Row?.Count == 0) throw new Exception(Commun.NotFoundOp);
            // stream = query.Force == true ? stream : stream?.CheckAccess($"{delegation}", user, true);
            WfRequest? result = stream?.ToWfRequest(delegation);
            return result;
        }
        public async Task<DBStream?> GetAll(DBQuery query, string? delegation = "")
        {
            DBStream? result = null;
            if (!ListNameKeys.Interview.EqualsNotNull(query?.ListName)) throw new InvalidOperationException($"Query authorized only for {query?.ListName}");
            var user = await userSvc.GetMe();
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (query != null)
            {
                var q = query;
                q.SiteUrl = site.SiteUrl;
                q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
                result = await uow.DBRepo.GetStream(q);
                //result = result?.CheckAccess($"{delegation}", user);
            }
            return result;
        }
        public async Task<DBStream?> GetAll(FormQuery query, string? delegation = "")
        {
            DBStream? result = null;
            var user = await userSvc.GetMe();
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (query != null)
            {
                var q = query;
                q.SiteUrl = site.SiteUrl;
                q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
                q.Site = delegation;
                if (string.IsNullOrEmpty(query.Filter) && query.InlineQuery != true)
                    query.Filter = query.GetQuery(user);
                result = await uow.DBRepo.GetStream(q);
            }
            return result;
        }
        public async Task<FileModel?> Export(FormQuery query, string? delegation = "")
        {
            FileModel? result = null;
            var user = await userSvc.GetMe();
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (query != null)
            {
                var q = query;
                q.SiteUrl = site.SiteUrl;
                q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
                q.Site = delegation;
                if (string.IsNullOrEmpty(query.Filter) && query.InlineQuery != true)
                    query.Filter = query.GetQuery(user);
                var items = await uow.DBRepo.GetStream(q, true);
                var targetListMeta = site.ListsMeta?[$"{q.ListName?.ToLower()}"];
                var q3 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = q.ListId, CatchError = true };
                var listFields = await uow.DBRepo.GetListColumns(q3);
                var q1 = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = q.ListId, CatchError = true };
                var views = await uow.DBRepo.GetListViews(q1);
                var targetView = views?.FirstOrDefault(a => "Dashboard".Equals(a.Title, StringComparison.OrdinalIgnoreCase));
                if (targetView == null) targetView = views?.FirstOrDefault();
                var viewFields = targetView?.Values?["Fields"]  as List<string>;
                var lines = new List<string>();
                var line = string.Empty;
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("RAC");
                var index = 1;
                var fields = listFields.Where(x => viewFields.Contains(x.UniqueId)).ToList();
                foreach (var field in fields)
                {
                    workSheet.Cells[1, index].Value = field.Title;
                    index++;
                }
                int recordIndex = 2;

                foreach (dynamic item in items.Row)
                {
                    index = 1;
                    Console.WriteLine(index);
                    try
                    {

                        foreach (var field in fields)
                        {
                            string v = "";
                            try
                            {
                                var v0 = "" + item[field.UniqueId];
                                if (!string.IsNullOrEmpty(v0))
                                {
                                    var type = "" + field.Values["Type"];
                                    switch (type)
                                    {
                                        case "DateTime":
                                            var dt0 = Convert.ToDateTime(item[$"{field.UniqueId}."]);
                                            v = dt0.ToShortDateString();
                                            break;
                                        case "Lookup":
                                            v = "" + item[field.UniqueId][0]["lookupValue"];
                                            break;
                                        case "User":
                                            v = "" + item[field.UniqueId][0]["title"];
                                            break;
                                        case "Taxonomy":
                                            v = "" + item[field.UniqueId]["Label"];
                                            break;
                                        default:
                                            v = "" + item[field.UniqueId];
                                            break;
                                    }
                                }
                            }
                            catch (Exception ex1)
                            {
                            }
                            v = v.Trim();
                            v = v.Replace(";", string.Empty);
                            workSheet.Cells[recordIndex, index].Value = v;
                            index++;
                        }
                    }
                    catch (Exception ex2)
                    {
                    }
                    recordIndex++;
                }
                index = 1;
                foreach (var field in fields)
                {
                    workSheet.Column(index).Width = 20;
                    if (""+field.Values["Type"] == "DateTime")
                    {
                        workSheet.Column(index).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                        workSheet.Column(index).Style.Numberformat.Format = "dd-MM-yyyy";
                    }
                    index++;
                }

                byte[] fileContents = excel.GetAsByteArray();
                Stream stream = new MemoryStream(fileContents);
                var dtISO = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"AIG_Export_{dtISO}.xlsx";
                result = new FileModel()
                {
                    Name= fileName,
                    ContentStream = stream,
                };
            }
            return result;
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
            DBItem? result = null;
            var wfRequest = await Get(item.ToDBQuery(), item.Site);
            if (wfRequest != null)
            {
                var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
                item.SiteId = site.Id;
                item.ListId = site.Lists?.GetStringValue2($"{item.ListName}");
                result = await uow.DBRepo.Put(item);
            }
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

        public async Task<DBStream?> Ressources(FormQuery query, string? delegation = "")
        {
            DBStream? result = null;
            var site = await siteSvc.Get(delegation) ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            if (query != null)
            {
                var qRequest = new DBQuery()
                {
                    SiteUrl = site.SiteUrl,
                    ListName = ListNameKeys.Interview,
                    Filter = $"<Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{query.ItemId}</Value></Eq></Where>"
                };
                var wfRequest = await Get(qRequest, delegation);
                if (wfRequest != null)
                {
                    var q = query;

                    q.SiteUrl = site.SiteUrl;
                    q.ListId = site.Lists?.GetStringValue2($"{q.ListName}");
                    result = await uow.DBRepo.GetStream(q);

                }
            }
            return result;
        }

        public async Task<string?> GenerateSharedLink(DBUpdate item, string fileUrl)
        {
            var site = await siteSvc.Get($"{item.Site}") ?? throw new InvalidOperationException($"Unable to find the site : {item.Site}");
            var ctx = spoContext.GetClientContext($"{site.SiteUrl}");
            var r = Microsoft.SharePoint.Client.Web.CreateOrganizationSharingLink(ctx, fileUrl, true);
            await ctx.ExecuteQueryAsync();
            var result = r.Value;
            return result;
        }

        public async Task<List<string>?> GetFilterValues(FormQuery query, string? delegation = "")
        {
            var items = new List<string>();
            var site = await siteSvc.Get($"{delegation}") ?? throw new InvalidOperationException($"Unable to find the site : {delegation}");
            query.ListUrl = $"{site.ListsMeta?[$"{query.ListName}"].RelativeUrl}";
            query.SiteUrl = site.SiteUrl;
            var result = await uow.DBRepo.GetFilterValues(query);
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace("SELECTED", "").Substring(0, result.IndexOf("</SELECT>") + 1);

                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(result);
                var select = xmlDoc.SelectSingleNode("SELECT");
                var nodes = select?.ChildNodes;
                if (nodes != null)
                {
                    foreach (XmlNode childrenNode in nodes)
                    {
                        var s = $"{childrenNode.Attributes?["Value"]?.Value}";
                        if (!string.IsNullOrEmpty(s))
                            items.Add(s);
                    }
                }
            }
            return items;
        }



    }
}