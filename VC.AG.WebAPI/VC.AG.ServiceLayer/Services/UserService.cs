using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web;
using Microsoft.SharePoint.Client;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;

namespace VC.AG.ServiceLayer.Services
{
    public class UserService : IUserContract
    {
        readonly IUnitOfWork uow;
        readonly IMemoryCache cache;
        readonly ISiteContract site;
        readonly IHttpContextAccessor? httpContextAccessor;

        public UserService(IUnitOfWork uow, IMemoryCache cache, ISiteContract site, IHttpContextAccessor? httpContextAccessor = null)
        {
            this.uow = uow;
            this.cache = cache;
            this.site = site;
            this.httpContextAccessor = httpContextAccessor;
            if (httpContextAccessor != null)
                GetRootSite().GetAwaiter().GetResult();

        }
        public async Task<UserEntity?> Get(string email, bool force = false)
        {
            string cacheKey = $"Profile-{email}";
            cache.TryGetValue(cacheKey, out UserEntity? result);
            if (result == null || force)
            {
                result = await uow.UserRep.Get(email);
                cache.Set(cacheKey, result);
            }
            if (result != null)
            {
                result.Access = await site.GetUserAccess(result, force);
            }
            return result;
        }
        public async Task<UserEntity?> GetMe(bool force = false)
        {
            HttpContext httpContext = httpContextAccessor?.HttpContext
                                      ?? throw new InvalidOperationException("Get Me HttpContext cannot be null");
            string? email = httpContext.User?.GetLoginHint();
            string cacheKey = $"Profile-{email}";
            cache.TryGetValue(cacheKey, out UserEntity? result);
            if (result == null || force)
            {
                result = await uow.UserRep.Get(email);
                cache.Set(cacheKey, result);
            }
            if (result != null)
            {
                result.Access = await site.GetUserAccess(result, force);
            }
            return result;
        }

        public async Task<IEnumerable<UserEntity>?> Search(string word)
        {
            var result = await uow.UserRep.Search(word);
            return result;
        }
        public async Task<string?> Assign(AssignAccess assign)
        {
            var siteInfo = await site.Get(assign.Site);
            assign.SiteUrl = siteInfo?.SiteUrl;
            var result = await uow.UserRep.UpdateAssignment(assign);
            return result;
        }

        /// <summary>
        private async Task<SiteEntity?> GetRootSite()
        {
            var rootSite = await site.Get();
            return rootSite;
        }

        public async Task<UserEntity?> GetById(int? spId)
        {
            string cacheKey = $"Profile-spid-{spId}";
            cache.TryGetValue(cacheKey, out UserEntity? result);
            if (result == null)
            {
                result = await uow.UserRep.Get(id:spId);
                cache.Set(cacheKey, result);
            }
            return result;

        }
    }
}
