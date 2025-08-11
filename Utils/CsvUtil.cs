using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace PhrazorApp.Utils
{
    public static class CsvUtil
    {
        private static CsvConfiguration DefaultConfig => new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        public static async IAsyncEnumerable<T> ReadAsync<T>(
            Stream stream,
            CsvConfiguration? config = null,
            ClassMap<T>? classMap = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, config ?? DefaultConfig);
            if (classMap != null) csv.Context.RegisterClassMap(classMap);

            await foreach (var record in csv.GetRecordsAsync<T>(ct))
            {
                ct.ThrowIfCancellationRequested();
                yield return record;
            }
        }

        public static async Task<List<T>> ReadToListAsync<T>(
            Stream stream,
            CsvConfiguration? config = null,
            ClassMap<T>? classMap = null,
            CancellationToken ct = default)
        {
            var list = new List<T>();
            await foreach (var r in ReadAsync(stream, config, classMap, ct))
                list.Add(r);
            return list;
        }

        public static async Task<(List<T> Records, List<(int RowNumber, string Message)> Errors)> ReadWithValidateAsync<T>(
            Stream stream,
            Func<T, (bool IsValid, string? ErrorMessage)> validate,
            CsvConfiguration? config = null,
            ClassMap<T>? classMap = null,
            CancellationToken ct = default)
        {
            var records = new List<T>();
            var errors = new List<(int, string)>();
            var row = 0;

            await foreach (var r in ReadAsync(stream, config, classMap, ct))
            {
                row++;
                var (ok, msg) = validate(r);
                if (ok) records.Add(r);
                else errors.Add((row, msg ?? "Invalid row"));
            }
            return (records, errors);
        }
    }
}
