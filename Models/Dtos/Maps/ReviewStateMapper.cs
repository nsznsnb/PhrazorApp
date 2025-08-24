using PhrazorApp.UI.State;

namespace PhrazorApp.Models.Dtos.Maps
{
    /// <summary>
    /// レビュー（出題）状態のDTOマッパー。
    /// </summary>
    public static class ReviewStateMapper
    {
        // 作成ヘルパ
        public static ReviewCardDto Create(Guid id, string front, string back)
            => new() { PhraseId = id, Front = front ?? string.Empty, Back = back ?? string.Empty };

        // 任意型からのDTO化（id/front/back セレクタ指定）
        public static List<ReviewCardDto> ToDtos<T>(
            IEnumerable<T> src,
            Func<T, Guid> id,
            Func<T, string> front,
            Func<T, string> back)
            => (src ?? Enumerable.Empty<T>())
                .Select(x => new ReviewCardDto { PhraseId = id(x), Front = front(x) ?? "", Back = back(x) ?? "" })
                .ToList();

        // Front をキーに突き合わせたいときのユーティリティ
        public static string FrontKey(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();

        public static Dictionary<string, ReviewCardDto> BuildFrontKeyMap(IEnumerable<ReviewCardDto> cards)
            => (cards ?? Enumerable.Empty<ReviewCardDto>())
               .GroupBy(c => FrontKey(c.Front))
               .ToDictionary(g => g.Key, g => g.First());
    }
}
