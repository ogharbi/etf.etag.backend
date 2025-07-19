using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VC.AG.Models.Enums;

namespace VC.AG.Models.Helpers
{
    public static class CamlHelper
    {
        public static string BuildCondition(string name, string value, SPFieldType type, CamlOp op, bool includeWhere=false)
        {
            var cond = $"<{op}><FieldRef Name=\"{name}\"/><Value Type=\"{type}\">{value}</Value></{op}>";
            var result = includeWhere ? $"<Where>{cond}</Where>" : cond;
            return result;
        }
        public static string BuildQuery(List<string> ops, string op)
        {
            var result = string.Empty;
            if (ops.Count == 1)
            {
                result = ops[0];
            }
            else if (ops.Count == 2)
            {
                result = $"<{op}>{ops[0]}{ops[1]}</{op}>";
            }
            else if (ops.Count > 2)
            {
                result = $"<{op}>{ops[0]}{ops[1]}</{op}>";
                for (var i = 2; i < ops.Count; i++)
                {
                    result = $"<{op}>{result}{ops[i]}</{op}>";
                }
            }
            return result;
        }
    }
}
