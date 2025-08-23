using System.Globalization;

namespace PhrazorApp.Utils
{
    public static class StringUtil
    {
        /// <summary>
        /// 文字数（書記素数）で安全に切り詰め、超過時は末尾に省略記号を付与します。
        /// </summary>
        public static string Truncate(string? input, int maxChars = 15, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(input) || maxChars <= 0) return string.Empty;

            var e = StringInfo.GetTextElementEnumerator(input);
            int count = 0;
            while (e.MoveNext())
            {
                count++;
                if (count > maxChars)
                {
                    // 現在要素の開始位置が“超過の先頭”。そこまでで切る
                    int cutIndex = e.ElementIndex;
                    return input[..cutIndex] + ellipsis;
                }
            }
            return input; // そもそも短い
        }
    }
}
