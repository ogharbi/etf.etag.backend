using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqForms : ReqQuery
    {
        public RequestStatus? Status { get; set; }
        public RequestScope? Scope { get; set; }
        public TaskTarget? TaskTarget { get; set; }
        public string? ContentTypeId { get; set; }
        public string? FormType { get; set; }
        public bool? InlineQuery { get; set; }
        public FormQuery ToFormQuery(UserEntity? user)
        {
            var q = new FormQuery()
            {
                ItemId = ItemId,
                ListName = ListName,
                Filter = Filter,
                InlineQuery= InlineQuery,
                Expand = Expand,
                Select = Select,
                Fields = Fields,
                OrderBy = OrderBy,
                Top = Top,
                Skip = Skip,
                SearchTerm = SearchTerm,
                AppendQuery = AppendQuery,
                Status=Status,
                Scope=Scope,
                TaskTarget=TaskTarget,
                ContentTypeId= ContentTypeId,
                FormType=FormType,
                NextHref = string.IsNullOrEmpty(NextHref) ? null : NextHref[1..],
                User=user
            };
            return q;
        }
    }
}
