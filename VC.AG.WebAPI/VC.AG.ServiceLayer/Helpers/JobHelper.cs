using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Helpers;
using VC.AG.Models;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;
using VC.AG.Models.Extensions;
using Azure.Core;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using System.Dynamic;
using Microsoft.SharePoint.News.DataModel;

namespace VC.AG.ServiceLayer.Helpers
{
    public class JobHelper(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, IUserContract userSvc, ISiteContract siteSvc, IAppContract appSvc)
    {
        private const string title = "Title";

        readonly IUnitOfWork uow = uow;
        readonly IAppContract appSvc = appSvc;
        readonly ISiteContract siteSvc = siteSvc;
        readonly IUserContract userSvc = userSvc;
        readonly IConfiguration config = config;
        readonly SpoContext spoContext = new(config, cache);
        readonly GraphContext graphContext = new(config, cache);
        public async Task<List<MailReminder>> GetInterviewsToStart(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{InterviewKeys.StartDateT}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{startDate.Value.ToString("s").Split('T')[0]}T00:00:00Z</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{InterviewKeys.StartDateT}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{endDate.Value.ToString("s").Split('T')[0]}T23:59:00Z</Value></Lt>");
                }
                ops.Add($"<IsNull><FieldRef Name='{InterviewKeys.NotifDate}'/></IsNull>");

                ops.Add($"<Or>" +
                    $"<Eq><FieldRef Name='{InterviewKeys.Status}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>" +
                    $"<IsNull><FieldRef Name='{InterviewKeys.Status}'/></IsNull>" +
                    $"</Or>");
                var filterOps = AppHelper.BuildQuery(ops, "And");
                string v = @$"<Where>{filterOps}</Where><OrderBy><FieldRef Name='{InterviewKeys.StartDateT}' Ascending='False' /></OrderBy>";
                var q = new DBQuery()
                {
                    SiteUrl = site?.SiteUrl,
                    ListId = $"{site?.Lists?[ListNameKeys.Interview.ToLower()]}",
                    Filter = v,
                    Top = 1000
                };
                var resultRequests = await uow.DBRepo.GetStream(q, true);

