using Newtonsoft.Json;
using VC.AG.Models;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;

namespace VC.AG.WebAPI.Models
{
    public class ReqUpdate
    {
        public string? Site { get; set; }
        public string? List { get; set; }
        public string? FormType { get; set; }
        public int? Id { get; set; }
        public object? Data { get; set; }
        public DBUpdate ToDBUpdate(IUserContract userSvc, bool addAuthor = true)
        {
            dynamic? d1 = JsonConvert.DeserializeObject<dynamic>($"{Data}");
            dynamic d2 = new { fields = d1?["fields"], contentType = d1?["contentType"] };
            var enabledAuthorLists = new string[] { AppConstants.ListNameKeys.Interview, AppConstants.ListNameKeys.RequestAttachments, AppConstants.ListNameKeys.Comment };
            if (enabledAuthorLists.Contains(List?.ToLower()) && addAuthor)
            {
                var user = userSvc.GetMe().Result;
                if (user != null && Data != null)
                {
                        d2.fields.Col_EditorLookupId = user.SPId;
                   if(d2.fields.Col_AuthorLookupId==null) d2.fields.Col_AuthorLookupId = user.SPId;
                }
            }
            var item = new DBUpdate()
            {
                Site = Site,
                ListName = List,
                Id = Id,
                Data = d2
            };
            return item;
        }
      
    }
}
