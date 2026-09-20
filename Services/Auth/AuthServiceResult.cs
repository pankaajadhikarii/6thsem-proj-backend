using Bizkit_backend.DTOs.Auth;

namespace Bizkit_backend.Services.Auth;

public sealed class AuthServiceResult
{
    private AuthServiceResult(
        bool succeeded,
        AuthResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public AuthResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static AuthServiceResult Success(
        AuthResponseDto response) =>
        new(true, response, []);

    public static AuthServiceResult Failure(
        IEnumerable<string> errors) =>
        new(false, null, errors.ToArray());
}