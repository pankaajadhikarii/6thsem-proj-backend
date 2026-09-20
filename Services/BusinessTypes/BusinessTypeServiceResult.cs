using Bizkit_backend.DTOs.BusinessTypes;

namespace Bizkit_backend.Services.BusinessTypes;

public sealed class BusinessTypeServiceResult
{
    private BusinessTypeServiceResult(
        bool succeeded,
        BusinessTypeResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public BusinessTypeResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static BusinessTypeServiceResult Success(
        BusinessTypeResponseDto response) =>
        new(true, response, []);

    public static BusinessTypeServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}