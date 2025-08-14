using System.ComponentModel.DataAnnotations;

/// <summary>
/// 確認ダイアログタイプ
/// </summary>
public enum DialogConfirmType
{
    /// <summary>
    /// 削除確認
    /// </summary>
    DeleteConfirm,
    /// <summary>
    /// 登録確認
    /// </summary>
    RegisterConfirm,
}

/// <summary>
/// ダイアログアラートタイプ
/// </summary>
public enum DialogAlertType
{
    Info,
    Warning,
    Error
}

/// <summary>
/// リンク付きメール送信種別
/// </summary>
public enum ConfirmationLinkType
{
    /// <summary>
    /// アカウント本登録
    /// </summary>
    Account,
    /// <summary>
    /// メール変更
    /// </summary>
    Email
}

/// <summary>
/// ドロップゾーン領域種別
/// </summary>
public enum DropItemType
{
    [Display(Name = "未割り当て")]
    UnAssigned,
    [Display(Name = "ターゲット")]
    Target
}

/// <summary>
/// カスタムテーブルページャ表示モード
/// </summary>
public enum TablePagerMode
{
    /// <summary>
    /// 表示
    /// </summary>
    Auto,
    /// <summary>
    /// 非表示
    /// </summary>
    Off
}

/// <summary>
/// オーバーレイの囲う範囲
/// </summary>
public enum OverlayScope
{
    /// <summary>
    /// ボディ部のみ
    /// </summary>
    BodyOnly,
    /// <summary>
    /// 全体
    /// </summary>
    Global
}

public enum PhraseGenreType
{
    /// <summary>
    /// ジャンル
    /// </summary>
    Genre,
    /// <summary>
    /// サブジャンル
    /// </summary>
    SubGenre
}

/// <summary>
/// ジャンル一括設定モード
/// </summary>
public enum BulkGenreMode
{
    /// <summary>
    /// 既存を全消し→選択を追加
    /// </summary>
    ReplaceAll = 0,
    /// <summary>
    /// 既存に無いものだけ追加
    /// </summary>
    AddMerge = 1,
    /// <summary>
    /// 既存を全消し（選択は無視）
    /// </summary>
    ClearAll = 2
}

public enum DateRangePreset { None, Today, Yesterday, Last7Days, Last30Days, Custom }