using FluentValidation;
using PhrazorApp.Commons;

namespace PhrazorApp.Models.Validators
{
    public class GenreModelValidator : AppValidator<GenreModel>
    {
        public GenreModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            RuleFor(x => x.SubGenres)
                .Must(x => x.Count <= 3)
                .WithMessage("サブジャンルの追加は3個までです。");

            RuleForEach(x => x.SubGenres)
                .SetValidator(new SubGenreModelValidator());
        }

    }
}
