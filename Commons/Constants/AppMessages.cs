namespace PhrazorApp.Commons.Constants
{
    /// <summary>
    /// メッセージ定数
    /// </summary>
    public class AppMessages
    {
        public const string MSG_I_PROGRESS_READ = "読込中です...";
        public const string MSG_I_PROGRESS_TAKE_IN = "取込中です...";
        public const string MSG_I_PROGRESS_SAVE = "保存中です...";
        public const string MSG_I_PROGRESS_DELETE = "削除中です...";
        public const string MSG_I_SUCCESS_CREATE_DETAIL = "{0}を作成しました。";
        public const string MSG_I_SUCCESS_UPDATE_DETAIL = "{0}を更新しました。";
        public const string MSG_I_SUCCESS_DELETE_DETAIL = "{0}を削除しました。";
        public const string MSG_I_SUCCESS_CSV_TAKE_IN = "CSV取込が完了しました。";
        public const string MSG_I_SUCCESS_UPLOAD_DETAIL = "{0}をアップロードしました。";
        public const string MSG_I_SUCCESS_DOWNLOAD_DETAIL = "{0}をダウンロードしました。";
        public const string MSG_I_CONFIRM_REGIST = "{0}を登録します。よろしいですか?";
        public const string MSG_I_CONFIRM_DELETE = "{0}を削除します。よろしいですか?";
        public const string MSG_I_HELPER_REQUIRED_DETAIL = "{0}を入力してください。(必須)";
        public const string MSG_I_HELPER_ARBITRAY_DETAIL = "{0}を入力してください。(任意)";
        public const string MSG_I_HELPER_REQUIRED_LESS_THAN = "{0}を{1}字以内で入力してください。(必須)";
        public const string MSG_I_HELPER_ARBITRARY_LESS_THAN = "{0}を{1}字以内で入力してください。(任意)";
        public const string MSG_I_TOOL_TIP_BULK_DELETE = "チェックされた{0}を一括削除します";


        public const string MSG_E_REQUIRED_DETAIL = "{0}を入力してください。";
        public const string MSG_E_CHOICE_DETAIL = "{0}を選択してください。";
        public const string MSG_E_RANGE_LESS_THAN = "{0}は{1}文字以内で入力してください。";
        public const string MSG_E_HANKAKU_LESS_THAN10 = "{0}は半角10文字以内で入力してください。";
        public const string MSG_E_HANKAKU_LESS_THAN20 = "{0}は半角20文字以内で入力してください。";
        public const string MSG_E_HANKAKU_LESS_THAN30 = "{0}は半角30文字以内で入力してください。";
        public const string MSG_E_HANKAKU_LESS_THAN40 = "{0}は半角40文字以内で入力してください。";
        public const string MSG_E_HANKAKU_LESS_THAN50 = "{0}は半角50文字以内で入力してください。";
        public const string MSG_E_RANGE_BETWEEN = "{0}は{2}文字以上{1}文字以下で入力してください。";
        public const string MSG_E_INVALID_FORMAT = "{0}の形式に誤りがあります。";
        public const string MSG_E_MISMATCH_DETAIL = "{0}が一致しません。";
        public const string MSG_E_NOT_FOUND = "{0}が見つかりませんでした。";
        public const string MSG_E_ERROR_DETEIL = "{0}にエラーが発生しました。";
        public const string MSG_E_FAILURE_CREATE_DETAIL = "{0}の作成に失敗しました。";
        public const string MSG_E_FAILURE_UPDATE_DETAIL = "{0}の更新に失敗しました。";
        public const string MSG_E_FAILURE_DELETE_DETAIL = "{0}の削除に失敗しました。";
        public const string MSG_E_FAILURE_DETAIL = "{0}に失敗しました。";
        public const string MSG_E_FAILURE_DETAIL2 = "{0}に失敗しました。({1})";
        public const string MSG_E_MISMATCH_ID_OR_PASSWORD = "ログインIDまたはパスワードが一致しません。";
        public const string MSG_E_MISMATCH_PASSWORD = "パスワードと確認用パスワードが一致しません。";
        public const string MSG_E_FILE_TOO_LARGE = "{0}のサイズは{1}MB以下にしてください。";
        public const string MSG_E_FILE_INVALID_TYPE = "{0}の形式が不正です。許可されている形式: {1}";
    }
}
