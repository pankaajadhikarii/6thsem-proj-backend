using Bizkit_backend.DTOs.Products;

namespace Bizkit_backend.Services.Products;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductResponseDto>> GetAllAsync(
        int? categoryId = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<ProductResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ProductServiceResult> CreateAsync(
        CreateProductRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ProductServiceResult> UpdateAsync(
        int id,
        UpdateProductRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}