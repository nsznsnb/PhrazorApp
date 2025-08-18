using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    /// <summary>
    /// 英語日記 画面モデルのバリデーション
    /// ・Content: 必須 / 最大500
    /// ・Title:   任意 / 最大100
    /// ・Note:    任意 / 最大500
    /// ・TagIds:  重複禁止（任意）
    /// </summary>
    public sealed class EnglishDiaryModelValidator : AbstractValidator<EnglishDiaryModel>
    {
        public EnglishDiaryModelValidator()
        {
            // 日記本文（必須・500文字まで）
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("日記は必須です。")
                .MaximumLength(500).WithMessage("日記は500文字以内で入力してください。");

            // タイトル（任意・100文字まで）
            RuleFor(x => x.Title)
                .MaximumLength(100).WithMessage("タイトルは100文字以内で入力してください。");

            // 補足情報（任意・500文字まで）
            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("補足情報は500文字以内で入力してください。");

            // タグIDの重複禁止（任意）
            RuleFor(x => x.TagIds)
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
                .WithMessage("同じタグが重複しています。");
        }
    }
}
