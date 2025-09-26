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
            public const string AGAdmins = "Admins - AG";
            public const string CTAdmins = "Admins - CT";
            public const string AppAdmins = "Admins - App";
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
            public const string VPRessource = "VP_RESSOURCE";
            public const string VPApiKey = "VP_APIKEY";
            public const string VPApiKey2 = "VP_APIKEY2";


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
            public const string Interview = "etf_interview";
            public const string Contract = "etf_contract";
            public const string QInterview = "etf_quartlyinterview";
            public const string AGActions = "etf_relatedinterview";
            public const string CTActions = "etf_relatedcontract";
            public const string Comment = "etf_requestcomments";
            public const string AGAttachments = "etf_agattachments";
            public const string CTAttachments = "etf_ctattachments";
            public const string DocTemplates = "etf_templates";
            public const string SiteLinks = "etf_link";
            public const string MailTemplate = "etf_mailtemplate";
            public const string Settings = "etf_settings";
            public const string Organization = "etf_organization";
            public const string Bus = "etf_bu";
            public const string ActionTemplates = "etf_tempaction";
            public const string CTAccess = "etf_ctaccess";
            public const string AGAcess = "etf_agaccess";

        }
        public static class AppKeys
        {

            public const string ParentId = "Col_ParentId";
            public const string SiteId = "Col_SiteId";
            public const string SiteUrl = "Col_SiteUrl";
            public const string ListName = "Col_ListName";
            public const string ListId = "Col_WF_ListId";
            public const string FileLeaf = "FileLeafRef";

            public const string WfTarget = "Col_WF_Target";

            public const string WfUrl = "Col_UrlR";
            public const string Author = "Col_Author";
            public const string Editor = "Col_Editor";
            public const string Code = "Col_E_Code";
            public const string Comment = "Col_Comment";
            public const string ParentCode = "Col_E_ParentCode";
            public const string Locale = "Col_E_Locale";
            public const string Name = "Col_E_Name";
            public const string Enabled = "Col_E_Enabled";
            public const string Disabled = "Col_E_Disabled";
            public const string User = "Col_User";
            public const string UserRole = "Col_UserRole";
            public const string Lk_Request = "Col_Lk_Request";
            public const string Lk_Contract = "Col_Lk_Contract";

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
            public const string User = "Col_User";
            public const string Role = "Col_UserRole";
            public const string Level = "Col_LevelRole";
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
        public static class InterviewKeys
        {
            public const string Responsible = "Col_RespUser";
            public const string Responsible2 = "Col_RespUser2";
            public const string StartDate = "Col_StartDate";
            public const string Status = "Col_Status";
            public const string FormType = "Col_FormType";
            public const string FormTarget = "Col_FormTarget";
            public const string ReqRoot = "Col_CtrRootReq";
            public const string Direction = "Col_DirGeneral";
        }
        public static class ContractType
        {
            public const string Aiguilleur = "Aiguilleur";
            public const string StageE = "Stage - Etudiant";
            public const string StageT = "Stage - Tuteur";
            public const string AlternanceE = "Alternance - Etudiant";
            public const string AlternanceT = "Alternance - Tuteur";
            public const string Chantier1 = "Chantier - Intégration";
            public const string Chantier2 = "Chantier - Tuteur - Graduate";
            public const string Chantier3 = "Chantier - RH – Graduate";
            public const string Conducteur1 = "CT - Bilan d'intégration à 1 mois";
            public const string Conducteur2 = "CT - Bilan 1ère affectation";
            public const string Conducteur3 = "CT - Bilan 2nd affectation";
            public const string Conducteur4 = "CT - Bilan RH 1ère affectation";
            public const string Conducteur5 = "CT - Bilan RH 2nd affectation";
        }
        public  class ContractActionType
        {
            public const string None = "None";
            public const string ObjectifMission = "ObjectifMission";
            public const string MissionRealisee = "MissionRealisee";
            public const string AxeProgres = "AxeProgres";
            public const string Engagement = "Engagement";
            public const string Rex = "Rex";
            public const string RexRoot = "RexRoot";
            public const string StageTutAppreciation = "StageTutAppreciation";
            public const string StageEtudAppreciation = "StageEtudAppreciation";
            public const string AlterEtudAppreciation = "AlterEtudAppreciation";
            public const string AlterTutAppreciation = "AlterTutAppreciation";
            public const string ChantierAccueillir = "ChantierAccueillir";
            public const string ChantierSavoirFaire = "ChantierSavoirFaire";
            public const string ChantierSuperviser = "ChantierSuperviser";
            public const string ChantierProgression = "ChantierProgression";
            public const string ChantierFormation = "ChantierFormation";
            public const string ChantierAppreciation = "ChantierAppreciation";
            public const string ChantierFinancier = "ChantierFinancier";
            public const string ChantierContractuel = "ChantierContractuel";
            public const string ChantierPrevention = "ChantierPrevention";
            public const string ChantierCommercial = "ChantierCommercial";
            public const string ConducteurAppreciation = "ConducteurAppreciation";
            public const string ConducteurFinancier = "ConducteurFinancier";
            public const string ConducteurContractuel = "ConducteurContractuel";
            public const string ConducteurPrevention = "ConducteurPrevention";
            public const string ConducteurCommercial = "ConducteurCommercial";
            public const string ConducteurAccueillir = "ConducteurAccueillir";
            public const string ConducteurSavoirFaire = "ConducteurSavoirFaire";
            public const string ConducteurSuperviser = "ConducteurSuperviser";
            public const string ConducteurProgression = "ConducteurProgression";
            public const string ConducteurFormation = "ConducteurFormation";
        }

        public static class QInterviewKeys
        {
            public const string AigId = "F_x003a_Responsable_x0020_ID";
            public const string AigId2 = "F_x003a_Responsable2_ID";
            public const string Direction = "F_x003a_Direction_x0020_g_x00e9_";
            public const string StartDate = "Col_StartDate";
            public const string DueDate = "Col_DueDate";
            public const string Status = "Col_Status";

        }

    }

}
