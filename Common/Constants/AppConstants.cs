using MudBlazor;

namespace PhrazorApp.Common;
public class AppConstants
{
    // アプリケーション名
    public const string APP_NAME = "Phrazor";

    // フォント
    public const string FONT_FAMILY_1 = "BIZ UDPGothic";
    public const string FONT_FAMILY_2 = "sans-serif";
    public const string FONT_FAMILY_3 = "";

    // ボタンテキスト
    public const string LABEL_BUTTON_EDIT = "編集";
    public const string LABEL_BUTTON_CREATE = "新規作成";
    public const string LABEL_BUTTON_REGISTER = "登録";
    public const string LABEL_BUTTON_DELETE = "削除";
    public const string LABEL_BUTTON_CHANGE = "変更";
    public const string LABEL_BUTTON_CLEAR = "クリア";
    public const string LABEL_BUTTON_CANCEL = "キャンセル";
    public const string LABEL_BUTTON_ROW_ADD = "行追加";
    public const string LABEL_BUTTON_ROW_DELETE = "行削除";
    public const string LABEL_BUTTON_BULK_DELETE = "一括削除";
    public const string LABEL_BUTTON_RETURN_INDEX = "一覧に戻る";
    public const string LABEL_BUTTON_CHOICE_FILE = "ファイル選択";

    // ダイアログタイトル
    public const string LABEL_DIALOG_TITLE_REGISTER_CONFIRM = "登録確認";
    public const string LABEL_DIALOG_TITLE_WARNING_CONFIRM = "警告";
    public const string LABEL_DIALOG_TITLE_DELETE_CONFIRM = "削除確認";

    // テーブルタイトル
    public const string LABEL_TABLE_TITLE = "一覧表示";

    // 正規表現
    public const string REGEX_HANKAKU_LESS_THAN10 = @"^[\u0020-\u007e]{1,10}$";
    public const string REGEX_HANKAKU_LESS_THAN20 = @"^[\u0020-\u007e]{1,20}$";
    public const string REGEX_HANKAKU_LESS_THAN30 = @"^[\u0020-\u007e]{1,30}$";
    public const string REGEX_HANKAKU_LESS_THAN40 = @"^[\u0020-\u007e]{1,40}$";
    public const string REGEX_HANKAKU_LESS_THAN50 = @"^[\u0020-\u007e]{1,50}$";

    public const string ID_BUTTON_SUBMIT = "btnSubmit";
    public const string ID_FORM = "myForm";

    // アカウント関連画面：Info情報プレフィックス
    public const string PREFIX_ACCOUNT_INFO_MESSAGE = "info:";

    // ボタンのデフォルトのサイズ値
    public const Size SIZE_BUTTON = Size.Medium;
    // カードのデフォルトのElevation値
    public const int DEFAULT_ELEVATION = 0;

    public const int DEFAULT_ROWS_PER_PAGE = 25;

    // FluentValidationのプロパティテンプレート
    public const string FLUENT_PROP_TEMPLATE = "{PropertyName}";
}

