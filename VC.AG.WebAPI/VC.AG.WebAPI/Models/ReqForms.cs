﻿using VC.AG.Models.Entities;
using VC.AG.Models.Enums;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class ReqForms : ReqQuery
    {
        public RequestStatus? Status { get; set; }
        public RequestScope? Scope { get; set; }
        public DashMode? Mode { get; set; }
        public string? AigField { get; set; }
        //AiguilleurField
        public string? AgField { get; set; }
        public string? DateField { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
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
                Data=AgField,
                AigField = AigField,
                DateField=DateField,
                MinDate=MinDate,
                MaxDate=MaxDate,
                NextHref = string.IsNullOrEmpty(NextHref) ? null : NextHref[1..],
                User=user,
                Mode=Mode
            };
            return q;
        }
    }
}
