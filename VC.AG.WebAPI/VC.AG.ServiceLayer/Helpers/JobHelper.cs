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

namespace VC.AG.ServiceLayer.Helpers
{
    public class JobHelper(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, IUserContract userSvc, ISiteContract siteSvc)
    {
        private const string title = "Title";

        readonly IUnitOfWork uow = uow;
        readonly ISiteContract siteSvc = siteSvc;
        readonly IUserContract userSvc = userSvc;
        readonly IConfiguration config = config;
        readonly SpoContext spoContext = new(config, cache);
        readonly GraphContext graphContext = new(config, cache);
        public async Task<List<MailReminder>> GetWfInProgress(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='TRUE' Type='DateTime'>{startDate.Value.ToString("s")}</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{QInterviewKeys.DueDate}'/><Value IncludeTimeValue='TRUE' Type='DateTime'>{endDate.Value.ToString("s")}</Value></Lt>");
                }
                ops.Add($"<Or>" +
                    $"<Eq><FieldRef Name='{InterviewKeys.WfStatus}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>" +
                    $"<Eq><FieldRef Name='{InterviewKeys.WfStatus}'/><Value Type='Text'>{RequestStatusStr.InProgress}</Value></Eq>" +
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
        public async Task<string> SendReminder(List<MailReminder> items, string reminderList, SiteEntity rootSite, DateTime? endDate)
        {
            var result = string.Empty;
            var allowedReminder = items;
            var distinctTos = allowedReminder.GroupBy(a => a.UserEmail).ToList();
            foreach (var to in distinctTos)
            {
                try
                {
                    var waitingItems = items.Where(a => a.UserEmail.EqualsNotNull(to.Key)).ToList();
                    var email = $"{to.Key}";
                    var mailTemplate = rootSite?.MailTemplates?.FirstOrDefault(a => MailType.Reminder.ToString().EqualsNotNull(a.Values?.GetStringValue2(AppConstants.AppKeys.Code)));
                    if (mailTemplate != null)
                    {
                        var subject = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Subject);
                        var body = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Body);
                        var appUrl = $"{config.GetValue<string>(AppSettingsKeys.AppUrl)}";
                        var appLink = $"<a href=\"{appUrl}\">{appUrl}</a>";
                        var count = waitingItems?.Count;
                        subject = subject?.Replace("[AppLink]", appLink).Replace("[NB]", $"{count}");
                        body = body?.Replace("[AppLink]", appLink).Replace("[NB]", $"{count}");
                        body = body?.Replace("[EndDate]", endDate?.ToString("dd/MM/yyyy"));
                        await Console.Out.WriteLineAsync($"{to} : {count} items");
                        var summury = new StringBuilder();
                        UpdateBodyHtml(ref summury, waitingItems, appUrl);
                        body = body?.Replace("[Details]", summury.ToString());
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
                        await SendNotifications(mailObject, reminderList, email, waitingItems, config, graphContext);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }
        private async Task SendNotifications(MailObject mailObject, string reminderList, string email, List<MailReminder>? waitingItems, IConfiguration config, GraphContext graphContext)
        {
            if (!string.IsNullOrEmpty(reminderList) || reminderList.Contains(email, StringComparison.CurrentCultureIgnoreCase))
            {
                var sendResult = await mailObject.Send(config, graphContext);
                var sitesList = waitingItems?.GroupBy(a => a.Site).Select(a => a.FirstOrDefault()?.Site).ToList();
                var allSites = sitesList != null ? String.Join(',', sitesList) : string.Empty;
            }
        }
        static void UpdateBodyHtml(ref StringBuilder summury, List<MailReminder>? waitingItems, string appUrl)
        {
            if (waitingItems != null)
            {
                summury.Append("<style>#tabDetails {border-collapse: collapse;}#tabDetails th,#tabDetails td { border:1px solid;  }</style>");
                summury.Append($@"<table id=""tabDetails"">");
                summury.Append($@"<tr>
                                                        <th style='padding:5px'>ID Fiche</th>
                                                        <th style='padding:5px'>Titre</th>
                                                        <th style='padding:5px'>Statut</th>
                                                        <th style='padding:5px'>Date prévue</th>
                                                        <th style='padding:5px'>Date d'entretien</th>
                                                      </tr>");

                foreach (var item in waitingItems)
                {
                    var wfUrl = $"{appUrl.TrimEnd('/')}/forms/{item.RequestId}";
                    summury.Append(@$"<tr>
                                                            <td style='padding:5px'>{item.RequestId}</td>
                                                            <td style='padding:5px'><a href=""{wfUrl}"">{item.Title}</a></td>
                                                            <td style='padding:5px'>{item.Status}</td>
                                                            <td style='padding:5px'>{item.DueDate?.ToString("dd/MM/yyyy")}</td>
                                                            <td style='padding:5px'>{item.StartDate?.ToString("dd/MM/yyyy")}</td>
                                                        </tr>");

                }
                summury.Append("</table>");
            }
        }
        public async Task<string> SendNotification(SiteEntity rootSite, WfRequest request, string? comment)
        {
            var result = string.Empty;
            var email = request.Values?.GetStringValue2("Col_AgUser.email");
            var mailTemplate = rootSite?.MailTemplates?.FirstOrDefault(a => MailType.Information.ToString().EqualsNotNull(a.Values?.GetStringValue2(AppConstants.AppKeys.Code)));
            if (mailTemplate != null && !string.IsNullOrEmpty(email)
            {
                var subject = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Subject);
                var body = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Body);
                var appUrl = $"{config.GetValue<string>(AppSettingsKeys.AppUrl)}";
                var appLink = $"<a href=\"{appUrl}\">{appUrl}</a>";
                subject = subject?.Replace("[AppLink]", appLink).Replace("[ID]", request.Id);
                body = body?.Replace("[AppLink]", appLink).Replace("[ID]", request.Id);
                body = body?.Replace("[Comment]",comment);
                var summury = new StringBuilder();
                body = body?.Replace("[Details]", summury.ToString());
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
                var sendResult = await mailObject.Send(config, graphContext);
            }
            return result;
        }



    }
}
