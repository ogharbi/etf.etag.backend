using System.Text.Json;
using System.Text.Json.Serialization;

namespace VC.AG.Models.Helpers
{
    public class DoubleInfinityConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            try
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    writer.WriteStringValue(default(double).ToString());
                    return;
                }
                writer.WriteStringValue(value.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DoubleInfinityConverter {value} : {ex.Message}");
                writer.WriteStringValue(string.Empty);
            }

        }
    }
}
