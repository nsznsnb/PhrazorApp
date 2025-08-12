using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public class SubGenreModelValidator : AppValidator<SubGenreModel>
    {
        public SubGenreModelValidator()
        {

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            // （必要なら）OrderNoの下限チェックを入れる場合はコメントアウト解除
            // RuleFor(x => x.OrderNo)
            //     .GreaterThanOrEqualTo(0)
            //     .WithMessage("並び順は0以上にしてください。");
        }
    }
}
