using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HandlebarsDotNet;
namespace VC.AG.Models.Helpers
{
    public class HandlebarsBlockHelper
    {
        public static void AddHelpers()
        {
            Handlebars.RegisterHelper("incremented", (output, context, arguments) =>
            {
                if (Convert.ToString(arguments[0]) != string.Empty)
                {
                    try
                    {
                        var index = Convert.ToInt32(arguments[0]);
                        index++;
                        output.WriteSafeString(index);
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            Handlebars.RegisterHelper("formatInt", (output, context, arguments) =>
            {
                if (Convert.ToString(arguments[0]) != string.Empty)
                {
                    try
                    {
                        var a = ("" + arguments[0]).Split('.')[0];
                        var index = String.Format("{0:0.##}", Convert.ToDecimal(a));
                        var number = Convert.ToInt32(a);
                        output.WriteSafeString(number);
                    }
                    catch (Exception ex)
                    {
                        output.WriteSafeString("" + arguments[0]);
                    }
                }
            });
            Handlebars.RegisterHelper("formatDecimal", (output, context, arguments) =>
            {
                if (Convert.ToString(arguments[0]) != string.Empty)
                {
                    try
                    {
                        var index = String.Format("{0:0.##}", Convert.ToDecimal(arguments[0]));
                        output.WriteSafeString(index);
                    }
                    catch (Exception)
                    {
                        output.WriteSafeString(arguments[0]);
                    }
                }
            });
            Handlebars.RegisterHelper("roundDecimal", (output, context, arguments) =>
            {
                if (Convert.ToString(arguments[0]) != string.Empty)
                {
                    try
                    {
                        var index = Convert.ToDecimal(arguments[0]);
                        index = Math.Ceiling(index);
                        output.WriteSafeString(index);
                    }
                    catch (Exception)
                    {
                        output.WriteSafeString(arguments[0]);
                    }
                }
            });

            Handlebars.RegisterHelper("convertNewline", (output, context, arguments) =>
            {
                if (Convert.ToString(arguments[0]) != string.Empty)
                {
                    string v = Convert.ToString(arguments[0]);
                    string replacement = "<br/>";
                    var line = v.Replace("\r\n", replacement)
                      .Replace("\r", replacement)
                      .Replace("\n", replacement);
                    output.WriteSafeString(line);
                }
            });

            Handlebars.RegisterHelper("ConvertToSign", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    var fieldName = arguments.At<string>(0);
                    var refCode = values.ContainsKey(fieldName) ? "" + values[fieldName] : string.Empty;
                    var signKey = $"SignRef_{refCode}";
                    var signture = values.ContainsKey(signKey) ? "" + values[signKey] : string.Empty;
                    if (!string.IsNullOrEmpty(signture)) output.WriteSafeString($"<td style=\"background:url('{signture}');background-size:contain;background-position:center;background-repeat: no-repeat;\"></td>");
                    else output.WriteSafeString($"<td>{refCode}</td>");

                }
                catch (Exception)
                {

                    output.WriteSafeString("<td></td>");
                }

            });
            Handlebars.RegisterHelper("ConvertToSign2", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    var refCode = arguments.At<string>(0);
                    var pValues = arguments.At<Dictionary<string, object>>(1);
                    var signKey = $"SignRef_{refCode}";
                    var signture = pValues.ContainsKey(signKey) ? "" + pValues[signKey] : string.Empty;
                    if (!string.IsNullOrEmpty(signture)) output.WriteSafeString($"<td style=\"background:url('{signture}');background-size:contain;background-position:center;background-repeat: no-repeat;\"></td>");
                    else output.WriteSafeString("<td></td>");

                }
                catch (Exception)
                {

                    output.WriteSafeString("<td></td>");
                }

            });
            Handlebars.RegisterHelper("ConvertToSign3", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    var refCode = arguments.At<string>(0);
                    var signKey = $"SignRef_{refCode}";
                    var signture = values.ContainsKey(signKey) ? "" + values[signKey] : string.Empty;
                    if (!string.IsNullOrEmpty(signture)) output.WriteSafeString($"<td style=\"background:url('{signture}');background-size:contain;background-position:center;background-repeat: no-repeat;\"></td>");
                    else output.WriteSafeString("<td></td>");

                }
                catch (Exception)
                {

                    output.WriteSafeString("<td></td>");
                }

            });
            Handlebars.RegisterHelper("SuspensionBlockHelper", (output, options, context, arguments) =>
            {
                try
                {
                    if (arguments.Length == 2)
                    {
                        Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                        var leftName = "" + arguments.At<string>(0);
                        var rightVal = "" + arguments.At<string>(1);
                        DateTime now = DateTime.Now.AddDays(-1);
                        DateTime.TryParse(leftName, out DateTime startDate);
                        DateTime.TryParse(rightVal, out DateTime endDate);

                        //IF Suspendu
                        if (startDate != DateTime.MinValue && now >= startDate && (endDate == DateTime.MinValue || now <= endDate))
                        {
                            output.WriteSafeString("");
                        }
                        else
                        {
                            options.Template(output, context);
                        }
                    }
                    else
                    {
                        output.WriteSafeString("");
                    }
                }
                catch (Exception ex)
                {

                    output.WriteSafeString("");
                }

            });

