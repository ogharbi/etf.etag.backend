using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;
using Microsoft.IdentityModel.Tokens;
using VC.AG.Models.Helpers;
using VC.AG.ServiceLayer.Helpers;
using Microsoft.Office.SharePoint.Tools;
using Microsoft.SharePoint.Client;
using VC.AG.Models;
using Microsoft.Graph;
using Microsoft.SharePoint.News.DataModel;
using Microsoft.Extensions.Azure;
using VC.AG.Models.ValuesObject.SPContext;
using System.Security.Claims;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
namespace VC.AG.ServiceLayer.Services
{
    public class NotifService(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, IUserContract userSvc, ISiteContract siteSvc, IAppContract appSvc) : INotifContract
    {
        readonly JobHelper jobHelper = new(uow, config, cache, userSvc, siteSvc, appSvc);
        readonly GraphContext graphContext = new(config, cache);

        public async Task<bool> SendInterviewsToStartReminder()
        {
            var result = true;
            //Keep now becaause we check notif date empty also
            var baseDate = DateTime.UtcNow.AddDays(15);
            var startDate = baseDate; // new DateTime(2025, 11, 15);//
            var endDate = startDate;
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetInterviewsToStart(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            await jobHelper.SendReminder(items, rootSite, MailType.InterviewToStartReminder, endDate);
            return result;
        }
        public async Task<bool> SendInterviewsNotStartedReminder()
        {
            var result = true;
            //Keep now becaause we check notif date empty also
            var baseDate = DateTime.UtcNow.AddDays(-15);
            var endDate = baseDate;// new DateTime(2025, 11, 14);
            var startDate = endDate;
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetInterviewsNotStarted(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            await jobHelper.SendReminder(items, rootSite, MailType.InterviewNotStartedReminder, endDate);
            return result;
        }
        public async Task<bool> SendQInterviewsToStartReminder()
        {
            var result = true;
            //Keep now becaause we check notif date empty also
            var baseDate = DateTime.UtcNow.AddDays(15);
            var startDate = baseDate; // new DateTime(2025, 11, 15);
            var endDate = startDate;
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetQInterviewsToStart(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            await jobHelper.SendReminder(items, rootSite, MailType.QInterviewToStartReminder, endDate);
            return result;
        }
        public async Task<bool> SendQInterviewsNotStartReminder()
        {
            var result = true;
            var baseDate = DateTime.UtcNow;
            var endDate = baseDate;
            // Every week jusqu'a 1 mois
            var startDate = endDate.AddMonths(-1);
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetQInterviewsNotStarted(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            await jobHelper.SendReminder(items, rootSite, MailType.QInterviewNotStartedReminder, startDate);
            return result;
        }

        public async Task<bool> SendReminder(DateTime? startDate, DateTime? endDate)
        {
            var result = true;
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetEntretiensInProgress(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            //await jobHelper.SendReminder(items, reminderList, rootSite, endDate);
            return result;
        }

        public async Task<string> NotifyUser(SiteEntity? rootSite, WfRequest? request, NotifQuery notifQuery)
        {
            var result = await jobHelper.NotifyUser(rootSite, request, notifQuery);
            return result;
        }


    }
}
