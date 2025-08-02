using System.Threading;
using System.Threading.Tasks;

namespace VetData.Client.Services.Auth;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}