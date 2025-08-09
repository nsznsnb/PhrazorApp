namespace PhrazorApp.Commons.Logging
{
    /// <summary>
    /// イベントID管理クラス
    /// </summary>
    public class LogEvents
    {
        /// <summary>
        /// アイテム取得
        /// </summary>
        public static readonly EventId GetItem = new(1000, nameof(GetItem));
        /// <summary>
        /// アイテム一覧取得
        /// </summary>
        public static readonly EventId ListItems = new(1100, nameof(ListItems));
        /// <summary>
        /// アイテム作成
        /// </summary>
        public static readonly EventId CreateItem = new(1200, nameof(CreateItem));
        /// <summary>
        /// アイテム更新
        /// </summary>
        public static readonly EventId UpdateItem = new(1300, nameof(UpdateItem));
        /// <summary>
        /// アイテム削除
        /// </summary>
        public static readonly EventId DeleteItem = new(1400, nameof(DeleteItem));
        /// <summary>
        /// アイテムダウンロード
        /// </summary>
        public static readonly EventId DownloadItem = new(1500, nameof(DownloadItem));
        /// <summary>
        /// アイテムアップロード
        /// </summary>
        public static readonly EventId UploadItem = new(1600, nameof(UploadItem));
        /// <summary>
        /// アイテム送信
        /// </summary>
        public static readonly EventId SendItem = new(1700, nameof(SendItem));
    }
}
