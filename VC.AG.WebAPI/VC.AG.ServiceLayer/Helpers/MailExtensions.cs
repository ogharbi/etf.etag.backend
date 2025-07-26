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
        public static Task<string> Send(this MailObject mail, IConfiguration config, GraphContext graphContext)
        {
            var result = string.Empty;
            string log = string.Empty;
            var smtpfrom = string.Empty;
            try
            {
                var smtpEnable = config.GetValue<string>(AppSettingsKeys.AppSmtpEnabled);
                var smtpServer = config.GetValue<string>(AppSettingsKeys.AppSmtpServer);
                var smtpFrom = config.GetValue<string>(AppSettingsKeys.AppSmtpFrom);
                var smtpUser = config.GetValue<string>(AppSettingsKeys.AppSmtpUser);
                var smtpPwd = config.GetValue<string>(AppSettingsKeys.AppSmtpPwd);
                if (smtpEnable != "true") return Task.FromResult("Not enabled");
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                SmtpClient client = new SmtpClient(smtpServer);
                client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPwd);
                MailAddress from = new MailAddress(smtpUser, string.Empty, System.Text.Encoding.UTF8);//new MailAddress(smtpFrom, string.Empty, System.Text.Encoding.UTF8);
                client.EnableSsl = true;

                MailMessage message = new MailMessage
                {
                    From = from,
                    Subject = mail.Subject,
                    Body = mail.Body
                };
                foreach (var t in mail.MailTo)
                {
                    message.To.Add(t);
                }
                if (mail.Cc != null)
                {
                    foreach (var t in mail.Cc)
                    {
                        message.CC.Add(t);
                    }
                }
                string htmlTemplateMail = mail.Body;

                message.IsBodyHtml = true;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                if (mail.Attachments != null)
                {
                    foreach (System.Net.Mail.Attachment attah in mail.Attachments)
                    {
                        message.Attachments.Add(attah);
                    }
                }
                smtpfrom = message.From.Address.ToString();
                client.Send(message);
                result = "OK";
            }
            catch (Exception ex)
            {
                //return ex.Message+"; Stack :"+ex.StackTrace;
                result = "KO :" + ex.ToString();
                log = ex.ToString();

            }
            log = log + "\nSmtp From : " + smtpfrom;
            return Task.FromResult(result);

        }
        public static async Task<string> Send2(this MailObject mail, IConfiguration config, GraphContext graphContext)
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
