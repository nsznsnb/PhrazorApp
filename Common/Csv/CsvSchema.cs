// File: Common/CsvSchema.cs
namespace PhrazorApp.Common;

/// <summary>CSV の列仕様</summary>
public sealed record CsvColumnSpec(
    string HeaderName,
    string PropertyName,
    bool Required = true,
    int? MaxLength = null);

/// <summary>
/// CSV のスキーマ定義
/// - AllowExtraColumns: 期待外列を許可するか
/// - HasHeaderRecord  : ヘッダ行があるか（false の場合は DTO の [Index] でマッピング）
/// </summary>
public sealed record CsvSchema(
    string Title,
    IReadOnlyList<CsvColumnSpec> Columns,
    bool AllowExtraColumns = false,
    bool HasHeaderRecord = true);
