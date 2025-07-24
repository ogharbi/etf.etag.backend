using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using VC.AG.Models.Helpers;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.ValuesObject.SPContext
{
    public class SpoContext
    {

        IConfiguration Config { get; }
        IMemoryCache Cache { get; }
        public string? Token { get { return GetToken().Result; } }
        public string RootUrl { get; }
        public string Url { get; }
        public SpoContext(IConfiguration config, IMemoryCache cache, string url = "")
        {
            Config = config;
            Cache = cache;
            Url = string.IsNullOrEmpty(url) ? $"{Config.GetValue<string>(AppSettingsKeys.SPOUrl)}" : url;
            RootUrl = $"{Config.GetValue<string>(AppSettingsKeys.SPOUrl)}";
        }
        public async Task<string?> GetToken()
        {
            string? result = null;

            string tokenCacheKey = $"vc-access-token-web-api-spo";
            string tokenExpireCacheKey = $"vc-access-token-expired-web-api-spo";
            DateTime expireAt;
            Cache.TryGetValue(tokenExpireCacheKey, out expireAt);
            if (expireAt != DateTime.MinValue && expireAt.ToUniversalTime().CompareTo(DateTime.UtcNow) > 0)
            {
                Cache.TryGetValue(tokenCacheKey, out result);
            }
            else
            {
                var authManager = new AuthManager(Config, false);

                var token = await authManager.GetToken();
                if (token != null)
                {
                    try
                    {
                        result = $"{token.GetValueOrDefault().GetProperty("access_token")}";
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(token.ToString());
                    }
                 
                    if (!string.IsNullOrEmpty(result))
                    {
                        var expireInStr = $"{token.GetValueOrDefault().GetProperty("expires_in")}";
                        var success = int.TryParse(expireInStr, out int expireIn);
                        if (!success) expireIn = 3600;
                        Cache.Set(tokenCacheKey, result);
                        Cache.Set(tokenExpireCacheKey, DateTime.UtcNow.AddSeconds(expireIn));
                    }
                }
            }
            return result;
        }

        public ClientContext GetClientContext(string url = "", string accessToken = "")
        {
            url = string.IsNullOrEmpty(url) ? $"{Config.GetValue<string>(AppSettingsKeys.SPOUrl)}" : url;
            Uri web = new(url);
            var context = new ClientContext(web);
            var token = string.IsNullOrEmpty(accessToken) ? Token : accessToken;
            if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("Token is empty");
            context.ExecutingWebRequest += (sender, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + token;
            };

            return context;
        }

    }
}
