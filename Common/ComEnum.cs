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