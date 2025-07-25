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
    public class NotifService(IUnitOfWork uow, IConfiguration config, IMemoryCache cache, IUserContract userSvc,ISiteContract siteSvc) : INotifContract
    {
       readonly JobHelper jobHelper = new(uow, config, cache,userSvc, siteSvc);
        readonly GraphContext graphContext = new(config, cache);

      

        public async Task<bool> SendReminder(DateTime? startDate, DateTime? endDate)
        {
            var result = true;
            var rootSite = await siteSvc.Get() ?? throw new InvalidOperationException($"Unable to find the root site");
            List<MailReminder> items = await jobHelper.GetWfInProgress(rootSite, startDate, endDate);
            var reminderList = $"{config.GetValue<string>(AppSettingsKeys.AppReminderList)}";
            await jobHelper.SendReminder(items, reminderList, rootSite, endDate);
            return result;
        }
        public async Task<bool> SendNotifications(SiteEntity? rootSite, WfRequest? request, string? comment)
        {
            await jobHelper.SendNotification(rootSite,request,comment);
            return true;
        }


    }
}
