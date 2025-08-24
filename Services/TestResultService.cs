using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models.Dtos;
using PhrazorApp.Models.Dtos.Maps;
using PhrazorApp.UI.State;
using System.Linq;

namespace PhrazorApp.Services
{
    /// <summary>テスト結果の保存（ヘッダ＋明細）</summary>
    public sealed class TestResultService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;
        private readonly GradeService _grade;
        private readonly ILogger<TestResultService> _logger;
        private const string MSG_PREFIX = "テスト結果";

        public TestResultService(UnitOfWork uow, UserService user, GradeService grade, ILogger<TestResultService> logger)
        {
            _uow = uow;
            _user = user;
            _grade = grade;
            _logger = logger;
        }

        /// <summary>
        /// PSS 由来の結果 <see cref="TestResultState"/> と、
        /// 出題カード列（<see cref="ReviewCardDto"/> = PhraseId/Front/Back）を突き合わせて保存します。
        /// ※ PhraseId が Guid.Empty の行はスキップ（警告ログのみ）。
        /// ※ 結果件数とカード件数が異なる場合、短い方に合わせます（先頭から順次）。
        /// </summary>
        public async Task<ServiceResult<Guid>> SaveAsync(
            TestResultState result,
            IReadOnlyList<ReviewCardDto> cards)
        {
            if (result is null) return ServiceResult.Error<Guid>($"{MSG_PREFIX}：結果が null です。");

            var total = result.Rows?.Count ?? 0;
            if (total <= 0) return ServiceResult.Error<Guid>($"{MSG_PREFIX}：明細がありません。");

            try
            {
                var userId = _user.GetUserId();

                var correct = result.Rows!.Count(r => r.Correct);
                var rate = total == 0 ? 0d : (double)correct / total;

                var grade = await _grade.ResolveByRateEnsureAsync(rate);
                if (grade is null) return ServiceResult.Error<Guid>($"{MSG_PREFIX}：成績が特定できません。");

                var testId = Guid.NewGuid();
                var now = DateTime.UtcNow;

                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    // ヘッダ
                    var head = new DTestResult
                    {
                        TestId = testId,
                        TestDatetime = now,
                        GradeId = grade.GradeId,
                        CompleteFlg = true,
                        UserId = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    await repos.TestResults.AddAsync(head);

                    // 明細
                    var count = Math.Min(total, cards.Count);
                    if (total != cards.Count)
                    {
                        _logger.LogInformation(
                            "TestResult: rows={Rows}, cards={Cards} → {Count} 件で保存",
                            total, cards.Count, count);
                    }

                    var detailNo = 1;
                    for (int i = 0; i < count; i++)
                    {
                        var phraseId = cards[i].PhraseId;
                        if (phraseId == Guid.Empty)
                        {
                            _logger.LogWarning(
                                "TestResultDetail: PhraseId が空のためスキップ (index={Index}, front={Front})",
                                i, cards[i].Front);
                            continue;
                        }

                        var row = result.Rows![i]; // TestResultRowDto
                        var d = new DTestResultDetail
                        {
                            TestId = testId,
                            TestResultDetailNo = detailNo++,
                            PhraseId = phraseId,
                            IsCorrect = row.Correct,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        await repos.TestResultDetails.AddAsync(d);
                    }

                    if (detailNo == 1)
                        throw new InvalidOperationException("保存可能な明細がありません (全行 PhraseId=Empty)。");
                });

                return ServiceResult.Success(testId, $"{MSG_PREFIX}を保存しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Msg} 保存失敗", MSG_PREFIX);
                return ServiceResult.Error<Guid>($"{MSG_PREFIX}の保存に失敗しました。");
            }
        }
    }
}
