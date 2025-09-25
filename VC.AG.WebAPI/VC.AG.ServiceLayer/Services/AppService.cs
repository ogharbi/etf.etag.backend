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
using VC.AG.Models;
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
    public class AppService(IUnitOfWork uow, ISiteContract siteSvc, IMemoryCache cache, IConfiguration config) : IAppContract
    {
        readonly SpoContext spoContext = new(config, cache);
        public async Task<SiteEntity?> GetSite(string delegation = "", bool force = false)
        {
            if (force)
            {
                if (cache is MemoryCache memoryCache)
                {
                    var percentage = 1.0;//100%
                    memoryCache.Compact(percentage);
                }
            }
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
                    case Ressource.Organization:
                        result = site.Organization;
                        break;
                    case Ressource.Bus:
                        result = site.Bus;
                        break;
                    case Ressource.ActionTemplates:
                        result = site.ActionTemplates;
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


        public async Task<string?> SendReminder(DateTime? startDate, DateTime? endDate)
        {
            //var result = await notifSvc.SendReminder(startDate,endDate);
            //return $"{result}";
            return null;
        }

        public async Task<FileModel?> GetPdf(IGeneratePdf generatePdf, DBQuery qp)
        {
            FileModel? result = null;
            IDictionary<string, Object> itemPdf = new Dictionary<string, Object>();
            var relatedItems = new List<dynamic>();
            var actions = new List<dynamic>();
            var force = false;

            var site = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the site");
            var targetList = GetTargetList(qp.AppTarget);
            var relatedTargetList = GetRelatedTargetList(qp.AppTarget);
            var actionTargetList = GetActionTargetList(qp.AppTarget);
            var targetLkField = GetLkTargetField(qp.AppTarget);
            var dbQuery = new DBQuery()
            {
                ListName = targetList,
                Filter = $"<Where><Eq><FieldRef Name='ID'/><Value Type='Number'>{qp.Id}</Value></Eq></Where>"
            };
            var mainItem = await GetAll(dbQuery);
            if (mainItem != null)
            {
                var values = mainItem.Row?.FirstOrDefault();
                if (values == null) return null;
                var formType = values.GetStringValue2(InterviewKeys.FormType);
                itemPdf["FormType"] = formType;
                itemPdf["FormTarget"] = values.GetStringValue2(InterviewKeys.FormTarget);
                itemPdf["DocumentName"] = GetDocumentName(formType);
                itemPdf["Type_Aiguilleur"] = ContractType.Aiguilleur;
                itemPdf["Type_StageE"] = ContractType.StageE;
                itemPdf["Type_StageT"] = ContractType.StageT;
                itemPdf["Type_Chantier1"] = ContractType.Chantier1;
                itemPdf["Type_Chantier2"] = ContractType.Chantier2;
                itemPdf["Type_Chantier3"] = ContractType.Chantier3;
                itemPdf["Type_Condcuteur1"] = ContractType.Conducteur1;
                itemPdf["Type_Condcuteur2"] = ContractType.Conducteur2;
                itemPdf["Type_Condcuteur3"] = ContractType.Conducteur3;
                itemPdf["Type_Condcuteur4"] = ContractType.Conducteur4;
                itemPdf["Type_Condcuteur5"] = ContractType.Conducteur5;

                var templateName = GetTemplateName(formType);
                var dbFile = new DBFile()
                {
                    SiteId = site.Id,
                    DriveId = site.Drives?[ListNameKeys.DocTemplates] as string,
                    Name = templateName
                };
                DBFile? file = await uow.FileRepo.Get(dbFile, true);
                if (file != null && file.ContentStream != null)
                {
                    string htmlCacheKey = $"app-html-template-carnet-{templateName}";
                    cache.TryGetValue(htmlCacheKey, out string? html);
                    if (string.IsNullOrEmpty(html))
                    {
                        var buffer = file.ContentStream.ReadAllBytes();
                        html = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        //cache.Set(htmlCacheKey, html);
                    }
                    IDictionary<string, Object> mValues = new Dictionary<string, Object>();
                    if (values != null)
                    {
                        foreach (KeyValuePair<string, object> it in values)
                        {
                            //var keybis = $"{it.Key}.";
                            //if (values.ContainsKey(keybis) && !itemPdf.ContainsKey(keybis))
                            //{
                            //    itemPdf.Add(it.Key, values[keybis]);
                            //}
                            //else
                            if (!itemPdf.ContainsKey(it.Key))
                            {
                                itemPdf.Add(it.Key, it.Value);
                            }
                        }
                    }
                    dbQuery = new DBQuery()
                    {
                        ListName = relatedTargetList,
                        Filter = $"<Where><Eq><FieldRef Name='{targetLkField}' LookupId='True'/><Value Type='Lookup'>{qp.Id}</Value></Eq></Where>"
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
                        relatedItems.Add(qValues);
                    }

                    dbQuery = new DBQuery()
                    {
                        ListName = actionTargetList,
                        Filter = $"<Where><Eq><FieldRef Name='{targetLkField}' LookupId='True'/><Value Type='Lookup'>{qp.Id}</Value></Eq></Where>"
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
                        var subItems = actions.Where(a => EqualsNotNull("" + a["Col_Guid"], itemPdfGuid) && EqualsNotNull("" + a["Title"], ContractActionType.ObjectifMission)).ToList();
                        itemPdf.Add("Objectifs", subItems);
                        subItems = actions.Where(a => EqualsNotNull("" + a["Col_Guid"], itemPdfGuid) && EqualsNotNull("" + a["Title"], ContractActionType.StageEtudAppreciation)).ToList();
                        itemPdf.Add("StageEtudAppreciation", subItems);
                    }
                    var i = 1;
                    foreach (var qItem in relatedItems)
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
                    itemPdf.Add("RItems", relatedItems.AsEnumerable().OrderBy(x => x["Col_Order."]));
                    var stream = AppHelper.GetPdfStream(generatePdf, html, itemPdf, Wkhtmltopdf.NetCore.Options.Orientation.Portrait);
                    result = new FileModel() { Title = $"{itemPdf["Title"]}.pdf", ContentStream = stream };

                }
            }
            return result;
        }

        private string GetActionTargetList(AppTarget? appTarget)
        {
            string r = "";
            switch (appTarget)
            {
                case AppTarget.None:
                    break;
                case AppTarget.AG:
                    r = ListNameKeys.AGActions;
                    break;
                case AppTarget.CT:
                    r = ListNameKeys.CTActions;
                    break;
                default:
                    break;
            }
            return r;
        }

        private string GetTargetList(AppTarget? appTarget)
        {
            string r = "";
            switch (appTarget)
            {
                case AppTarget.None:
                    break;
                case AppTarget.AG:
                    r = ListNameKeys.Interview;
                    break;
                case AppTarget.CT:
                    r = ListNameKeys.Contract;
                    break;
                default:
                    break;
            }
            return r;
        }
        private string GetRelatedTargetList(AppTarget? appTarget)
        {
            string r = "";
            switch (appTarget)
            {
                case AppTarget.None:
                    break;
                case AppTarget.AG:
                    r = ListNameKeys.QInterview;
                    break;
                case AppTarget.CT:
                    r = ListNameKeys.Contract;
                    break;
                default:
                    break;
            }
            return r;
        }
        private string GetLkTargetField(AppTarget? appTarget)
        {
            string r = "";
            switch (appTarget)
            {
                case AppTarget.None:
                    break;
                case AppTarget.AG:
                    r = AppKeys.Lk_Request;
                    break;
                case AppTarget.CT:
                    r = AppKeys.Lk_Contract;
                    break;
                default:
                    break;
            }
            return r;
        }

        bool EqualsNotNull(string? a, string? b)
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }
        string GetDocumentName(string formType)
        {
            string r = "Bilan";
            switch (formType)
            {
                case ContractType.Aiguilleur: r = "Aiguilleur"; break;
                case ContractType.StageE: r = "BILAN STAGIAIRE – RH et Etudiant(e)"; break;
                case ContractType.StageT: r = "BILAN STAGE - Tuteur et Etudiant(e)"; break;
                case ContractType.AlternanceE: r = "BILAN ALTERNANCE – RH et Etudiant(e)"; break;
                case ContractType.AlternanceT: r = "BILAN ALTERNANCE - Tuteur et Etudiant(e)"; break;
                case ContractType.Chantier1: r = ""; break;
                case ContractType.Chantier2: r = ""; break;
                case ContractType.Chantier3: r = ""; break;
                case ContractType.Conducteur1: r = ""; break;
                case ContractType.Conducteur2: r = ""; break;
                case ContractType.Conducteur3: r = ""; break;
                case ContractType.Conducteur4: r = ""; break;
                case ContractType.Conducteur5: r = ""; break;
            }
            return r;
        }
        string GetTemplateName(string formType)
        {
            string r = "Aiguilleur.html";
            if (formType.EqualsNotNull(ContractType.StageE) || formType.EqualsNotNull(ContractType.StageE)
                || formType.EqualsNotNull(ContractType.AlternanceT) || formType.EqualsNotNull(ContractType.AlternanceE))
            {
                r = "stage.html";
            }
            else if (formType.EqualsNotNull(ContractType.Chantier1) || formType.EqualsNotNull(ContractType.Chantier2)
                || formType.EqualsNotNull(ContractType.Chantier3))
            {
                r = "chantier.html";
            }
            else if (formType.EqualsNotNull(ContractType.Conducteur1) || formType.EqualsNotNull(ContractType.Conducteur2)
                || formType.EqualsNotNull(ContractType.Conducteur2) || formType.EqualsNotNull(ContractType.Conducteur4))
            {
                r = "conducteur.html";
            }
            return r;
        }
    }
}
