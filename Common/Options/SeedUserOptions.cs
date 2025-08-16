namespace PhrazorApp.Common.Options
{
    /// <summary>
    /// Identityデフォルトユーザー設定
    /// </summary>
    public class SeedUserOptions
    {
        /// <summary>
        /// デフォルトユーザー名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// デフォルトメールアドレス
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// デフォルトパスワード
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// デフォルトロール
        /// </summary>
        public string Role { get; set; } = "Admin";
    }
}
