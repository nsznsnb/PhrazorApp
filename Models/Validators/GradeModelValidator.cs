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
        }
    }
}
