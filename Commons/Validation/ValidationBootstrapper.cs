using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PhrazorApp.Commons.Validation
{
    public static class ValidationBootstrapper
    {
        public static void Configure()
        {
            ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) =>
            {
                if (member != null)
                {
                    var disp = member.GetCustomAttribute<DisplayAttribute>();
                    if (!string.IsNullOrWhiteSpace(disp?.Name))
                        return disp.Name;
                }
                return member?.Name;
            };
        }
    }
}
