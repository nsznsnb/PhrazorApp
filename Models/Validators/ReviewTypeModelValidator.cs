using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public sealed class ReviewTypeModelValidator : AbstractValidator<PhrazorApp.Models.ReviewTypeModel>
    {
        public ReviewTypeModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("ID を指定してください。");
            RuleFor(x => x.Name).NotEmpty().WithMessage("復習種別名を入力してください。")
                                .MaximumLength(20).WithMessage("復習種別名は20文字以内で入力してください。");
        }
    }
}
