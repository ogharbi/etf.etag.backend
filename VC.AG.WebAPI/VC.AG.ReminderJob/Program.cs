// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using VC.AG.DAO.UnitOfWork;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.ServiceLayer.Services;

Console.WriteLine("Hello, World!");
IConfiguration config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables().AddUserSecrets("b164e22c-af5a-45f2-99c7-ef156126118b")
.Build();

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
IUnitOfWork uow = new UnitOfWork(config,cache);
ISiteContract siteSvc = new SiteService(uow, cache, null);
IUserContract userSvc = new UserService(uow, cache, siteSvc, null);
IAppContract appSvc = new AppService(uow, siteSvc, cache, config);
INotifContract notifSvc = new NotifService(uow, config, cache, userSvc, siteSvc,appSvc);

await notifSvc.SendInterviewsToStartReminder();
await notifSvc.SendInterviewsNotStartedReminder();
await notifSvc.SendQInterviewsToStartReminder();
await notifSvc.SendQInterviewsNotStartReminder();
