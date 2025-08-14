using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Dtos.Maps
{
    public static class ProverbImportMapper
    {
        public static IEnumerable<MProverb> ToEntities(this IEnumerable<ProverbImportDto> rows)
            => rows.Where(r => !string.IsNullOrWhiteSpace(r.Text))
           .Select(r => new MProverb
           {
               ProverbId = Guid.NewGuid(),
               ProverbText = r.Text!.Trim(),
               Meaning = string.IsNullOrWhiteSpace(r.Meaning) ? null : r.Meaning!.Trim(),
               Author = string.IsNullOrWhiteSpace(r.Author) ? null : r.Author!.Trim()
           });
    }
}