                if (resultRequests != null && resultRequests.Row != null)
                {
                    foreach (var row in resultRequests.Row)
                    {
                        var itemId = row.GetIntValue2("ID");
                        try
                        {
                            var aigUser = row.GetStreamUserValue2(InterviewKeys.Responsible);
                            if (aigUser != null)
                            {
                                var title = row.GetStringValue2("Title");
                                var dueDate = row.GetDateTimeValue2(QInterviewKeys.DueDate + ".");
                                var sDateTutorat = row.GetDateTimeValue2(QInterviewKeys.StartDate + ".");
                                var status = row.GetStringValue2(QInterviewKeys.Status);
                                var user = await userSvc.GetById(aigUser.Id);
                                if (user != null)
                                {
                                    var userEmail = user.Email;
                                    var userName = user.DisplayName;
                                    result.Add(new MailReminder()
                                    {
                                        Title = title,
                                        DueDate = dueDate,
                                        StartDate = sDateTutorat,
                                        UserEmail = $"{userEmail}".ToLower(),
                                        UserName = userName,
                                        Status = status,
                                        itemId = itemId,
                                        RequestId = itemId,
                                        Values = row
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            //.AsEnumerable().OrderBy(x => x["Col_Order."]));
            return result;
        }
        public async Task<List<MailReminder>> GetQInterviewsToStart(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{startDate.Value.ToString("s").Split('T')[0]}T00:00:00Z</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{endDate.Value.ToString("s").Split('T')[0]}T23:59:00Z</Value></Lt>");
                }
                ops.Add($"<IsNull><FieldRef Name='{InterviewKeys.NotifDate}'/></IsNull>");

                ops.Add($"<Or>" +
                    $"<Eq><FieldRef Name='{QInterviewKeys.Status}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>" +
                    $"<IsNull><FieldRef Name='{QInterviewKeys.Status}'/></IsNull>" +
                    $"</Or>");
                var filterOps = AppHelper.BuildQuery(ops, "And");
                string v = @$"<Where>{filterOps}</Where><OrderBy><FieldRef Name='{QInterviewKeys.DueDate}' Ascending='False' /></OrderBy>";
                var q = new DBQuery()
                {
                    SiteUrl = site?.SiteUrl,
                    ListId = $"{site?.Lists?[ListNameKeys.QInterview.ToLower()]}",
                    Filter = v,
                    Top = 1000
                };
                var resultRequests = await uow.DBRepo.GetStream(q, true);

                if (resultRequests != null && resultRequests.Row != null)
                {
                    foreach (var row in resultRequests.Row)
                    {
                        var itemId = row.GetIntValue2("ID");
                        try
                        {
                            var aigUser = row.GetIntValue2(QInterviewKeys.AigId);
                            if (aigUser != null)
                            {
                                var title = row.GetStringValue2("Title");
                                var dueDate = row.GetDateTimeValue2(QInterviewKeys.DueDate + ".");
                                var sDate = row.GetDateTimeValue2(QInterviewKeys.StartDate + ".");
                                var status = row.GetStringValue2(QInterviewKeys.Status);
                                var user = await userSvc.GetById(aigUser);
                                if (user != null)
                                {
                                    var userEmail = user.Email;
                                    var userName = user.DisplayName;
                                    result.Add(new MailReminder()
                                    {
                                        Title = title,
                                        DueDate = dueDate,
                                        StartDate = sDate,
                                        UserEmail = $"{userEmail}".ToLower(),
                                        UserName = userName,
                                        Status = status,
                                        itemId = itemId,
                                        RequestId = row.GetStreamLookupValue2(AppKeys.Lk_Request)?.LookupId,
                                        Values = row
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            //.AsEnumerable().OrderBy(x => x["Col_Order."]));
            return result;
        }
        public async Task<List<MailReminder>> GetQInterviewsNotStarted(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{startDate.Value.ToString("s").Split('T')[0]}T00:00:00Z</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{endDate.Value.ToString("s").Split('T')[0]}T23:59:00Z</Value></Lt>");
                }

                ops.Add($"<Or>" +
                    $"<Eq><FieldRef Name='{QInterviewKeys.Status}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>" +
                    $"<IsNull><FieldRef Name='{QInterviewKeys.Status}'/></IsNull>" +
                    $"</Or>");
                var filterOps = AppHelper.BuildQuery(ops, "And");
                string v = @$"<Where>{filterOps}</Where><OrderBy><FieldRef Name='{QInterviewKeys.DueDate}' Ascending='False' /></OrderBy>";
                var q = new DBQuery()
                {
                    SiteUrl = site?.SiteUrl,
                    ListId = $"{site?.Lists?[ListNameKeys.QInterview.ToLower()]}",
                    Filter = v,
                    Top = 1000
                };
                var resultRequests = await uow.DBRepo.GetStream(q, true);

                if (resultRequests != null && resultRequests.Row != null)
                {
                    foreach (var row in resultRequests.Row)
                    {
                        var itemId = row.GetIntValue2("ID");
                        try
                        {
                            var aigUser = row.GetIntValue2(QInterviewKeys.AigId);
                            if (aigUser != null)
                            {
                                var title = row.GetStringValue2("Title");
                                var dueDate = row.GetDateTimeValue2(QInterviewKeys.DueDate + ".");
                                var notifDate = row.GetDateTimeValue2(QInterviewKeys.NotifDate + ".");
                                if (notifDate == null || notifDate.GetValueOrDefault().Date < dueDate.GetValueOrDefault().Date) notifDate = dueDate;
                                var sDate = row.GetDateTimeValue2(QInterviewKeys.StartDate + ".");
                                var status = row.GetStringValue2(QInterviewKeys.Status);
                                var user = await userSvc.GetById(aigUser);
                                double daysDiff =Math.Floor(Math.Abs((endDate.Value - notifDate.Value).TotalDays));
                                bool isFullWeek = daysDiff % 7 == 0;
                                if (user != null && isFullWeek)
                                {
                                    var userEmail = user.Email;
                                    var userName = user.DisplayName;
                                    result.Add(new MailReminder()
                                    {
                                        Title = title,
                                        DueDate = dueDate,
                                        StartDate = sDate,
                                        NotifDate = notifDate,
                                        UserEmail = $"{userEmail}".ToLower(),
                                        UserName = userName,
                                        Status = status,
                                        itemId = itemId,
                                        RequestId = row.GetStreamLookupValue2(AppKeys.Lk_Request)?.LookupId,
                                        Values = row
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            //.AsEnumerable().OrderBy(x => x["Col_Order."]));
            return result;
        }

        public async Task<string> SendReminder(List<MailReminder> items, SiteEntity rootSite, MailType mailType, DateTime? endDate)
        {
            var result = string.Empty;
            var allowedReminder = items;
            var distinctTos = items.Select(a => $"{a.UserEmail}".ToLower()).ToList();
            var mailTemplate = rootSite?.MailTemplates?.FirstOrDefault(a => mailType.ToString().EqualsNotNull(a.Values?.GetStringValue2(AppConstants.AppKeys.Code)));
            if (mailTemplate != null)
            {
                foreach (var item in items)
                {
                    try
                    {
                        var email = $"{item.UserEmail}";

                        var subject = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Subject);
                        var body = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Body);
                        var appUrl = $"{config.GetValue<string>(AppSettingsKeys.AppUrl)}";
                        var appLink = $"<a href=\"{appUrl}\">{appUrl}</a>";
                        var itemLink = $"<a href=\"{appUrl}/forms/{item.RequestId}?d={AppTarget.AG}\">Fiche aiguilleur N° : {item.RequestId}</a>";
                        subject = subject?.Replace("[AppLink]", appLink);
                        body = "" + body?.Replace("[AppLink]", appLink);
                        body = "" + body?.Replace("[ItemLink]", itemLink);
                        body = "" + body?.Replace("[EndDate]", endDate?.ToString("dd/MM/yyyy"));
                        subject = CompileText(subject, item);
                        body = CompileText(body, item);
                        var summury = new StringBuilder();
                        var tos = new List<string>()
                                    {
                                      email
                                    };
                        var mailObject = new MailObject()
                        {
                            MailTo = tos,
                            Subject = subject,
                            Body = body
                        };
                        var itemId = item.itemId.GetValueOrDefault();
                        string targetList = "";
                        switch (mailType)
                        {
                            case MailType.InterviewToStartReminder: targetList = ListNameKeys.Interview; break;
                            case MailType.QInterviewToStartReminder: targetList = ListNameKeys.QInterview; break;
                            case MailType.QInterviewNotStartedReminder: targetList = ListNameKeys.QInterview; break;
                        }
                        await SendNotification(rootSite, mailObject, targetList, itemId, config, graphContext);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return result;
        }
        public async Task<List<MailReminder>> GetEntretiensInProgress(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{startDate.Value.ToString("s")}</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='FALSE' Type='DateTime'>{endDate.Value.ToString("s")}</Value></Lt>");
                }
                ops.Add($"<Or>" +
                    $"<Eq><FieldRef Name='{InterviewKeys.Status}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>" +
                    $"<Eq><FieldRef Name='{InterviewKeys.Status}'/><Value Type='Text'>{RequestStatusStr.InProgress}</Value></Eq>" +
                    $"</Or>");
                var filterOps = AppHelper.BuildQuery(ops, "And");
                string v = @$"<Where>
                                           {filterOps} 
                                      </Where><OrderBy><FieldRef Name='{QInterviewKeys.DueDate}' Ascending='False' /></OrderBy>";
                var q = new DBQuery()
                {
                    SiteUrl = site?.SiteUrl,
                    ListId = $"{site?.Lists?[ListNameKeys.QInterview.ToLower()]}",
                    Filter = v,
                    Top = 2000
                };
                var resultRequests = await uow.DBRepo.GetStream(q, true);

                if (resultRequests != null && resultRequests.Row != null)
                {
                    foreach (var row in resultRequests.Row)
                    {
                        var itemId = row.GetIntValue2("ID");
                        try
                        {
                            var aigUserId = row.GetIntValue2(QInterviewKeys.AigId);
                            if (aigUserId != null)
                            {
                                var title = row.GetStringValue2("Title");
                                var dueDate = row.GetDateTimeValue2(QInterviewKeys.DueDate + ".");
                                var sDate = row.GetDateTimeValue2(QInterviewKeys.StartDate + ".");
                                var status = row.GetStringValue2(QInterviewKeys.Status);
                                var user = await userSvc.GetById(aigUserId);
                                var index = row.GetIntValue2($"{AppKeys.Order}.");
                                var entretiensCount = row.GetIntValue2(QInterviewKeys.EntretienCount);
                                if (user != null)
                                {
                                    var userEmail = user.Email;
                                    var userName = user.DisplayName;
                                    result.Add(new MailReminder()
                                    {
                                        Title = title,
                                        DueDate = dueDate,
                                        StartDate = sDate,
                                        UserEmail = $"{userEmail}".ToLower(),
                                        UserName = userName,
                                        Status = status,
                                        itemId = itemId,
                                        Index = index,
                                        EntretiensCount = entretiensCount,
                                        RequestId = row.GetStreamLookupValue2(AppKeys.Lk_Request)?.LookupId
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            //.AsEnumerable().OrderBy(x => x["Col_Order."]));
            return result;
        }

        static string CompileText(string body, MailReminder reminder)
        {
            var values = reminder.Values;
            if (values != null)
            {
                foreach (KeyValuePair<string, object> item in values)
                {
                    try
                    {
                        var value = item.Value;
                        body = body.Replace($"[{item.Key}]", "" + value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
            body = body.Replace("00:00", "");
            return body;
        }
        static void UpdateInterviewBodyHtml(ref StringBuilder summury, List<MailReminder>? waitingItems, string appUrl, AppTarget appTarget)
        {
            if (waitingItems != null)
            {
                summury.Append("<style>#tabDetails {border-collapse: collapse;}#tabDetails th,#tabDetails td { border:1px solid;  }</style>");
                UpdateInterviewTableDetails(ref summury, waitingItems, appUrl, appTarget);
            }
        }
        static void UpdateInterviewTableDetails(ref StringBuilder summury, List<MailReminder>? waitingItems, string appUrl, AppTarget appTarget)
        {
            if (waitingItems.Count > 0)
            {
                summury.Append($@"<table id=""tabDetails"">");
                summury.Append($@"<tr>
                                <th style='padding:5px'>Fiche ID</th>
                                <th style='padding:5px'>Titre</th>
                                <th style='padding:5px'>Statut</th>
                                <th style='padding:5px'>Date prévue</th>
                                </tr>");

                foreach (var item in waitingItems)
                {
                    var wfUrl = $"{appUrl.TrimEnd('/')}/forms/{item.RequestId}?t={item.Index}&d={appTarget}";
                    summury.Append(@$"<tr>
                                        <td style='padding:5px'>{item.RequestId}</td>
                                        <td style='padding:5px'><a href=""{wfUrl}"">{item.Title}</a></td>
                                        <td style='padding:5px'>{item.Status}</td>
                                        <td style='padding:5px'>{item.DueDate?.ToString("dd/MM/yyyy")}</td>
                                    </tr>");

                }
                summury.Append("</table>");
            }
        }
        static void UpdateBodyHtml(ref StringBuilder summury, List<MailReminder>? waitingItems, string appUrl, AppTarget appTarget)
        {
            if (waitingItems != null)
            {
                summury.Append("<style>#tabDetails {border-collapse: collapse;}#tabDetails th,#tabDetails td { border:1px solid;  }</style>");
                var firstEnts = waitingItems.Where(a => a.Index == 1).ToList();
                var lastEnts = waitingItems.Where(a => a.Index == a.EntretiensCount).ToList();
                var middleEnts = waitingItems.Where(a => !firstEnts.Any(b => b.itemId == a.itemId) && !lastEnts.Any(b => b.itemId == a.itemId)).ToList();
                UpdateTableDetails(ref summury, "Premiers entretiens à réaliser : Auto-évaluation + 1er entretien", firstEnts, appUrl, appTarget);
                UpdateTableDetails(ref summury, "Entretiens trimestiels à réaliser", middleEnts, appUrl, appTarget);
                UpdateTableDetails(ref summury, "Entretiens finaux à réaliser : Entretien final + bilan final", lastEnts, appUrl, appTarget);
            }
        }
        static void UpdateTableDetails(ref StringBuilder summury, string header, List<MailReminder>? waitingItems, string appUrl, AppTarget appTarget)
        {
            if (waitingItems.Count > 0)
            {
                summury.Append($"<ul><li style=\"text-decoration:underline\">{header}</li></ul>");
                summury.Append($@"<table id=""tabDetails"">");
                summury.Append($@"<tr>
                                <th style='padding:5px'>Fiche ID</th>
                                <th style='padding:5px'>Titre</th>
                                <th style='padding:5px'>Statut</th>
                                <th style='padding:5px'>Date prévue</th>
                                </tr>");

                foreach (var item in waitingItems)
                {
                    var wfUrl = $"{appUrl.TrimEnd('/')}/forms/{item.RequestId}?t={item.Index}&d={appTarget}";
                    summury.Append(@$"<tr>
                                        <td style='padding:5px'>{item.RequestId}</td>
                                        <td style='padding:5px'><a href=""{wfUrl}"">{item.Title}</a></td>
                                        <td style='padding:5px'>{item.Status}</td>
                                        <td style='padding:5px'>{item.DueDate?.ToString("dd/MM/yyyy")}</td>
                                    </tr>");

                }
                summury.Append("</table>");
            }
        }
        private async Task SendNotification(SiteEntity? rootSite, MailObject mailObject, string targetList, int itemId, IConfiguration config, GraphContext graphContext)
        {

            dynamic data = new ExpandoObject();
            var localDate=DateTime.Now;
            // Convert to UTC and set time to 00:00:00
            DateTime utcMidnight = new DateTime(
                localDate.Year,
                localDate.Month,
                localDate.Day,
                0, 0, 0,
                DateTimeKind.Utc
            );

            data.fields = new { Col_NotifDate = utcMidnight.ToString("s") };
            var d = new DBUpdate()
            {
                Id = itemId,
                ListName = targetList,
                SiteId = rootSite.Id,
                Data = data
            };
            var r = appSvc.Put(d).Result;
            var sendResult = await mailObject.Send(config, graphContext);

        }
        public async Task<string> NotifyUser(SiteEntity? rootSite, WfRequest? request, NotifQuery notifQuery)
        {
            var result = string.Empty;
            var email = "";// request?.Values?.GetStringValue2(InterviewKeys.Responsible);
            var mailTemplate = rootSite?.MailTemplates?.FirstOrDefault(a => notifQuery.Type.ToString().EqualsNotNull(a.Values?.GetStringValue2(AppConstants.AppKeys.Code)));
            switch (notifQuery.Type)
            {
                case NotifType.AskForAction: email = request.Values?.GetUserValue4(InterviewKeys.Responsible)?.Email; break;
                case NotifType.Assign: email = request.Values?.GetUserValue4(InterviewKeys.Responsible)?.Email; break;
            }
            if (mailTemplate != null && !string.IsNullOrEmpty(email))
            {
                var subject = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Subject);
                var body = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Body);
                var comment = notifQuery.Comment;
                subject = UpdateHtml(notifQuery.AppTarget, subject, request, comment);
                body = UpdateHtml(notifQuery.AppTarget, body, request, comment);

                var summury = new StringBuilder();
                var tos = new List<string>()
                                    {
                                      email
                                    };
                var mailObject = new MailObject()
                {
                    MailTo = tos,
                    Subject = subject,
                    Body = body
                };
                result = await mailObject.Send(config, graphContext);
            }
            return result;
        }
        string? UpdateHtml(AppTarget? target, string? body, WfRequest? request, string? comment)
        {
            var appUrl = $"{config.GetValue<string>(AppSettingsKeys.AppUrl)}";
            var appLink = $"<a href=\"{appUrl}?d={target}\">{appUrl}</a>";
            var itemLink = $"<a href=\"{appUrl}/forms/{request?.Id}?d={target}\">Fiche aiguilleur N° : {request.Id}</a>";
            body = body?.Replace("[AppLink]", appLink);
            body = body?.Replace("[ItemLink]", itemLink);
            body = body?.Replace("[ID]", request?.Id);
            body = body?.Replace("[Comment]", comment);
            body = body?.Replace("[ID]", request?.Id);
            if (request?.Values != null)
            {
                foreach (KeyValuePair<string, object> r in request.Values)
                {
                    body = body?.Replace($"[{r.Key}]", $"{r.Value}");
                }
            }
            return body;

        }



    }
}
