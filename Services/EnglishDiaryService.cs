using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// 英語日記ユースケース
    /// - GetMonthlyItemsAsync : 月のカレンダー表示用アイテム取得
    /// - GetByDateAsync       : 指定ローカル日（Asia/Tokyo）の日記取得（なければ null）
    /// - UpsertAsync          : 添削なしで保存（新規/更新）
    /// - CorrectAndUpsertAsync: 添削 → 当日分Upsert（制限チェック＋成功後記録）
    /// - DeleteByDateAsync    : 指定日の削除
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

        public async Task<ServiceResult<List<DiaryCalendarItem>>> GetMonthlyItemsAsync(DateOnly month)
        {
            try
            {
                var uid = _user.GetUserId();
                var (fromUtc, toUtc) = GetUtcRangeForLocalMonth(month);
                var tz = GetTokyoTz();

                var models = await _uow.ReadAsync(async (UowRepos repos) =>
                    await repos.EnglishDiaries
                        .Queryable(true)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .SelectModel()
                        .ToListAsync()
                ) ?? new List<EnglishDiaryModel>();

                var items = models
                    .Select(m =>
                    {
                        var local = TimeZoneInfo.ConvertTimeFromUtc(m.CreatedAt, tz);
                        var localDate = DateOnly.FromDateTime(local);
                        var start = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);

                        return new
                        {
                            m.Title,
                            LocalDate = localDate,
                            Start = start,
                            SortKey = m.UpdatedAt == default ? m.CreatedAt : m.UpdatedAt
                        };
                    })
                    .GroupBy(x => x.LocalDate)
                    .Select(g =>
                    {
                        var latest = g.OrderByDescending(x => x.SortKey).First();
                        return new DiaryCalendarItem
                        {
                            Title = string.IsNullOrWhiteSpace(latest.Title) ? "（無題）" : latest.Title,
                            Start = latest.Start
                        };
                    })
                    .OrderBy(x => x.Start)
                    .ToList();

                return ServiceResult.Success(items);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] 月次読込に失敗", MSG_PREFIX);
                return ServiceResult.Error<List<DiaryCalendarItem>>("カレンダーの読込に失敗しました。");
            }
        }

        public async Task<ServiceResult<EnglishDiaryModel?>> GetByDateAsync(DateOnly date)
        {
            try
            {
                var uid = _user.GetUserId();
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(date);

                var model = await _uow.ReadAsync(async (UowRepos repos) =>
                    await repos.EnglishDiaries
                        .Queryable(true)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .SelectModel()
                        .FirstOrDefaultAsync()
                );

                return ServiceResult.Success(model);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] {Date} の取得に失敗", MSG_PREFIX, date);
                return ServiceResult.Error<EnglishDiaryModel?>("日記の取得に失敗しました。");
            }
        }

        /// <summary>
        /// 添削なしで保存（新規/更新）。OpenAI・使用量記録は行いません。
        /// </summary>
        public async Task<ServiceResult<EnglishDiaryModel>> UpsertAsync(EnglishDiaryModel model)
        {
            try
            {
                var uid = _user.GetUserId();

                // 編集中モデルの CreatedAt が属するローカル日で Upsert
                var targetLocal = ToLocalDate(model.CreatedAt);
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(targetLocal);

                var saved = await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
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

                var message = saved.isNew
                    ? string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX)
                    : string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX);

                return ServiceResult.Success(saved.Item2, message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] 保存（添削なし）で例外", MSG_PREFIX);
                return ServiceResult.Error<EnglishDiaryModel>("保存に失敗しました。");
            }
        }

        /// <summary>
        /// OpenAIで添削を行い、当日（ローカル：Asia/Tokyo）分をUpsertする。
        /// </summary>
        public async Task<ServiceResult<EnglishDiaryModel>> CorrectAndUpsertAsync(EnglishDiaryModel model)
        {
            try
            {
                // 実行前チェック（操作種別：DiaryCorrection）
                var chk = await _limit.CheckAsync(OperationTypeCode.DiaryCorrection, units: 1);
                if (!chk.IsSuccess || !(chk.Data?.Allowed ?? false))
                {
                    var msg = chk.Message ?? chk.Data?.Message ?? "利用できません。";
                    return ServiceResult.Error<EnglishDiaryModel>(msg);
                }

                var uid = _user.GetUserId();

                const string FormatGuideEn = """
You edit English diary entries. Output MUST be GitHub Flavored Markdown.
Add no preface/appendix, no code blocks/HTML/images. Keep total length ≤ 500 characters.
Preserve paragraph breaks; keep the author’s intent. Also add a short explanation in Japanese.
""";

                var userPrompt = $"""
Text:
{model.Content}

Notes (optional):
{model.Note}

Edit the text as instructed and return Markdown with the corrected English and a brief Japanese explanation.
""";

                var ai = await _openAi.ChatOnceAsync(
                    system: FormatGuideEn,
                    user: userPrompt,
                    temperature: 0.4f, topP: 0.9f, frequencyPenalty: 0.2f
                );

                if (!ai.IsSuccess || string.IsNullOrWhiteSpace(ai.Data))
                    return ServiceResult.Error<EnglishDiaryModel>(ai.Message ?? "添削に失敗しました。");

                model.Correction = ai.Data.Trim();

                // Upsert（編集中モデルの CreatedAt が属するローカル日で1件）
                var targetLocal = ToLocalDate(model.CreatedAt);
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(targetLocal);

                var saved = await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var entity = await repos.EnglishDiaries
                        .Queryable(false)
                        .Include(x => x.DEnglishDiaryTags)
                        .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                        .FirstOrDefaultAsync();

                    bool isNew = entity is null;

                    if (entity is null)
                    {
                        var eNew = model.ToEntity(uid);
                        await repos.EnglishDiaries.AddAsync(eNew);
                        return (isNew, eNew.ToModel());
                    }
                    else
                    {
                        model.ApplyTo(entity);
                        await repos.EnglishDiaries.UpdateAsync(entity);
                        return (isNew, entity.ToModel());
                    }
                });

                // 成功時に使用量記録
                var rec = await _limit.RecordAsync(OperationTypeCode.DiaryCorrection, units: 1);
                if (!rec.IsSuccess)
                    _log.LogWarning("Operation usage record failed: {Message}", rec.Message);

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

        public async Task<ServiceResult<Unit>> DeleteByDateAsync(DateOnly localDateJst)
        {
            var uid = _user.GetUserId();
            var (fromUtc, toUtc) = GetUtcRangeForLocalDate(localDateJst);

            var result = ServiceResult.None.Success();

            await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
            {
                Guid? diaryId = await repos.EnglishDiaries.Queryable(false)
                    .Where(x => x.UserId == uid && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
                    .Select(x => (Guid?)x.DiaryId)
                    .SingleOrDefaultAsync();

                if (diaryId is null) return;

                await repos.EnglishDiaryTags.Queryable(false)
                    .Where(t => t.DiaryId == diaryId.Value)
                    .ExecuteDeleteAsync();

                await repos.EnglishDiaries.DeleteAsync(new DEnglishDiary
                {
                    DiaryId = diaryId.Value
                });
            });

            return result;
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
