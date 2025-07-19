using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Microsoft.SharePoint.News.DataModel;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using Graph = Microsoft.Graph;
namespace VC.AG.Models.Helpers
{
    public static class GraphHelper
    {
        private const string bearer = "Bearer";
        public static async Task<SiteEntity> GetHostInfo(GraphContext context, string delegation = "")
        {
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {context.Token}");
            var siteUrl = !string.IsNullOrEmpty(delegation) ? AppHelper.CombineURL(context.Url, delegation) : context.Url;
            var siteUri = new Uri(siteUrl);
            string url = $"https://graph.microsoft.com/v1.0/sites/{siteUri.Host}:{siteUri.LocalPath}";
            var q0 = await httpClient.GetAsync(url);
            var resp0 = await q0.Content.ReadAsStringAsync();
            JObject obj0 = JObject.Parse(resp0);
            var id = obj0["id"]?.Value<string>();
            var title = obj0["displayName"]?.Value<string>();
            var webUrl = obj0["webUrl"]?.Value<string>();
            if (id == null) throw new InvalidOperationException($"Unable to find site Id : {url}");
            SiteEntity result = new() { SiteUrl = webUrl, Id = id, Title = title };
            return result;
        }
        public static async Task<Dictionary<string, object>> GetLists(GraphContext context, string siteId)
        {
            Dictionary<string, object> r = [];
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, context.Token); return Task.FromResult(0); }));
            var lists = await client.Sites[siteId].Lists.Request().GetAsync();
            foreach (var list in lists)
            {
                try
                {
                    var t0 = list.WebUrl.ToLower().Split('/');
                    var rootFolder = HttpUtility.UrlDecode(t0[^1]);
                    r.Add(rootFolder, list.Id);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }

            }
            return r;
        }
        public static async Task<Dictionary<string, SPList>> GetListsMeta(GraphContext context, string siteId)
        {
            Dictionary<string, SPList> r = [];
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, context.Token); return Task.FromResult(0); }));
            var lists = await client.Sites[siteId].Lists.Request().GetAsync();
            foreach (var list in lists)
            {
                try
                {
                    var l = new SPList();
                    var t0 = list.WebUrl.ToLower().Split('/');
                    var rootFolder = HttpUtility.UrlDecode(t0[^1]);
                    l.Id = new Guid(list.Id);
                    l.Url = list.WebUrl;
                    l.RelativeUrl = new Uri(list.WebUrl).AbsolutePath;
                    l.Title = list.DisplayName;
                    l.RootFolder = rootFolder;
                    l.Template = list.ListInfo.Template;
                    r.Add(rootFolder, l);
                }
                catch (Exception ex)
                {

                    await Console.Out.WriteLineAsync(ex.Message);
                }
            }
            return r;
        }
        public static async Task<Dictionary<string, object>> GetDrives(GraphContext context, string siteId)
        {
            Dictionary<string, object> r = [];
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, context.Token); return Task.FromResult(0); }));
            var lists = await client.Sites[siteId].Drives.Request().GetAsync();
            foreach (var list in lists)
            {
                var t0 = list.WebUrl.ToLower().Split('/');
                var rootFolder = t0[^1];
                r.Add(rootFolder, list.Id);
            }
            return r;
        }
        public static async Task<Dictionary<string, object>> GetSites(GraphContext context, string siteId)
        {
            Dictionary<string, object> r = [];
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, context.Token); return Task.FromResult(0); }));
            var webs = await client.Sites[siteId].Sites.Request().GetAsync();
            while (webs.NextPageRequest != null)
            {
                foreach (var web in webs)
                {
                    var t0 = web.WebUrl.ToLower().Split('/');
                    var rootFolder = t0[^1];
                    r.Add(rootFolder, web.Id);
                }
                webs = await webs.NextPageRequest.GetAsync();
            }
            foreach (var web in webs)
            {
                var t0 = web.WebUrl.ToLower().Split('/');
                var rootFolder = t0[^1];
                r.Add(rootFolder, web.Id);
            }
            return r;
        }
        public static async Task<string?> ShareLink(GraphContext context, string siteId, string itemId)
        {
            string? result = null;
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {context.Token}");
            siteId = siteId.Split(',')[1];
            string url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/drive/items/{itemId}/createLink";
            var obj = new
            {
                type = "edit",
                scope = "organization"
            };

            JsonContent content = JsonContent.Create(obj);
            var q0 = await httpClient.PostAsync(url, content);
            result = await q0.Content.ReadAsStringAsync();

            return result;
        }

    }
}
