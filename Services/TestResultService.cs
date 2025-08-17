using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.UI.State;

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
        /// ReviewSession のカード順（PhraseId）と TestResultSession の結果を突き合わせて保存。
        /// ※ PhraseId が Guid.Empty の行はスキップ（ログのみ）。
        /// </summary>
        public async Task<ServiceResult<Guid>> SaveAsync(TestResultSession result, IReadOnlyList<ReviewSession.Card> cards)
        {
            if (result.Total <= 0) return ServiceResult.Error<Guid>($"{MSG_PREFIX}：明細がありません。");

            try
            {
                var userId = _user.GetUserId();
                var rate = result.Rate();
                var grade = await _grade.ResolveByRateAsync(rate);
                if (grade is null) return ServiceResult.Error<Guid>($"{MSG_PREFIX}：成績が特定できません。");

                var testId = Guid.NewGuid();
                var now = DateTime.UtcNow;

                await _uow.ExecuteInTransactionAsync(async repos =>
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

                    // 明細（Indexを連番に）
                    var count = Math.Min(result.Items.Count, cards.Count);
                    var details = new List<DTestResultDetail>(count);

                    for (int i = 0; i < count; i++)
                    {
                        var phraseId = cards[i].PhraseId;
                        if (phraseId == Guid.Empty)
                        {
                            _logger.LogWarning("TestResultDetail: PhraseId が空のためスキップ (index={Index})", i);
                            continue;
                        }

                        var row = result.Items[i];
                        details.Add(new DTestResultDetail
                        {
                            TestId = testId,
                            TestResultDetailNo = i + 1,
                            PhraseId = phraseId,
                            IsCorrect = row.IsCorrect,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }

                    if (details.Count == 0)
                        throw new InvalidOperationException("保存可能な明細がありません (全行 PhraseId=Empty)。");

                    // まとめて登録
                    foreach (var d in details)
                        await repos.TestResultDetails.AddAsync(d);
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
