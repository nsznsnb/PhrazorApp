using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public sealed class OperationTypeModelValidator : AbstractValidator<PhrazorApp.Models.OperationTypeModel>
    {
        public OperationTypeModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("ID を指定してください。");
            RuleFor(x => x.Name).NotEmpty().WithMessage("操作種別名を入力してください。")
                                .MaximumLength(20).WithMessage("操作種別名は20文字以内で入力してください。");
            RuleFor(x => x.Code).NotEmpty().WithMessage("操作種別コードを入力してください。")
                                .MaximumLength(50).WithMessage("操作種別コードは50文字以内で入力してください。");
            RuleFor(x => x.Limit).GreaterThanOrEqualTo(0)
                                 .WithMessage("操作回数上限は0以上で入力してください。");
        }
    }
}
