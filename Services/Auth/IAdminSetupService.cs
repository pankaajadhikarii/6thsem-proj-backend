namespace Bizkit_backend.Services.Auth;

public interface IAdminSetupService
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}