using VC.AG.Models.Entities;
using VC.AG.Models.ValuesObject;

namespace VC.AG.WebAPI.Models
{
    public class FormFetch
    {
        public string? Site { get; set; }
        public string? ListName { get; set; }
        public string? ItemId { get; set; }
        public string? FormType { get; set; }
        public string? Filter { get; set; }
        public DBQuery ToDGQuery(UserEntity? user)
        {
            var dbQuery = new DBQuery
            {
                ListName=ListName,
                ItemId=ItemId,
                Filter= Filter,
                User=user   
            };
            return dbQuery;
        }
    }
}
