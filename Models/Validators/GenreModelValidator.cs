using System;
using System.Linq;
using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public sealed class GenreModelValidator : AbstractValidator<GenreModel>
    {
        public GenreModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            // 空白のみもNG
            RuleFor(x => x.Name)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            // サブジャンルの“全体”チェックはモデルレベルでまとめて行い、ValidationSummary に出す
            RuleFor(x => x.SubGenres).Custom((list, ctx) =>
            {
                if (list is null || list.Count == 0) return;

                // 最大25件
                if (list.Count > 25)
                    ctx.AddFailure(string.Empty, "サブジャンルの追加は25個までです。");

                // 既定はちょうど1件
                var defCount = list.Count(s => s.IsDefault);
                if (defCount != 1)
                    ctx.AddFailure(string.Empty, "サブジャンルを指定する場合、既定は1件だけにしてください。");

                // 名称の重複（前後空白除去・大文字小文字無視）
                var hasDup = list
                    .Select(s => (s.Name ?? string.Empty).Trim())
                    .Where(n => n.Length > 0)
                    .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .Any(g => g.Count() > 1);
                if (hasDup)
                    ctx.AddFailure(string.Empty, "サブジャンル名が重複しています。");
            });

            // 各行のフィールド単位チェック（こちらは各テキストボックス横に出る）
            RuleForEach(x => x.SubGenres)
                .SetValidator(new SubGenreModelValidator());
        }
    }

    public sealed class SubGenreModelValidator : AbstractValidator<SubGenreModel>
    {
        public SubGenreModelValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));

            // 空白のみもNG
            RuleFor(x => x.Name)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, AppConstants.FLUENT_PROP_TEMPLATE));
        }
    }
}
