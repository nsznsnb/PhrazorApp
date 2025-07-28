using CsvHelper;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace PhrazorApp.Utils
{
    public static class UserUtil
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void SetHttpContextAccessor(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        #region ユーザー情報関連
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
        #endregion

        #region Csv関連
        //public static async Task<List<T>> ReadCsvAsync<T>(Stream stream)
        //{
        //    using var reader = new StreamReader(stream, Encoding.UTF8);
        //    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        //    return await csv.GetRecordsAsync<T>().ToListAsync();
        //}
        #endregion
    }
}
