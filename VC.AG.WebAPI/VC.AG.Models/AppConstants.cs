using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VC.AG.Models
{
    public class AppConstants
    {
        public const string ApiVersion = "1.0";
        public const string EnCulture = "en-US";
        public static class SiteGroups
        {
            public const string Admins = "AG admins";
            public const string Users = "AG Users";
        }
        public static class Commun
        {
            public const string UnauthorizedOp = "Unauthorized operation";
            public const string NotFoundOp = "404NotFound";
        }
        public static class AppSettingsKeys
        {

            public const string ClientSecret = "AzureAd-ClientSecret";
            public const string SPOPwd = "SPO-Pwd";
            public const string AppSmtpPwd = "APP-SmtpPwd";


            public const string SPOUrl = "SPO_URL";
            public const string SPOUser = "SPO_User";
            public const string SPOUrl2 = "SPO_URL2";

            public const string ClientId = "AzureAd_ClientId";

            public const string TenantId = "AzureAd_TenantId";
            public const string Authority = "AzureAd_Authority";
            public const string Resource = "AzureAd_Resource";
            public const string Domain = "AzureAd_Domain";
            public const string Instance = "AzureAd_Instance";

            public const string AppUrl = "App_URL";
            public const string AppVersion = "App_Version";
            public const string AppSmtpEnabled = "App_SmtpEnabled";
            public const string AppSmtpServer = "App_SmtpServer";
            public const string AppSmtpFrom = "App_SmtpFrom";
            public const string AppSmtpModern = "App_SmtpModern";
            public const string AppSmtpUser = "App_SmtpUser";
            public const string AppReminderList = "App_ReminderList";
            public const string APPEnv = "APP_Env";


            public const string AppCc = "App_Cc";


            public const string keyVault_Uri = "keyVault_Uri";


        }
        public static class CacheKeysKeys
        {
            public const string SiteRootInfo = "SPO:SiteRootInfo";
            public const string SiteInfo = "SPO:SiteInfo";
            public const string SitesInfo = "SPO:SitesInfo";

        }
        public static class MailLogsKeys
        {

            public const string To = "Col_To";
            public const string Cc = "Col_Cc";
            public const string Body = "Col_BodyR";
            public const string Status = "Col_Status";
            public const string Logs = "Col_Logs";
            public const string NotifType = "Col_NotifType";

        }
        public static class ListNameKeys
        {
            public const string Request = "vc_interview";
            public const string Comment = "vc_requestcomments";
            public const string RequestAttachments = "vc_requestattachments";
            public const string SiteLinks = "vc_link";
            public const string MailTemplate = "vc_mailtemplate";
            public const string Settings = "vc_settings";
            public const string Bus = "vc_bu";

        }
        public static class AppKeys
        {

            public const string ParentId = "Col_ParentId";
            public const string SiteId = "Col_SiteId";
            public const string SiteUrl = "Col_SiteUrl";
            public const string ListName = "Col_ListName";
            public const string ListId = "Col_WF_ListId";
            public const string FileLeaf = "FileLeafRef";

            public const string FormType = "Col_WF_FormType";
            public const string WfTarget = "Col_WF_Target";

            public const string WfUrl = "Col_UrlR";
            public const string Author = "Col_Author";
            public const string Editor = "Col_Editor";
            public const string Code = "Col_E_Code";
            public const string ParentCode = "Col_E_ParentCode";
            public const string Locale = "Col_E_Locale";
            public const string Name = "Col_E_Name";
            public const string Enabled = "Col_E_Enabled";
            public const string Disabled = "Col_E_Disabled";
            public const string User = "Col_E_User";
            public const string UserRole = "Col_E_UserRole";
            public const string Lk_Request = "Col_Lk_Request";

        }
        public static class TranslationKeys
        {
            public const string FrFR_Name = "Col_FrFR_Name";
            public const string EnUS_Name = "Col_EnUS_Name";
            public const string CzCZ_Name = "Col_CzCZ_Name";
            public const string DeDE_Name = "Col_DeDE_Name";
            public const string EsES_Name = "Col_EsES_Name";
            public const string PlPL_Name = "Col_PlPL_Name";
            public const string SelectFields = $"Title,{AppKeys.Code},{FrFR_Name},{EnUS_Name},{CzCZ_Name},{DeDE_Name},{EsES_Name},{PlPL_Name},{AppKeys.Disabled}";
        }
        public static class LocaleKeys
        {
            public const string Fr = "Fr-FR";
            public const string En = "En-US";
            public const string Pl = "Pl-PL";
            public const string Cz = "Cz-CZ";
            public const string Es = "Es-ES";
            public const string De = "De-DE";
            public const string Default = "Fr-FR";
        }
        public static class SettingsKeys
        {
            public const string SelectFields = $"Title,{AppKeys.Code}";

        }
        public static class MailTemplateKeys
        {
            public const string Subject = "Col_Subject";
            public const string Body = "Col_BodyR";

        }
        public static class SiteLinkKeys
        {
            public const string LinkUrl = "Col_LinkUrl";
            public const string LinkTarget = "Col_LinkTarget";
            public const string NewTab = "Col_NewTab";
            public const string Order = "Col_Order";
            public const string SelectFields = $"Title,{LinkUrl},{LinkTarget},{NewTab},{Order}";

        }
        public static class AccessKeys
        {
            public const string User = "Col_E_User";
            public const string Role = "Col_E_UserRole";
            public const string Level = "Col_E_LevelRole";
            public const string SelectFields = $"Title,{User},{Role},{AppKeys.Code},{Level}";
        }
        public static class RequestAttachmentKeys
        {
            public const string EEDNode = "Col_EEqNode";
            public const string Code2 = "Col_E_Code2";

        }
        public static class RequestCommentKeys
        {
            public const string Comment = "Col_Comment";

        }
        public static class RequestKeys
        {
            public const string WfStatus = "Col_S_WfStatus";
        }
       
    }

}
