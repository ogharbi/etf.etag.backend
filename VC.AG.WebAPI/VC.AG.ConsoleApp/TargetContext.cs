using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject.SPContext;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.ServiceLayer.Services;

namespace VC.AG.ConsoleApp
{
    internal class TargetContext
    {
        ClientContext ctx;
        SpoContext spoContext;
        readonly ISiteContract siteSvc;
        readonly IUserContract userSvc;
        readonly INotifContract notifSvc;
        readonly IAppContract appSvc;
        readonly IFormContract formSvc;
        readonly Dictionary<string, string> Sites;
        readonly IUnitOfWork uow;
        readonly IMemoryCache cache;
        readonly string? delegation;
        readonly SiteEntity? RootSite;
        ILogger logger;
        string siteUrl;
        public TargetContext(IConfiguration config, IMemoryCache cache, string url, string? subsite = null, bool? loadSite = true)
        {

            var rootUrl = url;
            url = string.IsNullOrEmpty(subsite) ? url : $"{url}/{subsite}";

            siteUrl = url;
            Console.WriteLine($"Start target : {url} -> {DateTime.Now}");
            this.cache = cache;
            uow = new UnitOfWork(config, cache);
            spoContext = new SpoContext(config, cache);
            ctx = spoContext.GetClientContext(url);
            siteSvc = new SiteService(uow, cache, null);
            notifSvc = new NotifService(uow, config, cache, userSvc,siteSvc);
            userSvc = new UserService(uow, cache, siteSvc, null);
            formSvc = new FormService(uow, cache, config, siteSvc, userSvc);
            appSvc = new AppService(uow, siteSvc, cache, config);
            delegation = subsite;
          
            RootSite = siteSvc.Get(null, false).Result;
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            logger = loggerFactory.CreateLogger<Program>();
            Console.WriteLine($"End target : {url} -> {DateTime.Now}");
        }
        public async Task<string> JobDebug()
        {
            string result = string.Empty;
            //await notifSvc.SendReminder(logger);
            var s = new DateTime(2026, 01, 01);
            var e = new DateTime(2027, 01, 01);
            await notifSvc.SendReminder(s,e);
            return result;
        }
    }
}
