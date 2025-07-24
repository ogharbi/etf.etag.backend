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
    public class JobHelper(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, IUserContract userSvc,ISiteContract siteSvc)
    {
        private const string title = "Title";

        readonly IUnitOfWork uow = uow;
        readonly ISiteContract siteSvc = siteSvc;
        readonly IUserContract userSvc = userSvc;
        readonly IConfiguration config = config;
        //readonly SpoContext spoContext = new(config, cache);
        //readonly GraphContext graphContext = new(config, cache);
        //readonly JobHelper jobHelper = new(uow, config, cache, siteSvc);
        public async Task<List<MailReminder>> GetWfInProgress(SiteEntity rootSite, DateTime? startDate, DateTime? endDate)
        {
            List<MailReminder> result = [];
            var site = await siteSvc.Get();
            if (site != null)
            {
                var ops = new List<string>();

                if (startDate.HasValue)
                {
                    ops.Add($"<Gt><FieldRef Name='{RequestKeys.DueDate}'/><Value IncludeTimeValue='TRUE' Type='DateTime'>{startDate.Value.ToString("s")}</Value></Gt>");
                }
                if (endDate.HasValue)
                {
                    ops.Add($"<Lt><FieldRef Name='{RequestKeys.DueDate}'/><Value IncludeTimeValue='TRUE' Type='DateTime'>{endDate.Value.ToString("s")}</Value></Lt>");
                }
                ops.Add($"<Eq><FieldRef Name='{RequestKeys.WfStatus}'/><Value Type='Text'>{RequestStatusStr.NotStarted}</Value></Eq>");
                var filterOps = AppHelper.BuildQuery(ops, "And");
                string v = @$"<Where>
                                           {filterOps} 
                                      </Where>";
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
                            var aigUserId = row.GetIntValue2("F_x003a_Aiguilleur_x0020_ID");
                            var user = userSvc.GetById(aigUserId);
                            //var to = row.GetStringValue2(AppConstants.RequestKeys.WfCurrent);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return result;
        }
        public async Task<string> SendReminder(List<MailReminder> items, string reminderList, SiteEntity rootSite, ILogger logger)
        {
            var result = string.Empty;
            var allowedReminder = items;
            var distinctTos = allowedReminder.GroupBy(a => a.To).ToList();
            AppHelper.LogEntry(logger, $"distinctTos count : {distinctTos.Count} ...");
            foreach (var to in distinctTos)
            {
                try
                {
                    var waitingItems = items.Where(a => a.To.EqualsNotNull(to.Key)).ToList();
                    var email = $"{to.Key}";
                    var mailTemplate = rootSite?.MailTemplates?.FirstOrDefault(a => MailType.Reminder.ToString().EqualsNotNull(a.Values?.GetStringValue2(AppConstants.AppKeys.Code)));
                    AppHelper.LogEntry(logger, $"Mail Template ID : {mailTemplate?.Id} ...");
                    if (mailTemplate != null)
                    {
                        var subject = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Subject);
                        var body = mailTemplate.Values?.GetStringValue2(MailTemplateKeys.Body);
                        var appUrl = $"{config.GetValue<string>(AppSettingsKeys.AppUrl)}";
                        var appLink = $"<a href=\"{appUrl}\">{appUrl}</a>";
                        var count = waitingItems?.Count;
                        subject = subject?.Replace("[Link]", appLink).Replace("[NB]", $"{count}");
                        body = body?.Replace("[Link]", appLink).Replace("[NB]", $"{count}");
                        await Console.Out.WriteLineAsync($"{to} : {count} items");

                        var summury = new StringBuilder();
                        //UpdateBodyHtml(ref summury, waitingItems, translations, locale, appUrl);
                        body = body?.Replace("[Details]", summury.ToString());
                        //await UpdateWfItems(spoContext, waitingItems, email, reminderList);
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
                        //await SendNotifications(mailObject, reminderList, email, waitingItems, config, graphContext);
                        AppHelper.LogEntry(logger, $"Send reminder : To : {string.Join(';', tos)} , waiting items : {count} ");
                    }
                }
                catch (Exception ex)
                {
                    AppHelper.LogEntry(logger, ex.Message, LogType.Error);
                }
            }
            return result;
        }



    }
}
