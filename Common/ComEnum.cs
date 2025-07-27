using System.ComponentModel.DataAnnotations;

public enum CommonDialogPattern
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

public enum PhraseCategoryType
{
    /// <summary>
    /// 大分類
    /// </summary>
    Large,
    /// <summary>
    /// 小分類
    /// </summary>
    Small
}