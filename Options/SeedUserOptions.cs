namespace PhrazorApp.Options
{
    /// <summary>
    /// Identityデフォルトユーザー設定
    /// </summary>
    public class SeedUserOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
    }
}
