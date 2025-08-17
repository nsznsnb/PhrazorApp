using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;

namespace PhrazorApp.Services
{
    /// <summary>
    /// 操作種別マスタ(MOperationType) と 操作履歴(DDailyUsage) を用いた日次上限チェック＆記録。
    /// ・CheckAsync: 実行前に本日の残回数を判定
    /// ・RecordAsync: 成功時に当日分の OperationCount を加算（Upsert）
    /// 基準日は Asia/Tokyo（DBは DateOnly で日単位）
    /// </summary>
    public sealed class OperationLimitService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;
        private readonly ILogger<OperationLimitService> _log;
        private const string MSG_PREFIX = "利用制限";

        public OperationLimitService(UnitOfWork uow, UserService user, ILogger<OperationLimitService> log)
        {
            _uow = uow;
            _user = user;
            _log = log;
        }

        public sealed record CheckResult(
            bool Allowed,
            string? Message,
            int? RemainingToday
        );

        // --- 使いやすさのためのオーバーロード（enum → string） ---
        public Task<ServiceResult<CheckResult>> CheckAsync(OperationTypeCode opCode, int units = 1)
            => CheckAsync(opCode.ToString(), units);

        public Task<ServiceResult<Unit>> RecordAsync(OperationTypeCode opCode, int units = 1)
            => RecordAsync(opCode.ToString(), units);

        /// <summary>実行前チェック（Success かつ Allowed=true で利用可）</summary>
        public async Task<ServiceResult<CheckResult>> CheckAsync(string operationTypeCode, int units = 1)
        {
            try
            {
                var uid = _user.GetUserId();
                var todayLocal = ToTokyoDateOnly(DateTime.UtcNow);

                return await _uow.ReadAsync(async repos =>
                {
                    // --- マスタ取得（OperationTypeCode で一致） ---
                    var op = await repos.OperationTypes
                        .Queryable(true)
                        .FirstOrDefaultAsync(x => x.OperationTypeCode == operationTypeCode);

                    if (op is null)
                        return ServiceResult.Success(new CheckResult(false, $"操作種別({operationTypeCode})が見つかりません。", null));

                    // 上限（0 以下は無制限として扱う）
                    var dailyLimit = op.OperationTypeLimit;

                    if (dailyLimit > 0)
                    {
                        // 今日の使用回数
                        var today = await repos.DailyUsages
                            .Queryable(true)
                            .FirstOrDefaultAsync(x =>
                                x.UserId == uid &&
                                x.OperationTypeId == op.OperationTypeId &&
                                x.OperationDate == todayLocal
                            );

                        var used = today?.OperationCount ?? 0;
                        var remaining = dailyLimit - used;

                        if (remaining < units)
                        {
                            return ServiceResult.Success(
                                new CheckResult(false, $"本日の上限({dailyLimit})に達しています。", 0)
                            );
                        }

                        return ServiceResult.Success(new CheckResult(true, null, remaining - units));
                    }

                    // 無制限
                    return ServiceResult.Success(new CheckResult(true, null, null));
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] Check 失敗 (code={Code})", MSG_PREFIX, operationTypeCode);
                return ServiceResult.Error<CheckResult>("利用可否チェックに失敗しました。");
            }
        }

        /// <summary>成功時に当日分の使用量を加算（戻り値なし→ServiceResult&lt;Unit&gt;）</summary>
        public async Task<ServiceResult<Unit>> RecordAsync(string operationTypeCode, int units = 1)
        {
            try
            {
                var uid = _user.GetUserId();
                var todayLocal = ToTokyoDateOnly(DateTime.UtcNow);

                return await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var op = await repos.OperationTypes
                        .Queryable(false)
                        .FirstOrDefaultAsync(x => x.OperationTypeCode == operationTypeCode);

                    if (op is null)
                        return ServiceResult.None.Error($"操作種別({operationTypeCode})が見つかりません。");

                    var row = await repos.DailyUsages
                        .Queryable(false)
                        .FirstOrDefaultAsync(x =>
                            x.UserId == uid &&
                            x.OperationTypeId == op.OperationTypeId &&
                            x.OperationDate == todayLocal
                        );

                    if (row is null)
                    {
                        row = new Data.Entities.DDailyUsage
                        {
                            UserId = uid,
                            OperationDate = todayLocal,
                            OperationTypeId = op.OperationTypeId,
                            OperationCount = units
                        };
                        await repos.DailyUsages.AddAsync(row);
                    }
                    else
                    {
                        row.OperationCount += units;
                        await repos.DailyUsages.UpdateAsync(row);
                    }

                    return ServiceResult.None.Success();
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Prefix}] Record 失敗 (code={Code})", MSG_PREFIX, operationTypeCode);
                return ServiceResult.None.Error("使用量の記録に失敗しました。");
            }
        }

        // ---- helpers ----
        private static DateOnly ToTokyoDateOnly(DateTime utcNow)
        {
            var tz = GetTokyoTz();
            var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            return DateOnly.FromDateTime(local);
        }
        private static TimeZoneInfo GetTokyoTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"); }
        }
    }
}
