using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.ValuesObject;

namespace VC.AG.Models.Extensions
{
    public static class SpoExtensions
    {
        public static void SetLookupValue(this ListItem item, string fieldName, RefItem value)
        {
            try
            {
                if (value != null && (value.Id != 0))
                {
                    FieldLookupValue v = new()
                    {
                        LookupId = value.Id.GetValueOrDefault()
                    };
                    item[fieldName] = v;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
