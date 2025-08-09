using FluentValidation;

namespace PhrazorApp.Models.Validators
{
    public class FileModelValidator : AppValidator<FileModel>
    {
        // 最大10MB
        private const long MaxFileSize = 10 * 1024 * 1024;

        // 許可するContentType
        private static readonly string[] AllowedContentTypes = new[]
        {
            "text/csv",
            "application/vnd.ms-excel",                // Excelがcsvを開く場合
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        public FileModelValidator()
        {
            RuleFor(x => x.File)
                .Cascade(CascadeMode.Stop)
                // ファイル必須（NotNull + Nameチェックまとめ）
                .Must(f => f != null && !string.IsNullOrWhiteSpace(f.Name))
                    .WithMessage(string.Format(AppMessages.MSG_E_CHOICE_DETAIL, "CSVファイル"))

                // サイズチェック
                .Must(f => f == null || f.Size <= MaxFileSize)
                    .WithMessage(string.Format(
                        AppMessages.MSG_E_INVALID_FORMAT,
                        $"ファイルサイズは {MaxFileSize / (1024 * 1024)}MB 以下"))

                // ContentTypeチェック
                .Must(f => f == null || AllowedContentTypes.Contains(f.ContentType))
                    .WithMessage(string.Format(
                        AppMessages.MSG_E_INVALID_FORMAT,
                        "CSV形式"));
        }

    }
}
