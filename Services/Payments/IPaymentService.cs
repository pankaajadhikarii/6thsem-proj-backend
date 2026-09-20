using Bizkit_backend.DTOs.Payments;

namespace Bizkit_backend.Services.Payments;

public interface IPaymentService
{
    Task<PaymentResponseDto?> GetByIdAsync(
        int paymentId,
        string userId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default);

    Task<PaymentServiceResult> MarkCodAsPaidAsync(
        int paymentId,
        CancellationToken cancellationToken = default);
}