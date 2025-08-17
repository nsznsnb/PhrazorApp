using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    /// <summary>
    /// 英語日記 マッピング（Entity ↔ Model）
    /// </summary>
    public static class EnglishDiaryModelMapper
    {
        // =========================
        // Entity -> Model
        // =========================
        public static EnglishDiaryModel ToModel(this DEnglishDiary e) => new()
        {
            Id = e.DiaryId,
            Title = e.Title ?? string.Empty,
            Content = e.Content ?? string.Empty,
            Note = e.Note,
            Explanation = e.Explanation,
            Correction = e.Correction,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            TagIds = (e.DEnglishDiaryTags != null)
                            ? e.DEnglishDiaryTags.Select(t => t.DiaryTagId).ToList()
                            : new List<Guid>()
        };

        // =========================
        // Model -> 新規 Entity
        // =========================
        public static DEnglishDiary ToEntity(this EnglishDiaryModel m, string userId)
        {
            var id = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id;
            return new DEnglishDiary
            {
                DiaryId = id,
                UserId = userId,
                Title = m.Title ?? string.Empty,
                Content = m.Content ?? string.Empty,
                Note = m.Note,
                Explanation = m.Explanation,
                Correction = m.Correction,
                DEnglishDiaryTags = (m.TagIds != null && m.TagIds.Count > 0)
                    ? m.TagIds.Distinct().Select(tagId => new DEnglishDiaryTag
                    {
                        DiaryId = id,
                        DiaryTagId = tagId
                    }).ToList()
                    : new List<DEnglishDiaryTag>()
            };
        }

        // =========================
        // Model -> 既存 Entity を更新
        // =========================
        /// <summary>
        /// 既存エンティティへ反映（Tags は全置換）。
        /// ※ 呼び出し前に entity.DEnglishDiaryTags は Include 済みを推奨
        /// </summary>
        public static void ApplyTo(this EnglishDiaryModel m, DEnglishDiary entity)
        {
            entity.Title = m.Title ?? string.Empty;
            entity.Content = m.Content ?? string.Empty;
            entity.Note = m.Note;
            entity.Explanation = m.Explanation;
            entity.Correction = m.Correction;

            // タグ同期（全置換）
            entity.DEnglishDiaryTags ??= new List<DEnglishDiaryTag>();
            entity.DEnglishDiaryTags.Clear();

            if (m.TagIds is { Count: > 0 })
            {
                foreach (var tagId in m.TagIds.Distinct())
                {
                    entity.DEnglishDiaryTags.Add(new DEnglishDiaryTag
                    {
                        DiaryId = entity.DiaryId,
                        DiaryTagId = tagId
                    });
                }
            }
        }

        // =========================
        // 投影（一覧/詳細 共通）
        // =========================
        public static readonly Expression<Func<DEnglishDiary, EnglishDiaryModel>> ListProjection
            = e => new EnglishDiaryModel
            {
                Id = e.DiaryId,
                Title = e.Title ?? string.Empty,
                Content = e.Content ?? string.Empty,
                Note = e.Note,
                Explanation = e.Explanation,
                Correction = e.Correction,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                TagIds = e.DEnglishDiaryTags.Select(t => t.DiaryTagId).ToList()
            };

        public static IQueryable<EnglishDiaryModel> SelectModel(this IQueryable<DEnglishDiary> q)
            => q.Select(ListProjection);

        // =========================
        // カレンダー用（← EF で投影不可なのでメソッドに分離）
        // =========================
        public static DiaryCalendarItem ToCalendarItem(this EnglishDiaryModel m)
        {
            // CreatedAt の日付で終日イベント化（1日1件前提）
            var y = m.CreatedAt.Year;
            var mo = m.CreatedAt.Month;
            var d = m.CreatedAt.Day;
            var start = new DateTime(y, mo, d, 0, 0, 0);
            var end = new DateTime(y, mo, d, 23, 59, 59);

            return new DiaryCalendarItem
            {
                DiaryId = m.Id,
                Title = m.Title ?? string.Empty,
                Start = start,
                End = end,
                AllDay = true,
                Text = m.Title ?? string.Empty
            };
        }

        public static DiaryCalendarItem ToCalendarItem(this DEnglishDiary e)
            => e.ToModel().ToCalendarItem();
    }
}
