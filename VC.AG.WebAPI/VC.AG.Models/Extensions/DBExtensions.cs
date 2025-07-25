﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using VC.AG.Models.Helpers;
using VC.AG.Models.ValuesObject;
using System.IO;
using Microsoft.SharePoint.Client;
using VC.AG.Models.Enums;
using VC.AG.Models.Entities;
using Azure.Core;
using static VC.AG.Models.AppConstants;
using Newtonsoft.Json.Linq;
using Microsoft.Graph;
using Microsoft.SharePoint.Client.Search.Query;
using System.Collections;
using System.Net.Mime;
using static System.Formats.Asn1.AsnWriter;

namespace VC.AG.Models.Extensions
{
    public static class DbExtensions
    {
        public static string SerializeStream(this DBStream result)
        {
            var r = System.Text.Json.JsonSerializer.Serialize(result);
            return r;
        }
        public static string SerializeItem(this DBItem result)
        {
            var r = System.Text.Json.JsonSerializer.Serialize(result);
            return r;
        }


        public static WfRequest? ToWfRequest(this DBStream stream, string? site)
        {
            WfRequest? result = null;
            if (stream != null && stream.Row != null && stream.Row.Count > 0)
            {
                var values = stream.Row[0];
                var success = Enum.TryParse($"{values?.GetStringValue2(InterviewKeys.WfStatus)}", out RequestStatus status);
                if (!success) status = RequestStatus.None;
                result = new WfRequest()
                {
                    Values = values,
                    Id = values?.GetStringValue2("ID"),

                };
            }
            return result;
        }

