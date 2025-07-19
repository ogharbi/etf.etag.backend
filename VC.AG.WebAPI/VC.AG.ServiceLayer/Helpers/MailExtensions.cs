using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static VC.AG.Models.AppConstants;
using VC.AG.Models.ValuesObject;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Extensions;
using VC.AG.ServiceLayer.Contracts;
using VC.AG.Models.Helpers;
using VC.AG.Models.Enums;
using System.Net.WebSockets;
using VC.AG.Models.ValuesObject.SPContext;
using Microsoft.SharePoint.News.DataModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Graph;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VC.AG.ServiceLayer.Helpers
{
    public static class MailExtensions
    {
        private const string bearer = "Bearer";
        public static async Task<string> Send(this MailObject mail, IConfiguration config, GraphContext graphContext)
        {
            var result = string.Empty;
            try
            {
                var smtpEnable = config.GetValue<string>(AppSettingsKeys.AppSmtpEnabled);
                var smtpFrom = $"{config.GetValue<string>(AppSettingsKeys.AppSmtpFrom)}";
                var token = await graphContext.GetS2SToken();
                HttpClient client = new();
                var url = $"https://graph.microsoft.com/v1.0/users/{smtpFrom}/sendMail";
                var tos = new List<dynamic>();
                var ccs = new List<dynamic>();
                UpdateTos(ref tos, mail);
                UpdateCcs(ref ccs, mail);

                dynamic item = new
                {
                    from = smtpFrom,
                    message = new
                    {
                        subject = mail.Subject,
                        body = new
                        {
                            contentType = "HTML",
                            content = mail.Body
                        },
                        toRecipients = tos,
                        ccRecipients = ccs
                    }
                };
                if (smtpEnable != "true") return "Not enabled";
                var dataAsString = JsonConvert.SerializeObject(item);
                var d = new StringContent(dataAsString, UnicodeEncoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(bearer, token);
                var resp = client.PostAsync(url, d).Result;
                var r = await resp.Content.ReadAsStringAsync();
                result = string.IsNullOrEmpty(r) ? "OK" : "KO";

            }
            catch (Exception ex)
            {
                result = $"KO - {ex.Message}";
            }

            return result;
        }

        private static void UpdateCcs(ref List<dynamic> ccs, MailObject mail)
        {
            if (mail.Cc != null)
            {
                foreach (var t in mail.Cc)
                {
                    ccs.Add(new
                    {
                        emailAddress = new { address = t }
                    });
                }
            }
        }

        private static void UpdateTos(ref List<dynamic> tos, MailObject mail)
        {
            if (mail.MailTo != null)
            {

                foreach (var t in mail.MailTo)
                {
                    tos.Add(new
                    {
                        emailAddress = new { address = t }
                    });
                }
            }
        }

       
    }
}
