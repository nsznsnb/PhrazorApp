using System.Security.Claims;

namespace PhrazorApp.Utils
{
    public static class UserUtil
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void SetHttpContextAccessor(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public static string? GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string? GetUserName()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        }

        public static ClaimsPrincipal? GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User;
        }


    }
}
