using Bizkit_backend.DTOs.Cart;

namespace Bizkit_backend.Services.Cart;

public sealed class CartServiceResult
{
    private CartServiceResult(
        bool succeeded,
        CartResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public CartResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static CartServiceResult Success(
        CartResponseDto response) =>
        new(true, response, []);

    public static CartServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}