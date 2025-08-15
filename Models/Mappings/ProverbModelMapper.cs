using PhrazorApp.Data.Entities;
using System.Linq.Expressions;

namespace PhrazorApp.Models.Mappings
{
    public static class ProverbModelMapper
    {
        public static ProverbModel ToModel(this MProverb e) => new()
        {
            Id = e.ProverbId,
            Text = e.ProverbText ?? string.Empty,
            Meaning = e.Meaning,
            Author = e.Author,
            CreatedAt = e.CreatedAt
        };

        public static void ApplyTo(this ProverbModel m, MProverb e)
        {
            e.ProverbText = m.Text;
            e.Author = m.Author;
            e.Meaning = m.Meaning;
        }

        public static MProverb ToEntityForCreate(this ProverbModel m) => new()
        {
            ProverbId = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id,
            ProverbText = m.Text,
            Meaning = m.Meaning,
            Author = m.Author
        };

        public static readonly Expression<Func<MProverb, ProverbModel>> ListProjection
            = x => new ProverbModel
            {
                Id = x.ProverbId,
                Text = x.ProverbText ?? string.Empty,
                Meaning = x.Meaning,
                Author = x.Author,
                CreatedAt = x.CreatedAt
            };
    }
}
