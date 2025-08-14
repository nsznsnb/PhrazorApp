// Services/TestService.cs
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Components.State;

namespace PhrazorApp.Services
{
    public sealed class TestService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;
        private readonly ILogger<TestService> _logger;
        private const string MSG_PREFIX = "テスト設定";

        public TestService(UnitOfWork uow, UserService user, ILogger<TestService> logger)
        {
            _uow = uow;
            _user = user;
            _logger = logger;
        }


    }
}
