using System.ComponentModel.DataAnnotations;
using VC.AG.Models.Entities;
using VC.AG.Models.Enums;

namespace VC.AG.Models.ValuesObject
{
    public class Access
    {
        public int? Id { get; set; }
        public string? Site { get; set; }
        public string? Role { get; set; }
        public string? Level { get; set; }
        public AppTarget AppTarget { get; set; }
        public UserEntity? User { get; set; }
        public bool? Validator { get; set; }
    }
}
