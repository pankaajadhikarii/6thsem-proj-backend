using Bizkit_backend.DTOs.Categories;

namespace Bizkit_backend.Services.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CategoryResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CategoryServiceResult> CreateAsync(
        CreateCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<CategoryServiceResult> UpdateAsync(
        int id,
        UpdateCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
