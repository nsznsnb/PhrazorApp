using Microsoft.AspNetCore.Identity;

namespace PhrazorApp.Data
{
    /// <summary>
    /// Identityバリデーションメッセージ(カスタマイズ)
    /// </summary>
    public class JapaneseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new() { Code = nameof(DefaultError), Description = "不明なエラーが発生しました。" };

        public override IdentityError ConcurrencyFailure() =>
            new() { Code = nameof(ConcurrencyFailure), Description = "同時に変更されたため、保存できませんでした。" };

        public override IdentityError PasswordMismatch() =>
            new() { Code = nameof(PasswordMismatch), Description = "パスワードが正しくありません。" };

        public override IdentityError InvalidToken() =>
            new() { Code = nameof(InvalidToken), Description = "無効なトークンです。" };

        public override IdentityError LoginAlreadyAssociated() =>
            new() { Code = nameof(LoginAlreadyAssociated), Description = "このログインはすでに他のアカウントに関連付けられています。" };

        public override IdentityError InvalidUserName(string userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"ユーザーID '{userName}' は無効です。" };

        public override IdentityError InvalidEmail(string email) =>
            new() { Code = nameof(InvalidEmail), Description = $"メールアドレス '{email}' は無効です。" };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"ユーザーID '{userName}' は既に使用されています。" };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"メールアドレス '{email}' は既に使用されています。" };

        public override IdentityError InvalidRoleName(string role) =>
            new() { Code = nameof(InvalidRoleName), Description = $"ロール名 '{role}' は無効です。" };

        public override IdentityError DuplicateRoleName(string role) =>
            new() { Code = nameof(DuplicateRoleName), Description = $"ロール名 '{role}' は既に使用されています。" };

        public override IdentityError UserAlreadyHasPassword() =>
            new() { Code = nameof(UserAlreadyHasPassword), Description = "このユーザーにはすでにパスワードが設定されています。" };

        public override IdentityError UserLockoutNotEnabled() =>
            new() { Code = nameof(UserLockoutNotEnabled), Description = "このユーザーにはロックアウトが有効になっていません。" };

        public override IdentityError UserAlreadyInRole(string role) =>
            new() { Code = nameof(UserAlreadyInRole), Description = $"ユーザーはすでにロール '{role}' に所属しています。" };

        public override IdentityError UserNotInRole(string role) =>
            new() { Code = nameof(UserNotInRole), Description = $"ユーザーはロール '{role}' に所属していません。" };

        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"パスワードは最低 {length} 文字である必要があります。" };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "パスワードには、少なくとも1つの記号（英数字以外の文字）が必要です。" };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "パスワードには、少なくとも1つの数字（0～9）が必要です。" };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "パスワードには、少なくとも1つの小文字（a～z）が必要です。" };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "パスワードには、少なくとも1つの大文字（A～Z）が必要です。" };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"パスワードには、少なくとも {uniqueChars} 種類の異なる文字が必要です。" };

        public override IdentityError RecoveryCodeRedemptionFailed() =>
            new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "リカバリーコードの使用に失敗しました。" };

    }
}
