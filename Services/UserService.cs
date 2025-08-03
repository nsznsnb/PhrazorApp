using System.Security.Claims;

namespace PhrazorApp.Services
{
    public class UserService
    {
        private IHttpContextAccessor? _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public string? GetUserName()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
        }

        public ClaimsPrincipal? GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User;
        }

    }
}
