using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// 英語日記ユースケース
    /// - GetMonthlyItemsAsync: 月のカレンダー表示用アイテム取得
    /// - GetByDateAsync      : 指定ローカル日（Asia/Tokyo）の日記取得（なければ null）
    /// - CorrectAndUpsertAsync: 添削 → 当日分Upsert（制限チェック＋成功後記録）
    /// </summary>
    public sealed class EnglishDiaryService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;
        private readonly OpenAiClient _openAi;
        private readonly OperationLimitService _limit;
        private readonly ILogger<EnglishDiaryService> _log;

        private const string MSG_PREFIX = "英語日記";

        public EnglishDiaryService(
            UnitOfWork uow,
            UserService user,
            OpenAiClient openAi,
            OperationLimitService limit,
            ILogger<EnglishDiaryService> log)
        {
            _uow = uow;
            _user = user;
            _openAi = openAi;
            _limit = limit;
            _log = log;
        }

        /// <summary>
        /// 対象月（ローカル：Asia/Tokyo）の日記をカレンダー表示用に取得
        /// </summary>
        public async Task<ServiceResult<List<DiaryCalendarItem>>> GetMonthlyItemsAsync(DateOnly month)
        {
            try
            {
                var uid = _user.GetUserId();
                var (fromUtc, toUtc) = GetUtcRangeForLocalMonth(month);

                var models = await _uow.ReadAsync(async repos =>
                {
                    return await repos.EnglishDiaries
                        .Queryable(true)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .SelectModel()               // ← Mapper: Entity→Model 投影
                        .ToListAsync();
                });

                var items = models.AsEnumerable()
                                  .Select(m => m.ToCalendarItem()) // ← CalendarItem 生成はメモリ上で
                                  .ToList();

                return ServiceResult.Success(items);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] 月次読込に失敗", MSG_PREFIX);
                return ServiceResult.Error<List<DiaryCalendarItem>>("カレンダーの読込に失敗しました。");
            }
        }

        /// <summary>
        /// 指定ローカル日（Asia/Tokyo）の日記を1件取得。存在しない場合は Success(null)。
        /// </summary>
        public async Task<ServiceResult<EnglishDiaryModel?>> GetByDateAsync(DateOnly date)
        {
            try
            {
                var uid = _user.GetUserId();
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(date);

                var model = await _uow.ReadAsync(async repos =>
                {
                    return await repos.EnglishDiaries
                        .Queryable(true)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .SelectModel()
                        .FirstOrDefaultAsync();
                });

                return ServiceResult.Success(model);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] {Date} の取得に失敗", MSG_PREFIX, date);
                return ServiceResult.Error<EnglishDiaryModel?>("日記の取得に失敗しました。");
            }
        }

        /// <summary>
        /// OpenAIで添削を行い、当日（ローカル：Asia/Tokyo）分をUpsertする。
        /// </summary>
        public async Task<ServiceResult<EnglishDiaryModel>> CorrectAndUpsertAsync(EnglishDiaryModel model)
        {
            try
            {
                // 1) 実行前チェック（操作種別：DiaryCorrection）
                var chk = await _limit.CheckAsync(OperationTypeCode.DiaryCorrection, units: 1);
                if (!chk.IsSuccess || !(chk.Data?.Allowed ?? false))
                {
                    var msg = chk.Message ?? chk.Data?.Message ?? "利用できません。";
                    return ServiceResult.Error<EnglishDiaryModel>(msg);
                }

                var uid = _user.GetUserId();

                // 2) OpenAI 添削（BuildPromptAsync は ServiceResult<string> を返す想定）
                var prompt =
                    $"You are an English writing tutor. Correct the following diary to natural English. " +
                    $"Keep the meaning. Provide short Japanese explanations of major fixes.\n\n" +
                    $"[Diary]\n{model.Content}\n\n" +
                    $"[Additional context]\n{(string.IsNullOrWhiteSpace(model.Note) ? "(none)" : model.Note)}\n\n" +
                    "Output format:\n" +
                    "### Corrected\n<rewritten diary>\n\n" +
                    "### Explanation (JA)\n- <point1>\n- <point2>\n";

                var ai = await _openAi.BuildPromptAsync(prompt);
                if (!ai.IsSuccess || string.IsNullOrWhiteSpace(ai.Data))
                {
                    return ServiceResult.Error<EnglishDiaryModel>(ai.Message ?? "添削に失敗しました。");
                }

                model.Correction = ai.Data;

                // 3) Upsert（当日ローカルの CreatedAt 範囲で1件に集約）
                var todayLocal = ToLocalDate(DateTime.UtcNow); // Asia/Tokyo
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(todayLocal);

                var saved = await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    // 既存取得（タグを更新するので Include）
                    var entity = await repos.EnglishDiaries
                        .Queryable(false)
                        .Include(x => x.DEnglishDiaryTags)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .FirstOrDefaultAsync();

                    bool isNew = entity is null;

                    if (entity is null)
                    {
                        var eNew = model.ToEntity(uid); // タグも反映
                        await repos.EnglishDiaries.AddAsync(eNew);
                        return (isNew, eNew.ToModel());
                    }
                    else
                    {
                        model.ApplyTo(entity); // タグ全置換
                        await repos.EnglishDiaries.UpdateAsync(entity);
                        return (isNew, entity.ToModel());
                    }
                });

                // 4) 成功時に使用量記録（戻り値なし → ServiceResult.None）
                var rec = await _limit.RecordAsync(OperationTypeCode.DiaryCorrection, units: 1);
                if (!rec.IsSuccess)
                {
                    _log.LogWarning("Operation usage record failed: {Message}", rec.Message);
                }

                var message = saved.isNew
                    ? string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX)
                    : string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX);

                return ServiceResult.Success(saved.Item2, message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] 添削/登録で例外", MSG_PREFIX);
                return ServiceResult.Error<EnglishDiaryModel>("添削または登録に失敗しました。");
            }
        }

        // ===== 時刻変換ヘルパ（Asia/Tokyo 基準） =====

        private static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForLocalMonth(DateOnly month)
        {
            var firstLocal = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var nextLocal = firstLocal.AddMonths(1);
            return (ToUtc(firstLocal), ToUtc(nextLocal));
        }

        private static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForLocalDate(DateOnly date)
        {
            var fromLocal = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var toLocal = fromLocal.AddDays(1);
            return (ToUtc(fromLocal), ToUtc(toLocal));
        }

        private static DateOnly ToLocalDate(DateTime utc)
        {
            var tz = GetTokyoTz();
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            return DateOnly.FromDateTime(local);
        }

        private static DateTime ToUtc(DateTime localUnspecified)
            => TimeZoneInfo.ConvertTimeToUtc(localUnspecified, GetTokyoTz());

        private static TimeZoneInfo GetTokyoTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"); }
        }
    }
}
