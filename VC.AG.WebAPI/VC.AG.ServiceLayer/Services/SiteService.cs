using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;
using static VC.AG.Models.AppConstants;
using VC.AG.ServiceLayer.Helpers;
using VC.AG.Models.Enums;
using VC.AG.Models.Helpers;
using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
namespace VC.AG.ServiceLayer.Services
{
    public class SiteService(IUnitOfWork uow, IMemoryCache cache, ILogger<SiteService>? logger) : ISiteContract
    {
        public async Task<SiteEntity?> Get(string? delegation = "", bool force = false, bool content = true)
        {
            delegation = $"{delegation}".ToLower();
            var cacheKey = string.IsNullOrEmpty(delegation) ? CacheKeysKeys.SiteRootInfo : $"{CacheKeysKeys.SiteInfo}_{delegation}";
            var opInProgressCacheKey = "App.Cache.Loading";
            cache.TryGetValue(cacheKey, out SiteEntity? result);
            cache.TryGetValue(opInProgressCacheKey, out bool? appLoading);
            if (result == null || force)
            {
                CheckAppInProgress(result, appLoading, delegation, opInProgressCacheKey);

                SiteEntity site = await uow.DBRepo.GetSite(delegation) ?? throw new InvalidOperationException($"Site {delegation} cannot be null");
                site.RootFolder = delegation.ToUpper();
                if (string.IsNullOrEmpty(delegation))
                {
                    result = await GetSiteContents(site, content, force);
                    cache.Set(cacheKey, result);
                }
                else
                {
                    result = await GetSubSiteContents(site, content, delegation);
                    cache.Set(cacheKey, result);
                }
                cache.Remove(opInProgressCacheKey);
            }
            return result;
        }
        void CheckAppInProgress(SiteEntity? result, bool? appLoading, string delegation, string opInProgressCacheKey)
        {

            if (result == null && appLoading == true && string.IsNullOrEmpty(delegation))
            {
                throw new InvalidOperationException($"Application loading in progress. Please try in few moment later ...");
            }
            else if (result == null && string.IsNullOrEmpty(delegation)) cache.Set(opInProgressCacheKey, true);
        }
        async Task<SiteEntity> GetSiteContents(SiteEntity site, bool content, bool force)
        {
            if (content)
            {
                site = await site.GetRootLists(uow);
                //Load all sub sites
                var index = 0;
                if (site.Sites != null)
                {
                    var count = site.Sites.Count;
                    var startIndex = 0;
                    var take = 30;
                    while (index < count)
                    {
                        var range = new Range(startIndex, startIndex + take);
                        List<System.Threading.Tasks.Task> TaskList = [];
                        foreach (var subSite in site.Sites.Select(a => a.Key).Take(range))
                        {
                            await Console.Out.WriteLineAsync($"{subSite} loading requested");
                            TaskList.Add(Get(subSite, force));
                            index++;
                        }
                        await Task.WhenAll(TaskList);
                        startIndex = index - 1;
                    }
                }
            }
            return site;
        }
        async Task<SiteEntity> GetSubSiteContents(SiteEntity site, bool content, string delegation)
        {
            try
            {
                if (content)
                {
                    site = await site.GetSiteLists(uow);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                errorMessage = errorMessage.Replace('\n', '_').Replace('\r', '_');
                string message = $"{delegation} : {errorMessage}";
                AppHelper.LogEntry(logger, message, LogType.Error);
            }
            return site;
        }
        public async Task<IEnumerable<Access>?> GetUserAccess(UserEntity user, bool force = false)
        {
            var result = new List<Access>();
            var rootSite = await Get();
            var sites = rootSite?.Sites?.Select(item => item.Key);
            if (sites != null)
            {
                foreach (var item in sites)
                {
                    var s = await Get(item);
                    if (force)
                    {
                        s = await Refresh(SiteRefreshTarget.Access, item);
                    }
                    if (s != null)
                    {
                        UpdateAccess0(ref result, user, s);
                    }

                }
            }
            return result;
        }

        private static void UpdateAccess0(ref List<Access> result, UserEntity user, SiteEntity s)
        {
            List<Access>? sAccessList = null;
            if (user.IsSiteAdmin == true)
            {
                var a = new Access() { Site = s.RootFolder?.ToUpper(), Role = UserRole.Admin.ToString() };
            }
           
            if (sAccessList != null)
                result.AddRange(sAccessList);
        }

       

    

        public async Task<SiteEntity?> Refresh(SiteRefreshTarget target, string? delegation = "")
        {
            var site = await Get(delegation) ?? throw new InvalidOperationException($"Site {delegation} cannot be null");
            var cacheKey = string.IsNullOrEmpty(delegation) ? CacheKeysKeys.SiteRootInfo : $"{CacheKeysKeys.SiteInfo}_{delegation}";
            site = await site.GetSiteBasicLists(uow, target);
            cache.Set(cacheKey, site);
            return site;
        }
    }
}
