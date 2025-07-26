using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using PhrazorApp.Common;
using PhrazorApp.Data;
using PhrazorApp.Extensions;
using PhrazorApp.Options;
using Resend;
using System.Net;
using System.Text.RegularExpressions;

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


        /// <summary>
        /// 本登録リンク付きメールを送信します
        /// </summary>
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {

            var type = GetLinkTypeFromUrl(confirmationLink);

            string confirmContent = "アカウント";
            if(type == ConfirmationLinkType.Email.ToString())
            {
                confirmContent = "メールアドレス";
            }


            var message = new EmailMessage();
            message.From = $"{ComDefine.APP_NAME} <onboarding@resend.dev>";
            message.To.Add(email);
            message.Subject = $"【{ComDefine.APP_NAME}】{confirmContent}本登録のご案内";

            var html = $"""
<h2 style="color: #333333;">【{ComDefine.APP_NAME}】{confirmContent}本登録のご案内</h2>

<p>{user.UserName} 様</p>

<p>{ComDefine.APP_NAME}へのご利用ありがとうございます。</p>

<p>現在、{confirmContent}は<strong>仮登録</strong>の状態です。</p>
<p>以下のボタンをクリックして、<strong>本登録</strong>を完了してください。</p>

<p style="margin: 24px 0;">
  <a href="{confirmationLink}" style="display: inline-block; padding: 10px 16px; background-color: #1976d2; color: #ffffff; text-decoration: none; border-radius: 4px;">
    本登録を完了する
  </a>
</p>

<p>※この確認リンクの有効期限は <strong>2時間</strong> です。</p>
<p>※期限が切れた場合は、再度登録手続きを行ってください。</p>
<p>※このメールに心当たりがない場合は、リンクをクリックせずにこのメールを破棄してください。</p>

<p>引き続き、{ComDefine.APP_NAME}をよろしくお願いいたします。</p>
""";

            message.HtmlBody = html;
            message.TextBody = HtmlToPlainText(html);


            try
            {
                await _resend.EmailSendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.SendItem, ex, string.Format(ComMessage.MSG_E_FAILURE_DETAIL2, "メール送信", email));
            }
        }



        /// <summary>
        /// パスワードリセットコード付きメールを送信します
        /// </summary>
        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var message = new EmailMessage();
            message.From = $"{ComDefine.APP_NAME} <onboarding@resend.dev>";
            message.To.Add(email);
            message.Subject = "パスワードリセットコードのご送付";

            var html = $"""
        <h2 style="color: #333333;">【{ComDefine.APP_NAME}】パスワードリセットコードのご送付</h2>
        <p>{user.UserName} 様</p>
        <p>いつも{ComDefine.APP_NAME}をご利用いただき、ありがとうございます。</p>
        <p>パスワードリセットに必要な確認コードは以下の通りです。</p>

        <p style="margin: 24px 0; font-size: 24px; font-weight: bold; letter-spacing: 2px;">{resetCode}</p>

        <p>※この確認リンクの有効期限は <strong>2時間</strong> です。</p>
        <p>※期限が切れた場合は、再度登録手続きを行ってください。</p>
        <p>※このメールに心当たりがない場合は、リンクをクリックせずにこのメールを破棄してください。</p>
        """;

            message.HtmlBody = html;
            message.TextBody = HtmlToPlainText(html);


            try
            {
                await _resend.EmailSendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.SendItem, ex, string.Format(ComMessage.MSG_E_FAILURE_DETAIL2, "メール送信", email));
            }
        }

        /// <summary>
        /// パスワードリセットリンク付きメールを送信します
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
            message.Subject = "パスワードリセットのご案内";

            var html = $"""
      <h2 style="color: #333333;">【{ComDefine.APP_NAME}】パスワードリセットのご案内</h2>
      <p>{user.UserName} 様</p>
      <p>いつも{ComDefine.APP_NAME}をご利用いただき、ありがとうございます。</p>
      <p>パスワードリセットのリクエストを受け付けました。以下のリンクより、新しいパスワードを設定してください。</p>

      <p style="margin: 24px 0;">
        <a href="{resetLink}" style="display: inline-block; padding: 8px 12px; background-color: #1976d2; color: #ffffff; text-decoration: none; border-radius: 4px;">
          パスワードをリセットする
        </a>
      </p>

    <p>※この確認リンクの有効期限は <strong>2時間</strong> です。</p>
    <p>※期限が切れた場合は、再度登録手続きを行ってください。</p>
    <p>※このメールに心当たりがない場合は、リンクをクリックせずにこのメールを破棄してください。</p>
    """;

            message.HtmlBody = html;
            message.TextBody = HtmlToPlainText(html);

            try
            {
                await _resend.EmailSendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.SendItem, ex, string.Format(ComMessage.MSG_E_FAILURE_DETAIL2, "メール送信", email));
            }
        }

        /// <summary>
        /// URLからLink種別を取得する
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        /// <remarks>Link種別:どのタイプの確認メールを送るかを決める</remarks>
        private static string GetLinkTypeFromUrl(string url)
        {
            try
            {
                url = WebUtility.HtmlDecode(url);
                var uri = new Uri(url);
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                return query.TryGetValue("type", out var type) ? type.ToString() : ConfirmationLinkType.Account.ToString();
            }
            catch
            {
                return ConfirmationLinkType.Account.ToString();
            }
        }

        /// <summary>
        /// HTML本文からプレーンテキストを生成します
        /// </summary>
        public string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // 1. <style>, <script> など無視したいタグを除去
            html = Regex.Replace(html, "<(script|style)[^>]*?>.*?</\\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // 2. <br>, <p> などを改行に変換
            html = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</p\s*>", "\n", RegexOptions.IgnoreCase);

            // 3. 残りのタグをすべて除去
            html = Regex.Replace(html, "<.*?>", "");

            // 4. HTMLエンティティのデコード（&nbsp; → 空白など）
            html = WebUtility.HtmlDecode(html);

            // 5. 複数の改行や空白を整理
            html = Regex.Replace(html, @"\n{2,}", "\n\n");
            html = Regex.Replace(html, @"[ \t]{2,}", " ");
            html = html.Trim();

            return html;
        }
    }

}
