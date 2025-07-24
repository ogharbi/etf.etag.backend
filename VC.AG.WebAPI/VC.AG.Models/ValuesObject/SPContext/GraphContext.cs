using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using VC.AG.Models.Helpers;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.ValuesObject.SPContext
{
    public class GraphContext
    {
        IConfiguration Config { get; }
        IMemoryCache Cache { get; }
        public string Url { get; }
        public string RootUrl { get; }
        public string? Token { get { return GetToken().Result; } }

        public GraphContext(IConfiguration config, IMemoryCache cache, string url = "")
        {
            Config = config;
            Cache = cache;
            Url = string.IsNullOrEmpty(url) ? $"{Config.GetValue<string>(AppSettingsKeys.SPOUrl)}" : url;
            RootUrl = $"{Config.GetValue<string>(AppSettingsKeys.SPOUrl)}";
        }
        public async Task<string?> GetToken()
        {
            string? result = null;

            string tokenCacheKey = $"vc-access-token-web-api-graph";
            string tokenExpireCacheKey = $"vc-access-token-expired-web-api-graph";
            Cache.TryGetValue(tokenExpireCacheKey, out DateTime expireAt);
            if (expireAt != DateTime.MinValue && expireAt.ToUniversalTime().CompareTo(DateTime.UtcNow) > 0)
            {
                Cache.TryGetValue(tokenCacheKey, out result);
            }
            else
            {
                var authManager = new AuthManager(Config, true);

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
        public async Task<string?> GetS2SToken()
        {
            string? result = null;

            string tokenCacheKey = $"vc-access-token-web-api-s2s-graph";
            string tokenExpireCacheKey = $"vc-access-token-expired-web-api-s2s-graph";
            Cache.TryGetValue(tokenExpireCacheKey, out DateTime expireAt);
            if (expireAt != DateTime.MinValue && expireAt.ToUniversalTime().CompareTo(DateTime.UtcNow) > 0)
            {
                Cache.TryGetValue(tokenCacheKey, out result);
            }
            else
            {
                var authManager = new AuthManager(Config, true);

                var token = await authManager.GetS2Soken();
                if (token != null)
                {
                    result = $"{token.GetValueOrDefault().GetProperty("access_token")}";
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

        public GraphServiceClient GetServiceClient()
        {
            var token = Token;
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return Task.FromResult(0); }));
            return client;
        }

    }
}
