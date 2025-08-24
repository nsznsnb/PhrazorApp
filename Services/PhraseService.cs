using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// フレーズのユースケース（CancellationToken不使用版）
    /// </summary>
    public sealed class PhraseService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<PhraseService> _logger;
        private const string MSG_PREFIX = "フレーズ";

        public PhraseService(UnitOfWork uow, UserService userService, ILogger<PhraseService> logger)
        {
            _uow = uow;
            _userService = userService;
            _logger = logger;
        }

        // 一覧（DTOに投影済みをRepoで取得）
        public Task<ServiceResult<List<PhraseListItemModel>>> GetPhraseListAsync()
        {
            return _uow.ReadAsync(async (UowRepos repos) =>
            {
                var uid = _userService.GetUserId();
                var list = await repos.Phrases.GetListProjectedAsync(uid);

                // 並び順は安定化
                list = list.OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue).ToList();

                // ▼ 一覧に含まれるフレーズIDだけで、所属フレーズ帳名を一括取得して埋める
                var phraseIds = list.Select(x => x.Id).Distinct().ToArray();
                if (phraseIds.Length > 0)
                {
                    var pairs = await (
                        from bi in repos.PhraseBookItems.Queryable(asNoTracking: true)
                        join b in repos.PhraseBooks.Queryable(asNoTracking: true)
                            on bi.PhraseBookId equals b.PhraseBookId
                        where phraseIds.Contains(bi.PhraseId)
                        select new { bi.PhraseId, b.PhraseBookName }
                    ).ToListAsync();

                    var phraseIdToBookNames = pairs
                        .GroupBy(x => x.PhraseId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.PhraseBookName)
                                  .Where(n => !string.IsNullOrWhiteSpace(n))
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .OrderBy(n => n)
                                  .ToList()
                        );

                    foreach (var row in list)
                        if (phraseIdToBookNames.TryGetValue(row.Id, out var names))
                            row.PhraseBookNames = names;
                }
                // ▲ 追記ここまで

                return ServiceResult.Success(list, message: "");
            });
        }

        // 編集ロード（Aggregate取得 → EditModel）
        public Task<ServiceResult<PhraseEditModel>> GetPhraseEditAsync(Guid id)
        {
            return _uow.ReadAsync(async (UowRepos repos) =>
            {
                // ※ Repo: GetPhraseByIdAsync は DPhraseImage / MPhraseGenres(+MSubGenre) を含む想定
                var e = await repos.Phrases.GetPhraseByIdAsync(id);
                var model = e is null ? new PhraseEditModel { Id = id } : e.ToModel();
                return ServiceResult.Success(model, message: "");
            });
        }

        /// <summary>
        /// テスト候補の抽出（画面のプレビュー／開始で使用）
        /// - 絞り込みは Repository レイヤーに委譲（DbContext のユーザーフィルタもそちらで適用）
        /// - 並び替えは Shuffle 指定時のみメモリでランダム
        /// - Limit は 1〜500 に丸め
        /// </summary>
        public Task<ServiceResult<List<PhraseListItemModel>>> BuildCandidatesAsync(TestFilterModel filter)
        {
            return _uow.ReadAsync(async (UowRepos repos) =>
            {
                var uid = _userService.GetUserId();

                // ★ 重要：Repository 側で SubGenreIds / PhraseBookIds / 期間 / UntestedOnly を解釈する
                //   （UI 変更に合わせ「GenreIds 依存」は不要。SubGenreIds 優先の実装にしておく）
                var list = await repos.Phrases.GetListByFilterProjectedAsync(uid, filter);

                // シャッフル（SQL依存を避けメモリ側で）
                if (filter.Shuffle && list.Count > 1)
                    list = list.OrderBy(_ => Guid.NewGuid()).ToList();

                // 上限丸め
                var take = Math.Clamp(filter.Limit <= 0 ? 20 : filter.Limit, 1, 500);
                if (list.Count > take)
                    list = list.Take(take).ToList();

                return ServiceResult.Success(list, "");
            });
        }

        public async Task<ServiceResult<HashSet<Guid>>> GetAvailableSubGenreIdsAsync(TestFilterModel f)
        {
            return await _uow.ReadAsync(async repos =>
            {
                // フレーズ帳が指定されている場合：帳→フレーズ→フレーズ×サブジャンル
                if (f.PhraseBookIds is { Count: > 0 })
                {
                    var ids =
                        await (from i in repos.PhraseBookItems.Queryable()
                               where f.PhraseBookIds.Contains(i.PhraseBookId)
                               join pg in repos.PhraseGenres.Queryable() on i.PhraseId equals pg.PhraseId
                               select pg.SubGenreId)
                              .Distinct()
                              .ToListAsync();

                    return ServiceResult.Success(ids.ToHashSet());
                }

                // 帳指定なし：全体でフレーズに紐づくサブジャンル
                {
                    var ids = await repos.PhraseGenres.Queryable()
                                    .Select(pg => pg.SubGenreId)
                                    .Distinct()
                                    .ToListAsync();
                    return ServiceResult.Success(ids.ToHashSet());
                }
            });
        }

        /// <summary>作成</summary>
        public async Task<ServiceResult<Unit>> CreatePhraseAsync(PhraseEditModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var userId = _userService.GetUserId();

                    await repos.Phrases.AddAsync(model.ToEntity(userId));

                    if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                        await repos.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));

                    if (model.SelectedDropItems?.Count > 0)
                        await repos.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ登録で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>一括作成</summary>
        public async Task<ServiceResult<Unit>> CreatePhrasesAsync(IEnumerable<PhraseEditModel> models)
        {
            if (models == null) return ServiceResult.None.Error("入力が null です。");

            var list = models.Where(m => m != null).ToList();
            if (list.Count == 0) return ServiceResult.None.Error("登録対象がありません。");

            var userId = _userService.GetUserId();

            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var nowUtc = DateTime.UtcNow;

                    var phraseEntities = new List<DPhrase>(list.Count);
                    var imageEntities = new List<DPhraseImage>();
                    var phraseGenreEntities = new List<MPhraseGenre>();

                    foreach (var model in list)
                    {
                        // ここで必ず新規発番（画面の Id は無視）
                        model.Id = Guid.NewGuid();

                        var phrase = model.ToEntity(userId);
                        phraseEntities.Add(phrase);

                        if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                            imageEntities.Add(model.ToImageEntity(nowUtc));

                        if (model.SelectedDropItems?.Count > 0)
                            phraseGenreEntities.AddRange(model.ToPhraseGenreEntities());
                    }

                    await repos.Phrases.AddRangeAsync(phraseEntities);
                    if (imageEntities.Count > 0) await repos.PhraseImages.AddRangeAsync(imageEntities);
                    if (phraseGenreEntities.Count > 0) await repos.PhraseGenres.AddRangeAsync(phraseGenreEntities);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ一括登録で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> SetGenresBulkAsync(IEnumerable<Guid> phraseIds,
                                                    List<DropItemModel> selected,
                                                    BulkGenreMode mode)
        {
            if (phraseIds is null || !phraseIds.Any())
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "対象"));

            selected ??= new();

            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var idSet = phraseIds.Distinct().ToArray();
                    const int chunkSize = 500;
                    const int MaxPerPhrase = 3;

                    // UI選択（順序維持・重複除去・Key1/Key2 必須）
                    var wantOrdered = selected
                        .Where(s => s.Key2.HasValue)
                        .Select(s => (GenreId: s.Key1, SubGenreId: s.Key2!.Value))
                        .DistinctBy(x => x)            // 先に現れた順を残す
                        .ToList();

                    var toDelete = new List<MPhraseGenre>();
                    var toAdd = new List<MPhraseGenre>();

                    foreach (var chunk in idSet.Chunk(chunkSize))
                    {
                        var phrases = await repos.Phrases.GetByPhrasesIdsAsync(chunk); // MPhraseGenres 含む

                        foreach (var p in phrases)
                        {
                            var current = p.MPhraseGenres ?? new List<MPhraseGenre>();

                            if (mode is BulkGenreMode.ReplaceAll or BulkGenreMode.ClearAll)
                            {
                                if (current.Count > 0) toDelete.AddRange(current);
                            }

                            if (mode == BulkGenreMode.ClearAll)
                                continue;

                            if (mode == BulkGenreMode.AddMerge)
                            {
                                // 既存維持＋不足分だけ追加（上限3）
                                var have = current.Select(c => (c.GenreId, c.SubGenreId)).ToHashSet();
                                var slots = MaxPerPhrase - have.Count;
                                if (slots <= 0) continue;

                                foreach (var (g, s) in wantOrdered)
                                {
                                    if (slots == 0) break;
                                    if (have.Add((g, s)))
                                    {
                                        toAdd.Add(new MPhraseGenre { PhraseId = p.PhraseId, GenreId = g, SubGenreId = s });
                                        slots--;
                                    }
                                }
                            }
                            else // ReplaceAll
                            {
                                foreach (var (g, s) in wantOrdered.Take(MaxPerPhrase))
                                    toAdd.Add(new MPhraseGenre { PhraseId = p.PhraseId, GenreId = g, SubGenreId = s });
                            }
                        }
                    }

                    if (toDelete.Count > 0) await repos.PhraseGenres.DeleteRangeAsync(toDelete);
                    if (toAdd.Count > 0) await repos.PhraseGenres.AddRangeAsync(toAdd);
                });

                return ServiceResult.None.Success("ジャンルを一括設定しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル一括設定で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DETAIL, "ジャンル一括設定"));
            }
        }

        /// <summary>更新（画像はUpsert、ジャンルは全差し替え）</summary>
        public async Task<ServiceResult<Unit>> UpdatePhraseAsync(PhraseEditModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var phraseEntity = await repos.Phrases.GetPhraseByIdAsync(model.Id);
                    if (phraseEntity == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // 本体更新（AutoDetectChanges OFF 前提なので Update 明示）
                    phraseEntity.Phrase = model.Phrase;
                    phraseEntity.Meaning = model.Meaning;
                    phraseEntity.Note = model.Note;
                    phraseEntity.UpdatedAt = DateTime.UtcNow;
                    await repos.Phrases.UpdateAsync(phraseEntity);

                    // 画像 Upsert
                    if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                    {
                        if (phraseEntity.DPhraseImage is null)
                        {
                            await repos.PhraseImages.AddAsync(model.ToImageEntity(DateTime.UtcNow));
                        }
                        else
                        {
                            phraseEntity.DPhraseImage.Url = model.ImageUrl;
                            phraseEntity.DPhraseImage.UpdatedAt = DateTime.UtcNow;
                            await repos.PhraseImages.UpdateAsync(phraseEntity.DPhraseImage);
                        }
                    }
                    else
                    {
                        if (phraseEntity.DPhraseImage is not null)
                            await repos.PhraseImages.DeleteAsync(phraseEntity.DPhraseImage);
                    }

                    // ジャンル全差し替え（子→親の順で安全）
                    if (phraseEntity.MPhraseGenres is { Count: > 0 })
                        await repos.PhraseGenres.DeleteRangeAsync(phraseEntity.MPhraseGenres);

                    if (model.SelectedDropItems is { Count: > 0 })
                        await repos.PhraseGenres.AddRangeAsync(model.ToPhraseGenreEntities());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ更新で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> DeletePhraseAsync(Guid phraseId)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var phrase = await repos.Phrases.GetPhraseByIdAsync(phraseId);
                    if (phrase == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // ★ フレーズ帳項目 / テスト結果明細 / （任意）復習ログ を先に削除
                    var pbItems = await repos.PhraseBookItems.GetByPhraseIdsAsync(new[] { phraseId });
                    if (pbItems.Count > 0) await repos.PhraseBookItems.DeleteRangeAsync(pbItems);

                    var trDetails = await repos.TestResultDetails.GetByPhraseIdsAsync(new[] { phraseId });
                    if (trDetails.Count > 0) await repos.TestResultDetails.DeleteRangeAsync(trDetails);



                    if (phrase.DPhraseImage is not null)
                        await repos.PhraseImages.DeleteAsync(phrase.DPhraseImage);

                    if (phrase.MPhraseGenres is { Count: > 0 })
                        await repos.PhraseGenres.DeleteRangeAsync(phrase.MPhraseGenres);

                    await repos.Phrases.DeleteAsync(phrase);

                    // 明細が全て消えて孤児になったテスト結果ヘッダを掃除したい場合は下記を検討
                    //await CleanupEmptyTestResultsAsync(repos);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ削除で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }


        public async Task<ServiceResult<Unit>> DeletePhrasesAsync(IEnumerable<Guid> phraseIds)
        {
            try
            {
                if (phraseIds is null || !phraseIds.Any())
                    return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_REQUIRED_DETAIL, "削除対象"));

                var idSet = phraseIds.Distinct().ToArray();

                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    const int chunkSize = 500;
                    var allPhrases = new List<DPhrase>(capacity: idSet.Length);

                    foreach (var chunk in idSet.Chunk(chunkSize))
                    {
                        // 本体（画像・ジャンル含む）を取得してバッファへ
                        var phrases = await repos.Phrases.GetByPhrasesIdsAsync(chunk);
                        allPhrases.AddRange(phrases);

                        // フレーズ帳項目 → テスト結果明細 を先に削除
                        var pbItems = await repos.PhraseBookItems.GetByPhraseIdsAsync(chunk);
                        if (pbItems.Count > 0) await repos.PhraseBookItems.DeleteRangeAsync(pbItems);

                        var trDetails = await repos.TestResultDetails.GetByPhraseIdsAsync(chunk);
                        if (trDetails.Count > 0) await repos.TestResultDetails.DeleteRangeAsync(trDetails);

                    }

                    if (allPhrases.Count != idSet.Length)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, MSG_PREFIX));

                    // 既存どおり：子 → 親
                    var genres = allPhrases.Where(p => p.MPhraseGenres is { Count: > 0 })
                                           .SelectMany(p => p.MPhraseGenres)
                                           .ToList();

                    var images = allPhrases.Where(p => p.DPhraseImage != null)
                                           .Select(p => p.DPhraseImage!)
                                           .ToList();

                    if (genres.Count > 0) await repos.PhraseGenres.DeleteRangeAsync(genres);
                    if (images.Count > 0) await repos.PhraseImages.DeleteRangeAsync(images);

                    await repos.Phrases.DeleteRangeAsync(allPhrases);

                    // 孤児ヘッダ掃除
                    //await CleanupEmptyTestResultsAsync(repos);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "フレーズ一括削除で例外が発生しました。");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        private static async Task CleanupEmptyTestResultsAsync(UowRepos repos)
        {
            // 明細が1件も無いテスト結果ヘッダを抽出して削除
            // ヘッダ側は削除するので NoTracking=false、サブクエリ側は読み取りのみなので NoTracking=true
            var emptyHeaders = await (
                from tr in repos.TestResults.Queryable(asNoTracking: false)
                where !repos.TestResultDetails.Queryable(true)
                             .Any(d => d.TestId == tr.TestId)
                select tr
            ).ToListAsync();

            if (emptyHeaders.Count > 0)
            {
                await repos.TestResults.DeleteRangeAsync(emptyHeaders);
            }
        }
    }
}
