using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using VC.AG.DAO.Contracts;
using VC.AG.Models.Entities;
using VC.AG.Models.Extensions;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using static Microsoft.Graph.Constants;
using Graph = Microsoft.Graph;

namespace VC.AG.DAO.Respository
{
    public class DBRepository(IConfiguration config, IMemoryCache cache) : IDBRepository
    {
        readonly GraphContext graphContext = new(config, cache);
        readonly SpoContext spoContext = new(config, cache);
        private const string bearer = "Bearer";
        public async Task<SiteEntity?> GetSite(string delegation = "")
        {
            var r = await GraphHelper.GetHostInfo(graphContext, delegation);
            var siteId = $"{r.Id}";
            var lists = await GraphHelper.GetLists(graphContext, siteId);
            var listsMeta = await GraphHelper.GetListsMeta(graphContext, siteId);
            var drives = await GraphHelper.GetDrives(graphContext, siteId);
            var sites = await GraphHelper.GetSites(graphContext, siteId);
            r.Sites = sites;
            r.Lists = lists;
            r.ListsMeta = listsMeta;
            r.Drives = drives;
            return r;
        }
        public async Task<IEnumerable<DBItem>?> GetAll(DBQuery query)
        {
            var token = graphContext.Token;
            var client = new Graph.GraphServiceClient(new Graph.DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, token); return Task.FromResult(0); }));
            var queryOptions = new List<Graph.QueryOption>();

            if (!string.IsNullOrEmpty(query.Filter))
            {
                queryOptions.Add(new Graph.QueryOption("filter", query.Filter));
            }
            if (!string.IsNullOrEmpty(query.Expand))
            {
                queryOptions.Add(new Graph.QueryOption("expand", query.Expand));
            }
            if (!string.IsNullOrEmpty(query.Select))
            {
                if (!query.Select.StartsWith("Id")) query.Select = $"Id,{query.Select}";
                queryOptions.Add(new Graph.QueryOption("select", query.Select));
            }
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                queryOptions.Add(new Graph.QueryOption("orderBy", query.OrderBy));
            }
            query.Top ??= 1000;
            if (query.Top.HasValue)
            {
                queryOptions.Add(new Graph.QueryOption("top", $"{query.Top.Value}"));
            }
            if (query.Skip.HasValue)
            {
                queryOptions.Add(new Graph.QueryOption("skip", $"{query.Skip.Value}"));
            }
            var result = new List<DBItem>();
            try
            {
                var items = await client.Sites[query.SiteId].Lists[query.ListId].Items.Request(queryOptions).GetAsync();
                var pageIterator = Graph.PageIterator<Graph.ListItem>
                     .CreatePageIterator(client, items, (e) =>
                     {
                         return true;
                     });
                await pageIterator.IterateAsync();
                foreach (Graph.ListItem item in items.CurrentPage)
                {
                    try
                    {
                        var model = new DBItem()
                        {
                            Id = item.Id,
                            Title = item.GetStringValue("Title"),
                            Values = item.Fields.AdditionalData
                        };
                        result.Add(model);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{item.Id} : {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }
        public async Task<DBStream?> GetStream(DBQuery query, bool? all = false)
        {
            DBStream? result = null;
            string viewFields = string.Empty;
            var token = spoContext.Token;
            if (query.Fields?.Count > 0)
            {
                viewFields = AppHelper.BuildViewFields(query.Fields);
                viewFields = string.Format("<ViewFields>{0}</ViewFields>", viewFields);
            }
            var top = query.Top.GetValueOrDefault();
            if (top == 0) top = 500;
            var viewXml = string.Concat(
             "<View>",
                viewFields,
                 "<Query>",
                    query.Filter,
                 "</Query>",
                 string.Format("<RowLimit Paged='TRUE'>{0}</RowLimit>", top),
             "</View>");
            dynamic d = new ExpandoObject();
            d.parameters = new ExpandoObject();
            d.parameters.ViewXml = viewXml;
            d.parameters.Paging = query.NextHref;
            var url = $"{query.SiteUrl}/_api/web/lists/getbyId('{query.ListId}')/RenderListDataAsStream?InplaceSearchQuery={query.SearchTerm}&InplaceFullListSearch=1&{query.AppendQuery}&{query.NextHref}";
            try
            {
                string r = await AppHelper.PostAsync(url, token, d);
                if (r != null)
                {
                    if (r.Contains("m:error", StringComparison.InvariantCultureIgnoreCase))
                        throw new InvalidOperationException(r);
                    result = JsonConvert.DeserializeObject<DBStream>(r);
                }
                if (all == true)
                {
                    result = await GetAll(result, query, d, url, token);
                }
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }

        static async Task<DBStream?> GetAll(DBStream result, DBQuery query, dynamic d,string url,string token)
        {
            query.NextHref = result?.NextHref;
            while (!string.IsNullOrEmpty(query.NextHref))
            {

                var nextHref = result?.NextHref;
                if (query.NextHref.StartsWith('?')) nextHref = query.NextHref[1..];
                d.parameters.Paging = nextHref;
                var r = await AppHelper.PostAsync(url, token, d);
                var subResult = JsonConvert.DeserializeObject<DBStream>(r);
                if (subResult?.Row != null)
                {
                    result?.Row?.AddRange(subResult.Row);
                    query.NextHref = subResult.NextHref;
                }
                else
                {
                    query.NextHref = null;
                }
            }
            return result;
        }

        public async Task<string?> GetFilterValues(DBQuery query)
        {
            string? result = null;
            var token = spoContext.Token;
            var url = $"{query.SiteUrl}/_api/web/GetListUsingPath(DecodedUrl=@a1)/RenderListFilterData?@a1=%27{HttpUtility.UrlEncode(query.ListUrl)}%27&FieldInternalName=%27{HttpUtility.UrlEncode(query.Select)}%27&{query.AppendQuery}";
            try
            {
                dynamic d = new ExpandoObject();
                result = await AppHelper.PostAsync(url, token, d);
                if (!string.IsNullOrEmpty(result) && result.Contains("m:error", StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidOperationException(result);
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }
        public async Task<IEnumerable<DBItem>?> GetListViews(DBQuery query)
        {
            List<DBItem>? result = [];
            try
            {
                var siteUrl = $"{query.SiteUrl}";
                var ctx = spoContext.GetClientContext(siteUrl);
                var views = ctx.Web.Lists.GetById(new Guid($"{query.ListId}")).Views;
                ctx.Load(views, view => view.Include(a => a.Id, a => a.ServerRelativeUrl, a => a.Title, a => a.DefaultView, a => a.ViewFields));
                await ctx.ExecuteQueryAsync();
                foreach (var view in views)
                {
                    var model = new DBItem { Values = new Dictionary<string, object>() };
                    var url = view.ServerRelativeUrl;
                    var name = view.Title;
                    var id = view.Id.ToString();
                    model.Id = id;
                    model.Title = name;
                    model.Url = url;
                    model.Values.Add("Fields", view.ViewFields.ToList());
                    model.Values.Add("IsDefault", view.DefaultView);
                    model.Values.Add("RelativeServerUrl", view.ServerRelativeUrl);
                    result.Add(model);
                }
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }
        public async Task<IEnumerable<DBItem>?> GetListContentTypes(DBQuery query)
        {
            var result = new List<DBItem>();
            try
            {
                var token = graphContext.Token;
                var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, token); return Task.FromResult(0); }));
                var cts = await client.Sites[query.SiteId].Lists[query.ListId].ContentTypes.Request().GetAsync();
                foreach (var ct in cts)
                {
                    result.Add(new DBItem() { Id = ct.Id, Title = ct.Name });
                }
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }
        public async Task<IEnumerable<DBItem>?> GetListColumns(DBQuery query)
        {
            List<DBItem>? result = [];
            try
            {
                var siteUrl = $"{query.SiteUrl}";
                var ctx = spoContext.GetClientContext(siteUrl);
                var fields = ctx.Web.Lists.GetById(new Guid($"{query.ListId}")).Fields;
                ctx.Load(fields, view => view.Include(a => a.Id, a => a.StaticName, a => a.Title, a => a.TypeAsString));
                await ctx.ExecuteQueryAsync();
                foreach (var field in fields)
                {
                    var staticName = field.StaticName;
                    if (staticName.StartsWith("Col_") || staticName.StartsWith("F_x003a"))
                    {
                        var id = field.Id.ToString();
                        DBItem model = new()
                        {
                            Values = new Dictionary<string, object>(),
                            Id = id,
                            Title = field.Title,
                            UniqueId = staticName
                        };
                        model.Values.Add("Type", field.TypeAsString);
                        result.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }
        public async Task<DBItem?> GetListView(DBQuery query)
        {
            var result = new DBItem { Values = new Dictionary<string, object>() };
            try
            {
                var rootUri = new Uri(spoContext.RootUrl);
                var siteUrl = $"{query.SiteUrl}";
                var ctx = spoContext.GetClientContext(siteUrl);
                var view = ctx.Web.Lists.GetById(new Guid($"{query.ListId}")).Views.GetById(new Guid($"{query.ItemId}"));
                ctx.Load(view, a => a.Id, a => a.ServerRelativeUrl, a => a.Title, a => a.DefaultView, a => a.ViewFields);
                await ctx.ExecuteQueryAsync();
                var url = view.ServerRelativeUrl;
                var name = view.Title;
                var id = view.Id.ToString();
                result.Values.Add("Id", id);
                result.Values.Add("Name", name);
                result.Values.Add("Fields", view.ViewFields.ToList());
                result.Values.Add("IsDefault", view.DefaultView);
                result.Values.Add("ServerUrl", view.ServerRelativeUrl);
                result.Values.Add("Url", $"{rootUri.Scheme}://{rootUri.Host}{url}".ToLowerInvariant());
            }
            catch (Exception ex)
            {
                if (query.CatchError != true) throw new InvalidOperationException(ex.Message, ex.InnerException);
            }
            return result;
        }

        public async Task<DBItem?> Post(DBCreate item)
        {
            var token = graphContext.Token;
            HttpClient client = new();
            var url = $"https://graph.microsoft.com/v1.0/sites/{item?.SiteId}/lists/{item?.ListId}/items";
            var dataAsString = JsonConvert.SerializeObject(item?.Data);
            var d = new StringContent(dataAsString, UnicodeEncoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(bearer, token);
            var resp = client.PostAsync(url, d).Result;
            var r = await resp.Content.ReadAsStringAsync();
            JObject obj0 = JObject.Parse(r);
            var error = obj0["error"];
            if (error != null) throw new InvalidOperationException($"{error}");
            var id = obj0["id"]?.Value<string>();
            var fields = $"{obj0["fields"]}";
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>($"{fields}");
            DBItem? result = new() { Id = id, Values = values };
            return result;
        }

        public async Task<DBItem?> Put(DBUpdate item)
        {
            var token = graphContext.Token;
            HttpClient client = new();
            var url = $"https://graph.microsoft.com/v1.0/sites/{item?.SiteId}/lists/{item?.ListId}/items/{item?.Id}";
            var dataAsString = JsonConvert.SerializeObject(item?.Data);
            var d = new StringContent(dataAsString, UnicodeEncoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(bearer, token);
            var resp = client.PatchAsync(url, d).Result;
            var r = await resp.Content.ReadAsStringAsync();
            JObject obj0 = JObject.Parse(r);
            var error = obj0["error"];
            if (error != null) throw new InvalidOperationException($"{error}");
            var id = obj0["id"]?.Value<string>();
            var fields = $"{obj0["fields"]}";
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>($"{fields}");
            DBItem? result = new() { Id = id, Values = values };
            return result;
        }

        public async Task<string> Delete(DBUpdate item)
        {
            var token = graphContext.Token;
            var client = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue(bearer, token); return Task.FromResult(0); }));
            await client.Sites[item.SiteId].Lists[item.ListId].Items[$"{item.Id}"].Request().DeleteAsync();
            return "OK";
        }

        public async Task<string?> PostForm(DBFormData item)
        {
            string? result = null;
            var siteUrl = $"{item.SiteUrl}";
            var ctx = spoContext.GetClientContext(siteUrl);
            var list = ctx.Web.Lists.GetById(new Guid($"{item.ListId}"));
            Microsoft.SharePoint.Client.ListItem? newItem;
            if (item.Id == null)
            {
                var newItemCreation = new ListItemCreationInformation();
                newItem = list.AddItem(newItemCreation);
            }
            else
            {
                newItem = list.GetItemById(item.Id);
            }
            if (item.Values != null)
            {
                foreach (var d in item.Values)
                {
                    newItem[d.Key] = d.Value;
                }
            }
            newItem.Update();
            await ctx.ExecuteQueryAsync();
            return result;
        }
    }
}
