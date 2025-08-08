using System.Threading;
using Microsoft.AspNetCore.Http;

namespace PhrazorApp.Services
{
    public interface ICancellationTokenProvider
    {
        CancellationToken Token { get; }
    }

    public sealed class HttpCancellationTokenProvider : ICancellationTokenProvider
    {
        private readonly IHttpContextAccessor _accessor;
        public HttpCancellationTokenProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
        public CancellationToken Token => _accessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    }
}