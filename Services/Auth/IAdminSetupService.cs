namespace Bizkit_backend.Services.Admin;

public interface IAdminSetupService
{
    Task SetupAsync(CancellationToken cancellationToken = default);
}