using Newtonsoft.Json;
using VC.AG.Models;
using VC.AG.Models.ValuesObject;
using VC.AG.ServiceLayer.Contracts;

namespace VC.AG.WebAPI.Models
{
    public class ReqCreate
    {
        public string? Site { get; set; }
        public string? List { get; set; }
        public object? Data { get; set; }
        public DBCreate ToDBCreate(IUserContract userSvc)
        {
            dynamic? d1 = JsonConvert.DeserializeObject<dynamic>($"{Data}");
            dynamic d2 = new { fields = d1?["fields"] };
            var enabledAuthorLists = new string[] { AppConstants.ListNameKeys.Interview, AppConstants.ListNameKeys.QInterview, AppConstants.ListNameKeys.Actions, AppConstants.ListNameKeys.RequestAttachments, AppConstants.ListNameKeys.Comment };
            if (enabledAuthorLists.Contains(List?.ToLower()))
            {
                var user = userSvc.GetMe().Result;
                if (user != null && Data!=null)
                {
                   
                        d2.fields.Col_AuthorLookupId = user.SPId;
                        d2.fields.Col_EditorLookupId = user.SPId;
                    
                }
            }
            var item = new DBCreate()
            {
                Site = Site,
                ListName = List,
                Data = d2
            };
            return item;
        }
    }


}
