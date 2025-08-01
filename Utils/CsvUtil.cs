using CsvHelper;
using System.Globalization;
using System.Text;

namespace PhrazorApp.Utils
{
    public class CsvUtil
    {
        public static async Task<IEnumerable<T>> ReadCsvAsync<T>(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<T>();

            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                records.Add(record);
            }

            return records;
        }
    }
}
