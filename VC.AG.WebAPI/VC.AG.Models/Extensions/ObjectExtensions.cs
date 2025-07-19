using Microsoft.AspNetCore.Http;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using VC.AG.Models.Entities;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using static VC.AG.Models.AppConstants;

namespace VC.AG.Models.Extensions
{
    public static class ObjectExtensions
    {
        public static DBFormData ToDBFormData(this IFormCollection form)
        {
            DBFormData result = new();
            var values = form.ToList();
            foreach (var item in values)
            {
                var value = $"{item.Value}";
                switch (item.Key)
                {
                    case "Id": result.Id = value; break;
                    case "Site": result.Site = value; break;
                    case "ListName": result.ListName = value; break;
                    case "Data": result.Data = value; break;
                }
            }
            return result;
        }
        public static DBFormData ToSiteLink(this DBFormData form)
        {
            SiteLink? siteLink = JsonConvert.DeserializeObject<SiteLink>($"{form.Data}");
            if (form.Values == null) form.Values = [];
            if (siteLink != null)
            {
                form.Values["Title"] = $"{siteLink.Title}";
                form.Values[SiteLinkKeys.LinkUrl] = $"{siteLink.Url}";
                form.Values[SiteLinkKeys.LinkTarget] = $"{siteLink.Target}";
                if (siteLink.NewTab != null)
                    form.Values[SiteLinkKeys.NewTab] = siteLink.NewTab;
                if (siteLink.Order != null)
                    form.Values[SiteLinkKeys.Order] = siteLink.Order;
            }
            return form;
        }
        public static byte[] ReadAllBytes(this Stream instream)
        {
            using var memoryStream = new MemoryStream();
            instream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static string GetStringValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            var success = fieldValues.TryGetValue(fieldName, out var r);
            return success ? $"{r}" : string.Empty;
        }
        public static bool? GetBoolValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            bool? r = null;
            try
            {
                var values = fieldValues;
                if (values[fieldName] != null)
                {
                    if ($"{values[fieldName]}" == "0") r = false;
                    else r = $"{values[fieldName]}" == "1" || Convert.ToBoolean($"{values[fieldName]}".ToLowerInvariant());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static string GeUrlValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            FieldUrlValue r = new();
            try
            {
                var values = fieldValues;
                r = (FieldUrlValue)values[fieldName];

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r.Url;
        }
        public static decimal? GetDecimalValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            decimal? r = null;
            try
            {
                var values = fieldValues;
                var value = values[fieldName];
                if (value != null)
                    r = Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static int? GetIntValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            int? r = null;
            try
            {
                var success0 = fieldValues.TryGetValue(fieldName, out object? value);
                if (success0 && value != null)
                {
                    var v = string.Join("", $"{value}".Split(' '));
                    bool success = decimal.TryParse(v, out decimal r0);
                    if ((!success))
                    {
                        success0 = fieldValues.TryGetValue($"{fieldName}.", out value);
                        if (success0 && value != null)
                        {
                            v = value.ToString()?.Split('.')[0];
                            success = decimal.TryParse(v, out r0);
                        }
                    }
                    if (success) r = decimal.ToInt32(r0);
                }
            }
            catch (Exception ex)
            {
                r = 0;
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static StreamLookup? GetStreamLookupValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            StreamLookup? r = null;
            try
            {
                var values = fieldValues;
                var value = $"{values[fieldName]}";
                if (value != null)
                {
                    var items = JsonConvert.DeserializeObject<List<StreamLookup>>(value);
                    if (items != null && items.Count > 0)
                    {
                        r = items[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static StreamUser? GetStreamUserValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            StreamUser? r = null;
            try
            {
                var values = fieldValues;
                var value = $"{values[fieldName]}";
                if (value != null)
                {
                    var items = JsonConvert.DeserializeObject<List<StreamUser>>(value);
                    if (items != null && items.Count > 0)
                    {
                        r = items[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static RefItem GetTaxoValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            RefItem r = new();
            try
            {
                var values = fieldValues;
                if (values[fieldName] is IDictionary<string, object> payload)
                {
                    foreach (var d in payload)
                    {
                        if (d.Key == "WssId") r.Id = int.Parse($"{d.Value}");
                        if (d.Key == "Label") r.Title = d.Value.ToString();
                        if (d.Key == "TermGuid") r.TermId = d.Value.ToString();
                    }
                }
                else if (values[fieldName] is TaxonomyFieldValue payload2)
                {
                    r.Id = payload2.WssId;
                    r.TermId = payload2.TermGuid;
                    r.Title = payload2.Label;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static RefItem GetRefModel2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            RefItem r = new();
            try
            {
                var values = fieldValues;
                var r0 = (FieldLookupValue)values[fieldName];
                r.Id = r0.LookupId;
                r.Title = r0.LookupValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static FieldLookupValue GetLookupValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            FieldLookupValue r = new();
            try
            {
                var values = fieldValues;
                r = (FieldLookupValue)values[fieldName];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static DateTime? GetDateTimeValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            DateTime? r = null;
            try
            {
                var values = fieldValues;
                var value = $"{values[fieldName]}";
                if (!string.IsNullOrEmpty(value))
                    r = Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static UserEntity? GetUserValue2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            UserEntity? r = null;
            try
            {
                var values = fieldValues;
                var value = (FieldUserValue)values[fieldName];
                if (value != null)
                {
                    r = new()
                    {
                        DisplayName = value.LookupValue,
                        SPId = value.LookupId,
                        Email = value.Email
                    };
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static UserEntity? GetUserValue3(this IDictionary<string, object> fieldValues, string fieldName)
        {
            UserEntity? r = null;
            try
            {
                var values = fieldValues;

                var success = int.TryParse($"{values[$"{fieldName}LookupId"]}", out int value);
                if (success && value == 0) return null;
                r = new()
                {
                    SPId = value
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static UserEntity? GetUserValue4(this IDictionary<string, object> fieldValues, string fieldName)
        {
            UserEntity? r = null;
            try
            {
                var values = fieldValues[fieldName];
                dynamic[]? d1 = JsonConvert.DeserializeObject<dynamic[]>($"{values}");
                if (d1?.Length > 0)
                {
                    r = new UserEntity() { SPId = d1[0]?["id"], Email = d1[0]?["email"], DisplayName = d1[0]?["title"] };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static FieldUserValue? GetUserValue5(this IDictionary<string, object> fieldValues, string fieldName)
        {
            FieldUserValue? r = null;
            try
            {
                var values = fieldValues[fieldName];
                dynamic[]? d1 = JsonConvert.DeserializeObject<dynamic[]>($"{values}");
                if (d1?.Length > 0)
                {
                    r = new FieldUserValue() { LookupId = d1[0]?["id"] };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static List<UserEntity>? GetUserValues2(this IDictionary<string, object> fieldValues, string fieldName)
        {
            List<UserEntity>? r = null;
            try
            {
                var values0 = fieldValues;
                if (values0[fieldName] is FieldUserValue[] values)
                {
                    r = [];
                    foreach (var user in values)
                    {
                        r.Add(new UserEntity()
                        {
                            DisplayName = user.LookupValue,
                            SPId = user.LookupId,
                            Email = user.Email
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static bool EqualsNotNull(this string? a, string? b)
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
