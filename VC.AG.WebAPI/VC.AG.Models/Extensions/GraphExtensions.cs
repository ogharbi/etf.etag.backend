using Microsoft.Graph;

namespace VC.AG.Models.Extensions
{
    public static class GraphExtensions
    {
        public static string GetStringValue(this ListItem item, string fieldName)
        {
            var success = item.Fields.AdditionalData.TryGetValue(fieldName, out var value);
            var result = success ? $"{value}" : string.Empty;
            return result;
        }
        public static DateTime? GetDateTimeValue(this ListItem item, string fieldName)
        {
            DateTime? r = null;
            try
            {
                var value = item.Fields.AdditionalData[fieldName];
                if (value != null)
                    r = Convert.ToDateTime(value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static bool GetBoolValue(this ListItem item, string fieldName)
        {
            bool r = false;
            try
            {
                r = Convert.ToBoolean(item.Fields.AdditionalData[fieldName]);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static decimal? GetDecimalValue(this ListItem item, string fieldName)
        {
            decimal? r = null;
            try
            {
                var value = item.Fields.AdditionalData[fieldName];
                if (value != null)
                    r = Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
        public static int? GetIntValue(this ListItem item, string fieldName)
        {
            int? r = null;
            try
            {
                var value = item.Fields.AdditionalData[fieldName];
                if (value != null)
                    r = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{fieldName} : {ex.Message}");
            }
            return r;
        }
    }
}
