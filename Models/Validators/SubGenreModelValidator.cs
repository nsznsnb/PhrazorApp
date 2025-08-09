using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public class SubGenreModelValidator : AppValidator<SubGenreModel>
    {
        public SubGenreModelValidator()
        {
            RuleFor(x => x.Id)
                        .NotEmpty()
                        .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL,AppConstants.FLUENT_PROP_TEMPLATE));
            RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));
        }


    }
}
