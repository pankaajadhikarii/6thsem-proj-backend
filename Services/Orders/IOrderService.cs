using Bizkit_backend.DTOs.Orders;

namespace Bizkit_backend.Services.Orders;

public interface IOrderService
{
    Task<IReadOnlyCollection<OrderResponseDto>> GetMyOrdersAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<OrderResponseDto?> GetByIdAsync(
        int orderId,
        string customerId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);

    Task<OrderServiceResult> CreateAsync(
        string customerId,
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<OrderServiceResult> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken = default);
}