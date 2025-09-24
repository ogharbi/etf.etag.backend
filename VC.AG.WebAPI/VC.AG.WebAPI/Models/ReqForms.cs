using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqForms : ReqQuery
    {
        public RequestStatus? Status { get; set; }
        public RequestScope? Scope { get; set; }
        public DashTarget? DashTarget { get; set; }
        public string? AigField { get; set; }
        //AiguilleurField
        public string? AgField { get; set; }
        public string? DateField { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public bool? InlineQuery { get; set; }
        public string? FormType { get; set; }
        public string? FormTarget{ get; set; }
        public bool? OnlyChildren { get; set; }

        

        public FormQuery ToFormQuery(UserEntity? user)
        {
            var q = new FormQuery()
            {
                AppTarget=AppTarget,
                ItemId = ItemId,
                ListName = ListName,
                Filter = Filter,
                FormType=FormType,
                FormTarget= FormTarget,
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
                Data=AgField,
                AigField = AigField,
                DateField=DateField,
                MinDate=MinDate,
                MaxDate=MaxDate,
                NextHref = string.IsNullOrEmpty(NextHref) ? null : NextHref[1..],
                User=user,
                DashTarget=DashTarget,
                OnlyChildren=OnlyChildren
            };
            return q;
        }
    }
}
