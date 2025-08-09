using FluentValidation;
using PhrazorApp.Commons;

namespace PhrazorApp.Models.Validators
{
    public class GenreModelValidator : AbstractValidator<GenreModel>
    {
        public GenreModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithName("ジャンルId")
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "ジャンルId"));
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "ジャンル名"));

            RuleFor(x => x.SubGenres)
                .Must(x => x.Count <= 3)
                .WithMessage("サブジャンルの追加は3個までです。");

            RuleForEach(x => x.SubGenres)
                .SetValidator(new SubGenreModelValidator());

        }
        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<GenreModel>.CreateWithOptions((GenreModel)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };

    }
}
