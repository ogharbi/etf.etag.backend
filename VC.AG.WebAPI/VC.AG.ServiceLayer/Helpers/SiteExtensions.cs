using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.DAO.UnitOfWork;
using VC.AG.Models.Entities;
using static VC.AG.Models.AppConstants;
using VC.AG.Models.Extensions;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.Enums;

namespace VC.AG.ServiceLayer.Helpers
{
    public static class SiteExtensions
    {
        const string fields = "fields";

        public static async Task<SiteEntity> GetRootLists(this SiteEntity site, IUnitOfWork uow)
        {
            var q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.Settings), Expand = fields, Select = SettingsKeys.SelectFields, CatchError = true };
            site.Settings = await uow.DBRepo.GetAll(q);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.MailTemplate), Expand = fields, Select = TranslationKeys.SelectFields };
            site.MailTemplates = await uow.DBRepo.GetAll(q);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.Bus), Expand = fields, Select = SettingsKeys.SelectFields, CatchError = true };
            site.Bus = await uow.DBRepo.GetAll(q);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.ActionTemplates), Expand = fields, Select = SettingsKeys.SelectFields, CatchError = true };
            site.ActionTemplates = await uow.DBRepo.GetAll(q);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.SiteLinks), Expand = fields, Select = SettingsKeys.SelectFields, CatchError = true };
            site.SiteLinks = await uow.DBRepo.GetAll(q);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.CTAccess), Expand = fields, Select = AccessKeys.SelectFields, CatchError = true };
            var ctAccess = await uow.DBRepo.GetAll(q);
            var ctAccessList = GetAccessList(ctAccess, AppTarget.CT);
            q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.AGAcess), Expand = fields, Select = AccessKeys.SelectFields, CatchError = true };
            var agAccess = await uow.DBRepo.GetAll(q);
            var agAccessList = GetAccessList(agAccess, AppTarget.AG);
            var access = ctAccess;
            if (agAccess != null) access = access?.Concat(agAccess);
            var accessList = ctAccessList?.Concat(agAccessList);
            site.Access = access;
            site.AccessList = accessList;
            return site;
        }
        public static async Task<SiteEntity> GetSiteBasicLists(this SiteEntity site, IUnitOfWork uow, SiteRefreshTarget target)
        {
            DBQuery q;
            switch (target)
            {

                case SiteRefreshTarget.SiteLinks:
                    q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.SiteLinks), Expand = fields, Select = SiteLinkKeys.SelectFields, CatchError = true };
                    site.SiteLinks = await uow.DBRepo.GetAll(q);
                    break;
                case SiteRefreshTarget.Access:
                    q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.CTAccess), Expand = fields, Select = AccessKeys.SelectFields, CatchError = true };
                    var ctAccess = await uow.DBRepo.GetAll(q);
                    var ctAccessList = GetAccessList(ctAccess, AppTarget.CT);
                    q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.AGAcess), Expand = fields, Select = AccessKeys.SelectFields, CatchError = true };
                    var agAccess = await uow.DBRepo.GetAll(q);
                    var agAccessList = GetAccessList(agAccess, AppTarget.AG);
                    var access = ctAccess;
                    if (agAccess != null) access = access?.Concat(agAccess);
                    var accessList = ctAccessList?.Concat(agAccessList);
                    site.Access = access;
                    site.AccessList = accessList;
                    break;


            }
            return site;
        }
        public static async Task<SiteEntity> GetSiteLists(this SiteEntity site, IUnitOfWork uow)
        {
            var q = new DBQuery() { SiteId = site.Id, ListId = site.Lists?.GetStringValue2(ListNameKeys.SiteLinks), Expand = fields, Select = SiteLinkKeys.SelectFields, CatchError = true };
            site.SiteLinks = await uow.DBRepo.GetAll(q);

            var selectedLists = new List<string>
                        {
                            ListNameKeys.Interview
                        };
            foreach (var listName in selectedLists)
            {
                q = new DBQuery() { SiteId = site.Id, SiteUrl = site.SiteUrl, ListId = site.Lists?.GetStringValue2(listName), CatchError = true };
                var views = await uow.DBRepo.GetListViews(q);
                var contentTypes = await uow.DBRepo.GetListContentTypes(q);
                var columns = await uow.DBRepo.GetListColumns(q);
                var listMeta = site.ListsMeta?.ContainsKey(listName) == true ? site.ListsMeta?[listName] : null;
                if (listMeta != null)
                {
                    listMeta.Columns = columns;
                    listMeta.ContentTypes = contentTypes;
                    listMeta.Views = views;
                }
            }
            return site;
        }
        public static IEnumerable<Access> GetAccessList(IEnumerable<DBItem>? access, AppTarget target)
        {
            var result = new List<Access>();
            if (access != null)
            {
                foreach (var item in access)
                {
                    var values = item.Values;
                    if (values == null) continue;
                    var a = new Access
                    {
                        Id = int.Parse($"{item.Id}"),
                        AppTarget = target,
                        Role = values.GetStringValue2(AccessKeys.Role),
                        User = values.GetUserValue3(AccessKeys.User),
                        Level = values.GetStringValue2(AccessKeys.Level),
                    };
                    result.Add(a);
                }
            }
            return result;
        }
    }
}
