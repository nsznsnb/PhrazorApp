using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PhrazorApp.Common;
using PhrazorApp.Data;
using PhrazorApp.Extensions;
using PhrazorApp.Options;
using Resend;

namespace PhrazorApp.Services
{
    public class ResendEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly ILogger<ResendEmailSender> _logger;
        private readonly IResend _resend;

        public ResendEmailSender(IResend resend, ILogger<ResendEmailSender> logger)
        {
            _logger = logger;
            _resend = resend;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            throw new NotImplementedException();


        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// パスワード再設定リンク付きメールを送信します
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <param name="resetLink"></param>
        /// <returns></returns>
        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var message = new EmailMessage();
            message.From = $"{ComDefine.APP_NAME} <onboarding@resend.dev>";
            message.To.Add(email);
            message.Subject = "パスワード再設定のご案内";

            var html = $"""
      <h2 style="color: #333333;">【{ComDefine.APP_NAME}】パスワード再設定のご案内</h2>
      <p>{user.UserName} 様</p>
      <p>いつも{ComDefine.APP_NAME}をご利用いただき、ありがとうございます。</p>
      <p>パスワードの再設定リクエストを受け付けました。以下のリンクより、新しいパスワードを設定してください。</p>

      <p style="margin: 24px 0;">
        <a href="{resetLink}" style="display: inline-block; padding: 8px 12px; background-color: #1976d2; color: #ffffff; text-decoration: none; border-radius: 4px;">
          パスワードをリセットする
        </a>
      </p>

      <p>※このリンクの有効期限は <strong>2時間</strong> です。期限を過ぎた場合は、再度設定手続きを行ってください。</p>
      <p>※このメールに心当たりがない場合は、リンクをクリックせずにこのメールを破棄してください。</p>
    """;

            message.HtmlBody = html;

            try
            {
                await _resend.EmailSendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.SendItem, ex, string.Format(ComMessage.MSG_E_FAILURE_DETAIL2, "メール送信", email));
            }
        }
    }
}
