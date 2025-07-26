// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using VC.AG.DAO.UnitOfWork;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.ServiceLayer.Services;

Console.WriteLine("Hello, World!");
IConfiguration config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables().AddUserSecrets("6b304ef7-4ec7-4950-a8d8-9a3911deb456")
.Build();

IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
IUnitOfWork uow = new UnitOfWork(config,cache);
ISiteContract siteSvc = new SiteService(uow, cache, null);
IUserContract userSvc = new UserService(uow, cache, siteSvc, null);
INotifContract notifSvc = new NotifService(uow, config, cache, userSvc, siteSvc);

var e = DateTime.Now.AddMonths(12);
await notifSvc.SendReminder(null, e);
