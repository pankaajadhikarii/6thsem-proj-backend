using Bizkit_backend.DTOs.Cart;

namespace Bizkit_backend.Services.Cart;

public interface ICartService
{
    Task<CartResponseDto> GetAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CartServiceResult> AddItemAsync(
        string userId,
        AddCartItemRequestDto request,
        CancellationToken cancellationToken = default);

    Task<CartServiceResult> UpdateItemAsync(
        string userId,
        int itemId,
        UpdateCartItemRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(
        string userId,
        int itemId,
        CancellationToken cancellationToken = default);

    Task ClearAsync(
        string userId,
        CancellationToken cancellationToken = default);
}