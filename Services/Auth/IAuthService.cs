using Bizkit_backend.DTOs.Auth;

namespace Bizkit_backend.Services.Auth;

public interface IAuthService
{
    Task<UserResponseDto?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<AuthServiceResult> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthServiceResult> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}