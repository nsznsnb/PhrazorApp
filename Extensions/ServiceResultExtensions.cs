using MudBlazor;

namespace PhrazorApp.Extensions
{
    public static class ServiceResultExtensions
    {
        public static Severity ToSeverity<T>(this ServiceResult<T> r) => r.Status switch
        {
            ServiceStatus.Success => Severity.Success,
            ServiceStatus.Warning => Severity.Warning,
            ServiceStatus.Error => Severity.Error,
            _ => Severity.Info
        };
    }
}
