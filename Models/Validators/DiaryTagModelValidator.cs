using FluentValidation;
using PhrazorApp.Models;
// ← AppValidator<T> の名前空間。環境に合わせて必要なら変更してください

namespace PhrazorApp.Models.Validators
{
    /// <summary>日記タグの入力検証</summary>
    public sealed class DiaryTagModelValidator : AbstractValidator<DiaryTagModel>
    {
        public DiaryTagModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            // 空白のみも NG ＋ 50文字まで
            RuleFor(x => x.Name)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE))
                .MaximumLength(50)
                .WithMessage("50文字以内で入力してください。");
        }
    }
}