        public static string? GetTitle(this DBItem item, string locale)
        {
            locale = locale.Replace("-", "").ToLowerInvariant();
            var targetField = AppConstants.AppKeys.Code;
            if (AppConstants.TranslationKeys.FrFR_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
            {
                targetField = AppConstants.TranslationKeys.FrFR_Name;
            }
            else if (AppConstants.TranslationKeys.EnUS_Name.ToLower().Contains(locale, StringComparison.InvariantCultureIgnoreCase))
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
            var result = item.Values?.GetStringValue2(targetField);
            return result;
        }
        public static string? GetQuery(this FormQuery query, UserEntity? currentUser)
        {
            string? result;
            var scope = query.Scope;
            if (scope == RequestScope.None) scope = Enums.RequestScope.AllRequests;
            var orderby = string.Empty;
            var orderbyAsc = string.Empty;
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                var tb = query.OrderBy.Split(',');
                orderby = tb[0];
                orderbyAsc = tb.Length > 1 ? tb[1] : "TRUE";
            }

            var ops = new List<string>();
            var isAdmin = currentUser?.IsSiteAdmin == true || currentUser?.Access?.Any(a => query.Site.EqualsNotNull(a.Site) && (a.Role.EqualsNotNull(UserRole.Admin.ToString()))) == true;
            UpdateQuery(ref ops, query, scope, currentUser, isAdmin);
            var filterOps = AppHelper.BuildQuery(ops, "And");
            filterOps = string.IsNullOrEmpty(filterOps) ? null : $"<Where>{filterOps}</Where>";
            result = $"{filterOps}<OrderBy><FieldRef Name='{orderby}' Ascending='{orderbyAsc}'/></OrderBy>";
            return result;
        }
        static void UpdateQuery(ref List<string> ops, FormQuery query, Enums.RequestScope? scope, UserEntity? currentUser, bool isAdmin)
        {
            var status = query.Status;
            string op;
            if (status != Enums.RequestStatus.None)
            {
                op = $"<Eq><FieldRef Name='{InterviewKeys.WfStatus}'/><Value Type='Text'>{GetRequestStatus(status)}</Value></Eq>";
                ops.Add(op);
            }
            if (!string.IsNullOrEmpty(query.ItemId))
            {
                op = $"<Eq><FieldRef Name='ID'/><Value Type='Counter'>{query.ItemId}</Value></Eq>";
                ops.Add(op);
            }
            if (query.MinDate != null)
            {
                var mnDate = query.MinDate.GetValueOrDefault().AddDays(1);
                mnDate = new DateTime(mnDate.Year, mnDate.Month, mnDate.Day, 0, 0, 0);
                op = $"<Gt><FieldRef Name='{query.DateField}'/><Value Type='DateTime' IncludeTimeValue='TRUE'>{query.MinDate?.ToString("s")}</Value></Gt>";
                ops.Add(op);
            }
            if (query.MaxDate != null)
            {
                var mxDate = query.MaxDate.GetValueOrDefault().AddDays(1);
                mxDate = new DateTime(mxDate.Year, mxDate.Month, mxDate.Day, 23, 59, 0);
                op = $"<Lt><FieldRef Name='{query.DateField}'/><Value Type='DateTime' IncludeTimeValue='TRUE'>{mxDate.ToString("s")}</Value></Lt>";
                ops.Add(op);
            }
            switch (scope)
            {
                case Enums.RequestScope.MyTasks:
                    if (query.Mode == DashMode.QInterview)
                    {
                        op = $"<Or><Eq><FieldRef Name='{QInterviewKeys.AigId}'/><Value Type='Number'>{currentUser?.SPId}</Value></Eq>" +
                                $"<Eq><FieldRef Name='{QInterviewKeys.AigId2}'/><Value Type='Number'>{currentUser?.SPId}</Value></Eq></Or>";
                        ops.Add(op);
                    }
                    else
                    {
                        op = $"<Or><Eq><FieldRef LookupId='TRUE' Name='{AppConstants.InterviewKeys.Aiguilleur}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>" +
                            $"<Eq><FieldRef LookupId='TRUE' Name='{AppConstants.InterviewKeys.Aiguilleur2}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq></Or>";
                        ops.Add(op);
                    }

                    break;
                case Enums.RequestScope.AllRequests:
                    if (!isAdmin)
                    {
                        if (query.Mode == DashMode.QInterview)
                        {
                            var op1 = $"<Or><Eq><FieldRef Name='{QInterviewKeys.AigId}'/><Value Type='Number'>{currentUser?.SPId}</Value></Eq>" +
                                $"<Eq><FieldRef Name='{QInterviewKeys.AigId2}'/><Value Type='Number'>{currentUser?.SPId}</Value></Eq></Or>";
                            var op2 = $"<Eq><FieldRef Name='{AppConstants.AppKeys.Author}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>";
                            op = $"<Or>{op1}{op2}</Or>";
                            ops.Add(op);
                        }
                        else
                        {
                            var op1 = $"<Or><Eq><FieldRef LookupId='TRUE' Name='{AppConstants.InterviewKeys.Aiguilleur}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>" +
                                $"<Eq><FieldRef LookupId='TRUE' Name='{AppConstants.InterviewKeys.Aiguilleur2}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq></Or>";
                            var op2 = $"<Eq><FieldRef LookupId='TRUE' Name='{AppConstants.AppKeys.Author}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>";
                            op = $"<Or>{op1}{op2}</Or>";
                            ops.Add(op);
                        }
                    }
                    break;
                case Enums.RequestScope.MyRequests:
                    op = $"<Eq><FieldRef LookupId='TRUE' Name='{AppConstants.AppKeys.Author}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>";
                    ops.Add(op);
                    break;
            }

            if (ops.Count == 0 && !isAdmin)
            {
                op = $"<Eq><FieldRef LookupId='TRUE' Name='{AppConstants.AppKeys.Author}'/><Value Type='Lookup'>{currentUser?.SPId}</Value></Eq>";
                ops.Add(op);
            }
            static string GetRequestStatus(RequestStatus? status)
            {
                var r = RequestStatusStr.None;
                if (status.HasValue)
                {
                    switch (status)
                    {
                        case RequestStatus.NotStarted:
                            r = RequestStatusStr.NotStarted;
                            break;
                        case RequestStatus.InProgress:
                            r = RequestStatusStr.InProgress;
                            break;
                        case RequestStatus.Completed:
                            r = RequestStatusStr.Completed;
                            break;

                    }
                }
                return r;
            }
        }


    }
}
