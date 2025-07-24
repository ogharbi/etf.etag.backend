// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using VC.AG.ConsoleApp;
using static VC.AG.Models.AppConstants;
//ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().AddUserSecrets("b164e22c-af5a-45f2-99c7-ef156126118b").Build();
IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
var urlTarget = $"{config.GetValue<string>(AppSettingsKeys.SPOUrl)}";
Console.WriteLine("Hello, World!");
var targetContextRoot = new TargetContext(config, cache, urlTarget);
await targetContextRoot.JobDebug();