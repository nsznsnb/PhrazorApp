using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public sealed class GradeModelValidator : AbstractValidator<PhrazorApp.Models.GradeModel>
    {
        public GradeModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("ID を指定してください。");
            RuleFor(x => x.Name).NotEmpty().WithMessage("成績名を入力してください。")
                                .MaximumLength(20).WithMessage("成績名は20文字以内で入力してください。");

            // 追加：並び順は 0 以上
            RuleFor(x => x.OrderNo).GreaterThanOrEqualTo(0)
                                   .WithMessage("並び順は0以上で指定してください。");
        }
    }
}
