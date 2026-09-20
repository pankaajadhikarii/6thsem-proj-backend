using Bizkit_backend.DTOs.Categories;

namespace Bizkit_backend.Services.Categories;

public sealed class CategoryServiceResult
{
    private CategoryServiceResult(
        bool succeeded,
        CategoryResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public CategoryResponseDto? Response { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static CategoryServiceResult Success(
        CategoryResponseDto response) =>
        new(true, response, []);

    public static CategoryServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}
