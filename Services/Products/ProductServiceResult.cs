using Bizkit_backend.DTOs.Products;

namespace Bizkit_backend.Services.Products;

public sealed class ProductServiceResult
{
    private ProductServiceResult(
        bool succeeded,
        ProductResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public ProductResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static ProductServiceResult Success(
        ProductResponseDto response) =>
        new(true, response, []);

    public static ProductServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}