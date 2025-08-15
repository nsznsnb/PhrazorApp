using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public sealed class ProverbModelValidator : AbstractValidator<PhrazorApp.Models.ProverbModel>
    {
        public ProverbModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("ID を指定してください。");
            RuleFor(x => x.Text).NotEmpty().WithMessage("格言を入力してください。")
                                .MaximumLength(200).WithMessage("格言は200文字以内で入力してください。");
            RuleFor(x => x.Author).MaximumLength(100).When(x => x.Author != null)
                                  .WithMessage("著者は100文字以内で入力してください。");
            RuleFor(x => x.Meaning).MaximumLength(200).When(x => x.Meaning != null)
                                   .WithMessage("意味は200文字以内で入力してください。");
        }
    }
}
