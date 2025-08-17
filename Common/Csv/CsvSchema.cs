// File: Common/CsvSchema.cs
namespace PhrazorApp.Common;

/// <summary>CSVの1列ぶんの仕様（ヘッダ→DTOプロパティ対応＋簡易バリデーション）。</summary>
public sealed record CsvColumnSpec(
    string HeaderName,   // ヘッダ名（HasHeaderRecord=false時は説明用）
    string PropertyName, // DTO側のプロパティ名
    bool Required = true,
    int? MaxLength = null);

/// <summary>CSVスキーマ（列定義＋ルール）。</summary>
/// <remarks>
/// HasHeaderRecord=false: ヘッダ行なし（DTOに[Index]必須）
/// AllowExtraColumns=true: 期待列が揃っていれば余分列OK
/// </remarks>
public sealed record CsvSchema(
    string Title,                          // 表示用ラベル（ログ/画面）
    IReadOnlyList<CsvColumnSpec> Columns,  // 期待列（AllowExtraColumns=false時は順序も一致）
    bool AllowExtraColumns = false,
    bool HasHeaderRecord = true);

/// <summary>アプリ標準のCSVスキーマ集。</summary>
public static class CsvSchemas
{
    /// <summary>英語フレーズ</summary>
    public static readonly CsvSchema PhraseImport = new(
        Title: "PhraseImport",
        Columns: new[]
        {
            new CsvColumnSpec("フレーズ",  "Phrase",  Required: true,  MaxLength: 200),
            new CsvColumnSpec("意味", "Meaning", Required: true,  MaxLength: 200),
        },
        AllowExtraColumns: false,
        HasHeaderRecord: false
    );

    /// <summary>格言</summary>
    public static readonly CsvSchema ProverbImport = new(
        Title: "ProverbImport",
        Columns: new[]
        {
            new CsvColumnSpec("格言", "Text",    Required: true,  MaxLength: 200),
            new CsvColumnSpec("意味",     "Meaning", Required: true, MaxLength: 200),
            new CsvColumnSpec("著者",      "Author",  Required: true, MaxLength: 100),
        },
        AllowExtraColumns: false,
        HasHeaderRecord: false
    );
}