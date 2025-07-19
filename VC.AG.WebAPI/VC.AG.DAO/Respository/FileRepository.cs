using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.Contracts;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
using Graph = Microsoft.Graph;
using VC.AG.Models.Extensions;
namespace VC.AG.DAO.Respository
{
    public class FileRepository(IConfiguration config, IMemoryCache cache) : IFileRepository
    {
        readonly GraphContext graphContext = new(config, cache);

        public async Task<DBFile?> Get(DBFile? dBFile, bool? load = false)
        {
            DBFile? result = null;
            if (dBFile != null)
            {
                var token = graphContext.Token;
                var client = new Graph.GraphServiceClient(new Graph.DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return Task.FromResult(0); }));

                DriveItem? targetFile = null;
                try
                {
                    targetFile = await client.Sites[dBFile.SiteId].Drives[dBFile.DriveId].Root.ItemWithPath($"/{dBFile.Name}").Request().GetAsync();
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                }
                if (targetFile != null)
                {
                    result = new()
                    {
                        Name = targetFile.Name,
                        UniqueId = targetFile.Id,
                        Url = targetFile.AdditionalData?.GetStringValue2("@microsoft.graph.downloadUrl"),
                        SiteId = dBFile.SiteId,
                        DriveId = dBFile.DriveId
                    };
                    if (load == true)
                    {
                        var stream = await client.Sites[dBFile.SiteId].Drives[dBFile.DriveId].Items[targetFile.Id].Content.Request().GetAsync();
                        result.ContentStream = stream;
                    }
                }
            }
            return result;
        }
        public async Task<DBFile?> Post(DBFile? item)
        {
            DBFile? result = null;
            var token = graphContext.Token;
            var client = new Graph.GraphServiceClient(new Graph.DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return Task.FromResult(0); }));
            if (item == null || item.Content == null) throw new InvalidOperationException($"File {item?.Name} content cannot be null");
            var fileStream = item.ContentStream != null ? item.ContentStream : new MemoryStream(item.Content);
            var newFile = new Graph.DriveItem
            {
                File = new Graph.File(),
                Name = item.Name
            };
            var existFile = await Get(item);
            string fileId = string.Empty;
            if (existFile != null)
            {
                fileId = $"{existFile.UniqueId}";
            }
            else
            {
                newFile = await client.Sites[item.SiteId].Drives[item.DriveId].Root.Children.Request().AddAsync(newFile);
                fileId = newFile.Id;
            }

            string? idItem = null;
            //si le taille de fichier est sup/égale à 4 mo
            var newFileContent = await client.Sites[item.SiteId].Drives[item.DriveId].Items[fileId].Content.Request().PutAsync<DriveItem>(fileStream);
            idItem = newFileContent?.Id;

            var data = new ListItem()
            {
                Fields = new FieldValueSet() { AdditionalData = item.Values }
            };
            if (!string.IsNullOrEmpty(idItem))
            {
                var lstitm = await client.Sites[item.SiteId].Drives[item.DriveId].Items[idItem].ListItem.Request().UpdateAsync(data);
                result = new DBFile()
                {
                    Id = Convert.ToInt32(lstitm?.Id),
                    Name = item.Name,
                    Values = lstitm?.Fields.AdditionalData,
                    SiteId = item.SiteId,
                    DriveId = item.DriveId
                };

            }
            return result;
        }


        public async Task<string> Delete(DBFile item)
        {
            var token = graphContext.Token;
            var client = new Graph.GraphServiceClient(new Graph.DelegateAuthenticationProvider((requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return Task.FromResult(0); }));
            await client.Sites[item.SiteId].Drives[item.DriveId].Items[item.UniqueId].ListItem.Request().DeleteAsync();
            return "OK";

        }


    }
}
