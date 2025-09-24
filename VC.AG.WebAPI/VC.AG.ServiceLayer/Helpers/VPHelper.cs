using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models;
using VC.AG.Models.ValuesObject;

namespace VC.AG.ServiceLayer.Helpers
{
    public class VPHelper
    {
        public static async Task<VPRessourceModel> GetVPPersonnel(IConfiguration config,string fullname)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) => { return true; };
            var result = new VPRessourceModel();
            var item = new VPResponse();
            var httpClient = new HttpClient();
            var vpHost = config.GetValue<string>(AppConstants.AppSettingsKeys.VPRessource);
            var vpApiKey = config.GetValue<string>(AppConstants.AppSettingsKeys.VPApiKey2);
            httpClient.DefaultRequestHeaders.Add("ApiKey", vpApiKey);

            string url = $"?resourceModel=PERSONNELS&attribute=PERSONNELS-ID VINCI&attribute=PERSONNELS-Secteur(Code secteur)&attribute=PERSONNELS-Fonction(Job Name)&attribute=PERSONNELS-Portable&attribute=PERSONNELS-Email&attribute=PERSONNELS-Type ressource(Libellé)&operator=EQUALS&PERSONNELS-Nom Prénom={fullname}";
            //var url = $"{vpHost}/{resourceId}";
            //var response = await httpClient.GetAsync(url);
            //response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync();
            //var resp = JsonConvert.DeserializeObject<VPResponse>(content);
            //result.Id = resourceId;
            //if (resp != null && resp.Entities != null && resp.Entities.Count > 0)
            //{
            //    foreach (var entity in resp.Entities)
            //    {
            //        var properties = entity;
            //        //var respItem = new ChantierModel();
            //        foreach (var pr in properties)
            //        {

            //            try
            //            {
            //                //if (pr.EntityName.Contains("-Code"))
            //                //{
            //                //    result.Code = pr.EntityValue;
            //                //}
            //                //else if (pr.EntityName.Contains("-Désignation"))
            //                //{
            //                //    result.Title = pr.EntityValue;
            //                //}
            //                //else if (pr.EntityName.Contains(queryParams.ParentEntity))
            //                //{
            //                //    result.ParentId = pr.EntityValue;
            //                //}
            //            }
            //            catch (Exception ex)
            //            {
            //            }

            //        }

            //    }
            //}
            return result;

        }
    }
}
