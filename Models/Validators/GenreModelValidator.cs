using FluentValidation;

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

            // ★ nullでもOK、指定時は最大3件
            RuleFor(x => x.SubGenres)
                .Must(list => list == null || list.Count <= 3)
                .WithMessage("サブジャンルの追加は3個までです。");

            // ★ 未指定(0件)はOK。指定した場合は IsDefault をちょうど1件
            RuleFor(x => x.SubGenres)
                .Must(list => list == null || list.Count == 0 || list.Count(s => s.IsDefault) == 1)
                .WithMessage("サブジャンルを指定する場合、既定は1件だけにしてください。");

            RuleForEach(x => x.SubGenres)
                .SetValidator(new SubGenreModelValidator());
        }
    }
}
