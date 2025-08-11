using MudBlazor;

namespace PhrazorApp.Extensions
{
    public static class ServiceResultExtensions
    {
        public static Severity ToSeverity(this ServiceResult r) => r.Status switch
        {
            ServiceStatus.Success => Severity.Success,
            ServiceStatus.Warning => Severity.Warning,
            ServiceStatus.Error => Severity.Error,
            _ => Severity.Info
        };
    }
}
