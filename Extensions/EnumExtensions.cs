using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PhrazorApp.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 表示名を取得する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum value)
        {
            var memberInfo = value.GetType().GetMember(value.ToString());
            var attribute = memberInfo.FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();

            return attribute?.Name ?? value.ToString();
        }
    }
}
