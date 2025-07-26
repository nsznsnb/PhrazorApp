using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Services
{
    public interface IPhraseService
    {
        public Task<List<PhraseModel>> GetPhraseViewModelListAsync();

        public Task<PhraseModel> GetPhraseViewModelAsync(Guid largePhraseId);

        public Task<IServiceResult> CreatePhraseAsync(PhraseModel model);

        public Task<IServiceResult> UpdatePhraseAsync(PhraseModel model);

        public Task<IServiceResult> DeletePhraseAsync(Guid largePhraseId);
    }
    public class PhraseService
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
        private readonly IUserService _userService;
        private readonly ILogger<PhraseService> _logger;

        public PhraseService(IDbContextFactory<EngDbContext> dbContextFactory, IUserService userService, ILogger<PhraseService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _userService = userService;
            _logger = logger;
        }
    }
}
