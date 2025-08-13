using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Dtos.Maps
{
    public static class PhraseImportMapper
    {
        /// <summary>
        /// CSV用DTO → PhraseModel
        /// </summary>
        public static PhraseEditModel ToPhraseModel(this PhraseImportDto dto)
        {
            return new PhraseEditModel
            {
                Id = Guid.NewGuid(),
                Phrase = dto.Phrase,
                Meaning = dto.Meaning,
                Note = string.Empty,
                ImageUrl = string.Empty,
                ReviewCount = 0,
                SelectedDropItems = new()
            };
        }

        /// <summary>
        /// CSV用DTO → DPhrase（エンティティ）
        /// </summary>
        public static DPhrase ToPhraseEntity(this PhraseImportDto dto, string userId)
        {
            return new DPhrase
            {
                PhraseId = Guid.NewGuid(),
                Phrase = dto.Phrase,
                Meaning = dto.Meaning,
                Note = string.Empty,
                UserId = userId
            };
        }

        /// <summary>
        /// CSV用DTOのリスト → PhraseModelリスト
        /// </summary>
        public static List<PhraseEditModel> ToPhraseModels(this IEnumerable<PhraseImportDto> dtos)
        {
            return dtos.Select(d => d.ToPhraseModel()).ToList();
        }

        /// <summary>
        /// CSV用DTOのリスト → DPhraseリスト
        /// </summary>
        public static List<DPhrase> ToPhraseEntities(this IEnumerable<PhraseImportDto> dtos, string userId)
        {
            return dtos.Select(d => d.ToPhraseEntity(userId)).ToList();
        }
    }
}
