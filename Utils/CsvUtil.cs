// File: Utils/CsvUtil.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace PhrazorApp.Utils
{
    /// <summary>
    /// CSV の読み込み・フォーマット検証（ヘッダあり／なし両対応）
    /// - 外部パラメータは Schema のみ（トークン/ClassMap/config は内部固定）
    /// - DTO は [Name] もしくは [Index] 属性でマッピング
    /// </summary>
    public static class CsvUtil
    {
        // 内部既定設定（外部公開しない）
        private static CsvConfiguration CreateConfig(bool hasHeader) => new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = hasHeader,
            MissingFieldFound = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        // ---- ヘルパ ----
        public static string BuildHeaderLine(CsvSchema schema)
            => string.Join(",", schema.Columns.Select(c => c.HeaderName));

        public static string BuildHeaderGuide(CsvSchema schema)
            => schema.HasHeaderRecord
                ? BuildHeaderLine(schema)
                : "ヘッダなし, 列順：" + string.Join(", ", schema.Columns.Select(c => c.HeaderName));

        public static IReadOnlyList<string> GetExpectedHeaders(CsvSchema schema)
            => schema.Columns.Select(c => c.HeaderName).ToArray();

        // ---- ヘッダ検証（固定メッセージのみ）----
        /// <summary>
        /// ヘッダを簡易検証し、NG の場合は固定メッセージで返す。
        /// - HasHeaderRecord=false → 検証スキップ（OK）
        /// - AllowExtraColumns=false → 期待ヘッダと完全一致（順序含む・大文字小文字無視）
        /// - AllowExtraColumns=true  → 期待ヘッダが含まれていればOK（順序不問）
        /// </summary>
        public static List<string> ValidateHeader(Stream stream, CsvSchema schema)
        {
            if (!schema.HasHeaderRecord)
                return new List<string>();

            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, CreateConfig(hasHeader: true));

            if (!csv.Read() || !csv.ReadHeader())
                return new List<string> { "CSVヘッダが不正です。", $"正しい見本：{BuildHeaderLine(schema)}" };

            var provided = (csv.HeaderRecord ?? Array.Empty<string>())
                .Select(h => h?.Trim() ?? string.Empty)
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList();

            var expected = GetExpectedHeaders(schema);

            bool ok;
            if (schema.AllowExtraColumns)
            {
                var providedSet = new HashSet<string>(provided, StringComparer.OrdinalIgnoreCase);
                ok = expected.All(h => providedSet.Contains(h));
            }
            else
            {
                ok = provided.Count == expected.Count
                     && provided.Zip(expected, (p, e) => string.Equals(p, e, StringComparison.OrdinalIgnoreCase)).All(b => b);
            }

            return ok
                ? new List<string>()
                : new List<string> { "CSVヘッダが不正です。", $"正しい見本：{BuildHeaderLine(schema)}" };
        }

        // ---- 行検証（必須/最大長）----
        public static (bool Ok, string? Message) ValidateRowBySchema<T>(T item, CsvSchema schema)
        {
            if (item is null) return (false, "行データが空です。");

            var type = typeof(T);
            foreach (var col in schema.Columns)
            {
                var prop = type.GetProperty(col.PropertyName);
                if (prop is null) continue; // DTOに無い列はスキップ（必要なら厳格化）

                var value = prop.GetValue(item);
                var s = value as string ?? string.Empty;

                if (col.Required && string.IsNullOrWhiteSpace(s))
                    return (false, $"'{col.HeaderName}' は必須です。");

                if (col.MaxLength.HasValue && s.Length > 0 && s.Length > col.MaxLength.Value)
                    return (false, $"'{col.HeaderName}' は{col.MaxLength.Value}文字以内で入力してください。");
            }
            return (true, null);
        }

        // ---- ヘッダ＋行の一体検証＆読込（公開API・最小）----
        public static async Task<(List<T> Records,
                                  List<(int RowNumber, string Message)> RowErrors,
                                  List<string> HeaderErrors)>
            ReadWithSchemaAndValidateAsync<T>(Stream stream, CsvSchema schema)
        {
            // stream をコピー（ヘッダ検証と読込で共有）
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            var headerErrors = ValidateHeader(ms, schema);
            if (headerErrors.Count > 0)
                return (new List<T>(), new List<(int, string)>(), headerErrors);

            ms.Position = 0;

            var cfg = CreateConfig(schema.HasHeaderRecord);
            var records = new List<T>();
            var errors = new List<(int, string)>();
            var row = 0;

            using var reader = new StreamReader(ms, leaveOpen: true);
            using var csv = new CsvReader(reader, cfg);

            if (cfg.HasHeaderRecord)
            {
                if (!await csv.ReadAsync() || !csv.ReadHeader())
                    return (records, errors, new List<string>()); // ここに来るのは稀
            }

            while (await csv.ReadAsync())
            {
                row++;
                T? item;
                try { item = csv.GetRecord<T>(); }
                catch { errors.Add((row, "行の解析に失敗しました。")); continue; }

                var (ok, msg) = ValidateRowBySchema(item, schema);
                if (ok) records.Add(item);
                else errors.Add((row, msg ?? "Invalid row"));
            }

            return (records, errors, new List<string>());
        }

        // ---- 互換ヘルパ：単純読み込み（必要な場合のみ使用）----
        /// <summary>
        /// 単純な読み込み（検証やスキーマ無しで、ヘッダ有/無だけ指定）
        /// </summary>
        public static async Task<List<T>> ReadToListAsync<T>(Stream stream, bool hasHeader = true)
        {
            var cfg = CreateConfig(hasHeader);
            var list = new List<T>();

            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, cfg);

            if (cfg.HasHeaderRecord)
            {
                if (!await csv.ReadAsync() || !csv.ReadHeader())
                    return list;
            }

            while (await csv.ReadAsync())
            {
                try
                {
                    var item = csv.GetRecord<T>();
                    if (item is not null) list.Add(item);
                }
                catch { /* 解析失敗はスキップ */ }
            }
            return list;
        }


    }
}