            Handlebars.RegisterHelper("StringEqualityBlockHelper", (output, options, context, arguments) =>
            {
                try
                {
                    if (arguments.Length == 2)
                    {
                        Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                        var leftName = arguments.At<string>(0);
                        var rightVal = arguments.At<string>(1);
                        var leftVal = "" + leftName; //values.ContainsKey(leftName) ? "" + values[leftName] : string.Empty;
                        if (leftVal.Equals(rightVal, StringComparison.OrdinalIgnoreCase)) options.Template(output, context);
                        else options.Inverse(output, context);
                    }
                    else
                    {
                        output.WriteSafeString("");
                    }
                }
                catch (Exception)
                {

                    output.WriteSafeString("");
                }

            });
            Handlebars.RegisterHelper("StringNullOrEmptyBlockHelper", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    var left = arguments.At<string>(0);
                    var val = values.ContainsKey(left) ? "" + values[left] : string.Empty;
                    if (string.IsNullOrEmpty(val)) options.Template(output, context);
                    else options.Inverse(output, context);

                }
                catch (Exception ex)
                {

                    output.WriteSafeString("");
                }

            });
            Handlebars.RegisterHelper("StringNotNullOrEmptyBlockHelper", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    var left = arguments.At<string>(0);
                    var val = values.ContainsKey(left) ? "" + values[left] : string.Empty;
                    if (!string.IsNullOrEmpty(val)) options.Template(output, context);
                    else options.Inverse(output, context);

                }
                catch (Exception ex)
                {

                    output.WriteSafeString("");
                }

            });
            Handlebars.RegisterHelper("compare", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    int part1 = 0;
                    var key1 = arguments.At<string>(0);
                    var val1 = values.ContainsKey(key1) ? "" + values[key1] : "0";
                    int.TryParse(val1, out part1);
                    int part2 = 0;
                    var key2 = arguments.At<string>(2);
                    var val2 = values.ContainsKey(key2) ? "" + values[key2] : "0";
                    int.TryParse(val2, out part2);
                    var op = arguments.At<string>(1);
                    var result = false;
                    switch (op)
                    {
                        case "=": result = part1 == part2; break;
                        case "!=": result = part1 != part2; break;
                        case ">=": result = part1 >= part2; break;
                        case "<=": result = part1 <= part2; break;
                        case ">": result = part1 > part2; break;
                        case "<": result = part1 < part2; break;
                    }

                    if (result) options.Template(output, context);
                    else options.Inverse(output, context);

                }
                catch (Exception ex)
                {

                    output.WriteSafeString("");
                }

            });
            Handlebars.RegisterHelper("compareValue", (output, options, context, arguments) =>
            {
                try
                {
                    Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                    int part1 = 0;
                    var key1 = arguments.At<string>(0);
                    var val1 = values.ContainsKey(key1) ? "" + values[key1] : "0";
                    int.TryParse(val1, out part1);
                    int part2 = 0;
                    var val2 = arguments.At<string>(2);
                    int.TryParse(val2, out part2);
                    var op = arguments.At<string>(1);
                    var result = false;
                    switch (op)
                    {
                        case "=": result = part1 == part2; break;
                        case "!=": result = part1 != part2; break;
                        case ">=": result = part1 >= part2; break;
                        case "<=": result = part1 <= part2; break;
                        case ">": result = part1 > part2; break;
                        case "<": result = part1 < part2; break;
                    }

                    if (result) options.Template(output, context);
                    else options.Inverse(output, context);

                }
                catch (Exception ex)
                {

                    output.WriteSafeString("");
                }

            });



            Handlebars.RegisterHelper("formatDate", (output, context, arguments) =>
            {
                try
                {
                    if (Convert.ToString(arguments[0]) != string.Empty)
                    {
                        var d = Convert.ToDateTime(arguments[0].ToString());
                        if (d == DateTime.MinValue)
                            output.Write(string.Empty);
                        else
                        {
                            DateTime dt = d;
                            if (dt != DateTime.MinValue)
                            {
                                if (arguments.Length > 1)
                                {
                                    string dateFormat = arguments[1].ToString();
                                    output.WriteSafeString(string.Format(dateFormat, dt));
                                }
                                else
                                {
                                    output.WriteSafeString(dt.ToLocalTime().ToString("dd/MM/yyyy"));
                                }
                            }
                            else
                                output.WriteSafeString(string.Empty);

                        }
                    }
                    else
                        output.WriteSafeString(string.Empty);
                }
                catch (Exception exc)
                {
                    //output.Write(arguments[0].ToString());
                    output.Write(string.Empty);
                }
            });
            Handlebars.RegisterHelper("formatDateS", (output, context, arguments) =>
            {
                try
                {
                    if (Convert.ToString(arguments[0]) != string.Empty)
                    {
                        Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                        var param1 = arguments[0].ToString();
                        var d = DateTime.MinValue;
                        if (param1.EndsWith("."))
                            d = Convert.ToDateTime(values[param1]);
                        else d = Convert.ToDateTime(param1);
                        if (d == DateTime.MinValue)
                            output.Write(string.Empty);
                        else
                        {
                            DateTime dt = d;
                            if (dt != DateTime.MinValue)
                            {
                                if (arguments.Length > 1)
                                {
                                    string dateFormat = arguments[1].ToString();
                                    output.WriteSafeString(string.Format(dateFormat, dt));
                                }
                                else
                                {
                                    output.WriteSafeString(dt.ToLocalTime().ToString("dd/MM/yy"));
                                }
                            }
                            else
                                output.WriteSafeString(string.Empty);

                        }
                    }
                    else
                        output.WriteSafeString(string.Empty);
                }
                catch (Exception exc)
                {
                    //output.Write(arguments[0].ToString());
                    output.Write(string.Empty);
                }
            });
            Handlebars.RegisterHelper("formatDateS2", (output, context, arguments) =>
            {
                try
                {
                    if (Convert.ToString(arguments[0]) != string.Empty)
                    {
                        Dictionary<string, object> values = (Dictionary<string, object>)context.Value;
                        var param1 = arguments[0].ToString();
                        var d = DateTime.MinValue;
                        if (param1.EndsWith("."))
                            d = Convert.ToDateTime(values[param1]);
                        else d = Convert.ToDateTime(param1);
                        if (d == DateTime.MinValue)
                            output.Write(string.Empty);
                        else
                        {
                            DateTime dt = d;
                            if (dt != DateTime.MinValue)
                            {
                                if (arguments.Length > 1)
                                {
                                    string dateFormat = arguments[1].ToString();
                                    output.WriteSafeString(string.Format(dateFormat, dt));
                                }
                                else
                                {
                                    output.WriteSafeString(dt.ToLocalTime().ToString("dd/MM/yyyy"));
                                }
                            }
                            else
                                output.WriteSafeString(string.Empty);

                        }
                    }
                    else
                        output.WriteSafeString(string.Empty);
                }
                catch (Exception exc)
                {
                    //output.Write(arguments[0].ToString());
                    output.Write(string.Empty);
                }
            });
            Handlebars.RegisterHelper("formatPk", (output, context, arguments) =>
            {
                try
                {
                    var v0 = "" + arguments[0];
                    if (Convert.ToString(v0) != string.Empty)
                    {
                        v0 = Regex.Replace(v0, @"\p{Zs}", "");
                        var value = double.Parse(v0.Trim().Replace(" ", ""));
                        var p1 = Math.Floor(value / 1000).ToString("N0");
                        var p2 = Math.Floor(value % 1000).ToString("N0");
                        var p3 = (value - Math.Floor(value)).ToString("N2").Split(".")[1];
                        if (p2.Length == 1) p2 = $"00{p2}";
                        else if (p2.Length == 2) p2 = $"0{p2}";
                        var description2 = p3 == "00" ? $"{p1}+{p2}" : $"{p1}+${p2},${p3}";

                        output.WriteSafeString(description2);
                    }
                    else
                        output.WriteSafeString(string.Empty);
                }
                catch (Exception exc)
                {
                    output.Write(string.Empty);
                }
            });
            Handlebars.RegisterHelper("checkboxFor", (output, context, arguments) =>
            {
                try
                {
                    if (arguments.Length < 2)
                    {
                        output.WriteSafeString("<input type=\"checkbox\" />");
                    }
                    else
                    {
                        if (arguments[0] != null && arguments[1] != null)
                        {
                            var leftValue = arguments[0].ToString();
                            var rightValue = arguments[1].ToString();
                            if (leftValue.Equals(rightValue, StringComparison.OrdinalIgnoreCase))
                                output.WriteSafeString("<input type=\"checkbox\" checked=\"checked\" />");
                            else
                                output.WriteSafeString("<input type=\"checkbox\" />");
                        }
                        else
                        {
                            output.WriteSafeString("<input type=\"checkbox\" />");
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteSafeString("<input type=\"checkbox\" />");
                }
            });
            Handlebars.RegisterHelper("checkboxForIn", (output, context, arguments) =>
            {
                try
                {
                    if (arguments.Length < 2)
                    {
                        output.WriteSafeString("<input type=\"checkbox\" />");
                    }
                    else
                    {
                        if (arguments[0] != null && arguments[1] != null)
                        {
                            var leftValue = arguments[0].ToString();
                            var rightValue = arguments[1].ToString();
                            if (leftValue.Contains(rightValue, StringComparison.OrdinalIgnoreCase))
                                output.WriteSafeString("<input type=\"checkbox\" checked=\"checked\" />");
                            else
                                output.WriteSafeString("<input type=\"checkbox\" />");
                        }
                        else
                        {
                            output.WriteSafeString("<input type=\"checkbox\" />");
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteSafeString("<input type=\"checkbox\" />");
                }
            });
            Handlebars.RegisterHelper("writePossibleNull", (output, context, arguments) =>
            {
                try
                {
                    if (arguments.Length == 0)
                        throw new Exception("Handlebars Helper equal needs 1 parameter");

                    var value = arguments[0].ToString();

                    if (null != value)
                        output.WriteSafeString((string)value);
                    else
                        output.WriteSafeString(string.Empty);
                }
                catch (Exception ex)
                {
                    output.WriteSafeString(string.Empty);
                }
            });

        }
    }

}
