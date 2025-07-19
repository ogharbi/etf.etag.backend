using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using static System.Formats.Asn1.AsnWriter;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.Helpers
{
    internal class AuthManager : IDisposable
    {
        readonly string clientId;
        readonly string clientSecret;
        readonly bool graphScope;
        readonly string tokenEndpoint;
        readonly string tokenEndpointV2;
        readonly string spoUrl;
        readonly string spoUser;
        readonly string spoPwd;
        public AuthManager(IConfiguration config, bool graphScope = false)
        {
            string tenantId = $"{config.GetValue<string>(AppSettingsKeys.TenantId)}";
            spoUrl = $"{config.GetValue<string>(AppSettingsKeys.SPOUrl)}";
            spoUser = $"{config.GetValue<string>(AppSettingsKeys.SPOUser)}";
            spoPwd = $"{config.GetValue<string>(AppSettingsKeys.SPOPwd)}";
            clientId = $"{config.GetValue<string>(AppSettingsKeys.ClientId)}";
            clientSecret = $"{config.GetValue<string>(AppSettingsKeys.ClientSecret)}";
            this.graphScope = graphScope;
            tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
            tokenEndpointV2 = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
        }
        // Token cache handling
        readonly AutoResetEvent? tokenResetEvent = null;
        bool disposedValue;

        public async Task<JsonElement?> GetToken()
        {
            Uri site = new(spoUrl);
            JsonElement token = await AcquireTokenAsync(site, spoUser, spoPwd);
            return token;
        }
        public async Task<JsonElement?> GetS2Soken()
        {
            JsonElement token = await AcquireS2STokenAsync();
            return token;
        }
        private async Task<JsonElement> AcquireS2STokenAsync()
        {
            HttpClient httpClient = new();
            var scope = "https://graph.microsoft.com//.default";
            var body = $"client_id={clientId}&client_secret={clientSecret}&scope={HttpUtility.UrlEncode(scope)}&grant_type=client_credentials";
            using var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await httpClient.PostAsync(tokenEndpointV2, stringContent).ContinueWith((response) =>
            {
                return response.Result.Content.ReadAsStringAsync().Result;
            }).ConfigureAwait(false);

            var tokenResult = JsonSerializer.Deserialize<JsonElement>(result);
            return tokenResult;
        }

        private async Task<JsonElement> AcquireTokenAsync(Uri resourceUri, string username, string password)
        {
            HttpClient httpClient = new();
            string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";
            string scope = "https://microsoft.sharepoint-df.com/AllSites.FullControl";
            if (graphScope)
            {
                resource = "https://graph.microsoft.com";
                scope = "https://graph.microsoft.com/Sites.Manage.All";
            }
            var body = $"resource={resource}&client_id={clientId}&scope={HttpUtility.UrlEncode(scope)}&grant_type=password&username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}";
            using var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await httpClient.PostAsync(tokenEndpoint, stringContent).ContinueWith((response) =>
            {
                return response.Result.Content.ReadAsStringAsync().Result;
            }).ConfigureAwait(false);

            var tokenResult = JsonSerializer.Deserialize<JsonElement>(result);
            return tokenResult;
        }




        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {

                if (tokenResetEvent != null)
                {
                    tokenResetEvent.Set();
                    tokenResetEvent.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

