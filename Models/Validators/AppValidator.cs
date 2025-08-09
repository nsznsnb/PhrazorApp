using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public abstract class AppValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// MudBlazorのフィールド単位バリデーション用デリゲート
        /// "*" または未指定なら全項目検証
        /// </summary>
        public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
            async (model, propertyName) =>
            {
                if (string.IsNullOrWhiteSpace(propertyName) || propertyName == "*")
                {
                    var all = await ValidateAsync((T)model);
                    return all.IsValid ? Array.Empty<string>() : all.Errors.Select(e => e.ErrorMessage);
                }

                var ctx = ValidationContext<T>.CreateWithOptions(
                    (T)model, x => x.IncludeProperties(propertyName));

                var result = await ValidateAsync(ctx);
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            };
    }
}
