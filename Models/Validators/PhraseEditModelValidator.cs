using FluentValidation;
using PhrazorApp.Models;

namespace PhrazorApp.Validators
{
    public sealed class PhraseEditModelValidator : AbstractValidator<PhraseEditModel>
    {
        public PhraseEditModelValidator()
        {
            // 必須 + 最大200
            RuleFor(x => x.Phrase)
                .NotEmpty().WithMessage("フレーズは必須です。")
                .MaximumLength(200).WithMessage("フレーズは200文字以内で入力してください。");

            RuleFor(x => x.Meaning)
                .NotEmpty().WithMessage("意味は必須です。")
                .MaximumLength(200).WithMessage("意味は200文字以内で入力してください。");

            // 任意 + 最大200
            RuleFor(x => x.Note)
                .MaximumLength(200).WithMessage("Note は200文字以内で入力してください。");

            // 念のため：選択数の上限（UI側でも制限）
            RuleFor(x => x.SelectedDropItems)
                .Must(list => list == null || list.Count <= 3)
                .WithMessage("ジャンルの選択は最大3つまでです。");
        }
    }
}
