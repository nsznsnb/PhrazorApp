namespace PhrazorApp.Utils
{
    public static class DateUtil
    {
        public static string Format(DateTime? dt)
        {
            if (dt is null) return "—";
            var v = dt.Value;
            // Unspecified はそのまま、UTC/Local はローカル表示
            var local = v.Kind switch
            {
                DateTimeKind.Utc => v.ToLocalTime(),
                _ => v
            };
            return local.ToString("yyyy/MM/dd HH:mm");
        }
    }
}
