namespace PhrazorApp.Common;

/// <summary>
/// ブラウザの sessionStorage / localStorage 用キー
/// </summary>
public static class SessionStorageKey
{
    /// <summary>レビュー（カード）状態：出題カード配列など</summary>
    public const string ReviewStateV1 = "review.state.v1";

    /// <summary>テスト結果（集計＋明細）</summary>
    public const string ReviewResultV1 = "test.result.v1";

    /// <summary>レビュー表示設定：シャッフルON/OFF 等のUIプリファレンス</summary>
    public const string ShufflePrefV1 = "review.pref.shuffle.v1";

    /// <summary>テスト結果の署名（HMAC など）</summary>
    public const string ReviewResultSigV1 = "test.result.sig.v1";

    /// <summary>「間違いのみで再テスト」フラグ</summary>
    public const string ReviewRetestFlagV1 = "review.retest.flag.v1";
}