using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using static VC.AG.Models.AppConstants;
using VC.AG.Models.ValuesObject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VC.AG.Models.Extensions;
using Microsoft.SharePoint.News.DataModel;
using Wkhtmltopdf.NetCore;
using Microsoft.SharePoint.Client.Utilities;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;
using VC.AG.Models.Enums;
namespace VC.AG.Models.Helpers
{
    public static class AppHelper
    {
        public static void LogEntry(ILogger? logger, string message, LogType? logType = LogType.Information)
        {
            if (logger != null && message != null)
            {
                    message = message.Replace('\n', '_').Replace('\r', '_');
                    switch (logType)
                    {
                        case LogType.Warning:
                            logger.LogWarning(message);
                            break;
                        case LogType.Error:
                            logger.LogError(message);
                            break;
                        default:
                            logger.LogInformation(message);
                            break;
                    }
            }
        }
        public static Stream GetPdfStream(IGeneratePdf generatePdf, string html, Wkhtmltopdf.NetCore.Options.Orientation orientation)
        {
            if (html.Contains("orientation=landscape", StringComparison.CurrentCultureIgnoreCase)) orientation = Wkhtmltopdf.NetCore.Options.Orientation.Landscape;
            var myConvertOptions = new PdfConvertOptions
            {
                IsLowQuality = false,
                FooterCenter = "\" [page] / [topage] \"",
                PageOrientation = orientation,
                HeaderSpacing = 0,
                PageSize = Wkhtmltopdf.NetCore.Options.Size.A4,
                PageMargins = new Wkhtmltopdf.NetCore.Options.Margins(5, 5, 5, 5)
            };
            generatePdf.SetConvertOptions(myConvertOptions);
            var pdf = generatePdf.GetPDF(html);
            var pdfStream = new System.IO.MemoryStream();
            pdfStream.Write(pdf, 0, pdf.Length);
            pdfStream.Position = 0;
            Stream streamResult = pdfStream;
            return streamResult;
        }
        public static string? GetTranslationValue(List<DBItem>? values, string? parent, string? key, string locale)
        {
            var result = key;
            string targetField;
            locale = locale.Replace("-", "");
            if (AppConstants.TranslationKeys.EnUS_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.EnUS_Name;
            }
            else if (AppConstants.TranslationKeys.DeDE_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.DeDE_Name;
            }
            else if (AppConstants.TranslationKeys.EsES_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.EsES_Name;
            }
            else if (AppConstants.TranslationKeys.PlPL_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.PlPL_Name;
            }
            else if (AppConstants.TranslationKeys.CzCZ_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.CzCZ_Name;
            }
            else
            {
                targetField = AppConstants.TranslationKeys.FrFR_Name;
            }
            var v = string.IsNullOrEmpty(parent) ? values?.Find(a => a.Values?.GetStringValue2(AppConstants.AppKeys.Code).EqualsNotNull(key) == true) :
                values?.Find(a => a.Values?.GetStringValue2("Title").EqualsNotNull(parent) == true && a.Values?.GetStringValue2(AppConstants.AppKeys.Code).EqualsNotNull(key) == true);
            if (v != null)
            {
                result = v.Values?.GetStringValue2(targetField) ?? key;
            }
            return result;
        }
        public static async Task<string> PostAsync(string url, string accessToken, dynamic data)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var json = JsonConvert.SerializeObject(data);
            var d = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, d);
            var response = await resp.Content.ReadAsStringAsync();
            return response;
        }
        public static string BuildViewFields(List<string> fields)
        {
            if (fields == null) return string.Empty;
            StringBuilder sb = new();
            foreach (var f in fields)
            {
                sb.AppendFormat("<FieldRef Name='{0}'/>", f);
            }
            return sb.ToString();
        }
        public static string CombineURL(string url1, string url2)
        {
            bool url1Empty = string.IsNullOrEmpty(url1);
            bool url2Empty = string.IsNullOrEmpty(url2);

            if (url1Empty && url2Empty)
            {
                return "";
            }

            if (url1Empty)
            {
                return url2;
            }

            if (url2Empty)
            {
                return url1;
            }

            if (url1.EndsWith('/'))
            {
                url1 = url1[..^1];
            }

            if (url2.StartsWith('/'))
            {
                url2 = url2.Remove(0, 1);
            }

            return url1 + "/" + url2;
        }

        public static string CombineURL(params string[] urls)
        {
            return urls.Aggregate(CombineURL);
        }
        public static string XmlSerialize(object item)
        {
            XmlSerializer xmlSerializer = new(item.GetType());
            StringWriter textWriter = new();

            xmlSerializer.Serialize(textWriter, item);
            return textWriter.ToString();
        }

        private static readonly string[] separator = [","];

        public static string GetUniqueValues(string input)
        {
            string result = $"{input}";
            try
            {
                var tb = result.Split(separator, StringSplitOptions.RemoveEmptyEntries).GroupBy(a => a).Select(a => a.Key).ToList();
                result = string.Join(",", tb);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        public static string CleanFileName(string name)
        {
            try
            {
                var catacters = "\"*:<>?/\\|~#%&*{|}";
                foreach (char c in catacters)
                {
                    name = name.Replace(c, '-');
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return name;
        }
        public static string BuildQuery(List<string> ops, string op)
        {
            var result = string.Empty;
            if (ops.Count == 1)
            {
                result = ops[0];
            }
            else if (ops.Count == 2)
            {
                result = $"<{op}>{ops[0]}{ops[1]}</{op}>";
            }
            else if (ops.Count > 2)
            {
                result = $"<{op}>{ops[0]}{ops[1]}</{op}>";
                for (var i = 2; i < ops.Count; i++)
                {
                    result = $"<{op}>{result}{ops[i]}</{op}>";
                }
            }
            return result;
        }

    }
}
