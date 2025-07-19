using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.ApplicationPages.ClientPickerQuery;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using Newtonsoft.Json;
using VC.AG.DAO.Contracts;
using VC.AG.Models;
using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;
using VC.AG.Models.ValuesObject.SPContext;
namespace VC.AG.DAO.Respository
{
    public class UserRepository(IConfiguration config, IMemoryCache cache) : IUserRepository
    {
        readonly SpoContext spoContext = new(config, cache);

        public async Task<UserEntity?> Get(string? email = null, int? id = null)
        {
            UserEntity? result = null;
            var ctx = spoContext.GetClientContext();
            if (!string.IsNullOrEmpty(email))
            {
                var r = Utility.ResolvePrincipal(ctx, ctx.Web, email, PrincipalType.User, PrincipalSource.All, null, true);
                await ctx.ExecuteQueryAsync();
                if (r != null)
                {
                    var user = ctx.Web.EnsureUser(r.Value.LoginName);
                    ctx.Load(user, u => u.Id, u => u.Title, u => u.Email, u => u.LoginName, u => u.Groups, u => u.IsSiteAdmin);
                    await ctx.ExecuteQueryAsync();
                    result = new(user);
                }
            }
            if (id.HasValue)
            {
                var user = ctx.Web.GetUserById(id.Value);
                ctx.Load(user, u => u.Id, u => u.Title, u => u.Email, u => u.LoginName, u => u.Groups, u => u.IsSiteAdmin);
                await ctx.ExecuteQueryAsync();
                result = new(user);
            }
            return result;
        }



        public async Task<IEnumerable<UserEntity>?> Search(string word)
        {
            var cc = spoContext.GetClientContext();
            ClientPeoplePickerQueryParameters querryParams = new()
            {
                AllowMultipleEntities = false,
                MaximumEntitySuggestions = 50,
                PrincipalSource = PrincipalSource.All,
                PrincipalType = PrincipalType.User,
                QueryString = word
            };
            // //execute query to Sharepoint
            ClientResult<string> clientResult = ClientPeoplePickerWebServiceInterface.ClientPeoplePickerSearchUser(cc, querryParams);
            await cc.ExecuteQueryAsync();
            var s = clientResult.Value;
            var users = JsonConvert.DeserializeObject<List<PeoplePicker>>(s);
            var lstUsers = new List<UserEntity>();
            if (users != null)
            {
                foreach (var u in users)
                {
                    lstUsers.Add(new UserEntity() { AccountName = u.Key, DisplayName = u.DisplayText, Email = u.EntityData?.Email });
                }
            }
            return lstUsers;
        }
        public async Task<string?> UpdateAssignment(AssignAccess assign)
        {
            var cc = spoContext.GetClientContext($"{assign.SiteUrl}");
            Web web = cc.Web;
            cc.Load(web, w => w.AssociatedOwnerGroup, w => w.AssociatedMemberGroup, w => w.AssociatedVisitorGroup, w => w.Title);
            await cc.ExecuteQueryAsync();
            Group? targetGroup = null;
            var user = cc.Web.GetUserById(assign.UserId.GetValueOrDefault());
            switch (assign.Role)
            {
                case Models.Enums.UserRole.None:
                    break;
                case Models.Enums.UserRole.Admin:
                    targetGroup = web.AssociatedOwnerGroup;
                    break;
                case Models.Enums.UserRole.User:
                    break;
                case Models.Enums.UserRole.Reader:
                    break;
                default:
                    break;
            }
            if (targetGroup != null)
            {
                cc.Load(user);
                cc.Load(targetGroup);
                await cc.ExecuteQueryAsync();
                switch (assign.Action)
                {
                    case Models.Enums.AssignAction.None:
                        break;
                    case Models.Enums.AssignAction.Add:
                        targetGroup.Users.AddUser(user);
                        break;
                    case Models.Enums.AssignAction.Remove:
                        var users = targetGroup.Users;
                        cc.Load(users, a => a.Include(b => b.Id));
                        await cc.ExecuteQueryAsync();
                        if (users.Any(a => a.Id == assign.UserId))
                        {
                            targetGroup.Users.RemoveById(assign.UserId.GetValueOrDefault());
                        }

                        break;
                    default:
                        break;
                }
                await cc.ExecuteQueryAsync();
            }

            return "OK";
        }


    }
}
