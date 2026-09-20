using Bizkit_backend.Data;
using Bizkit_backend.DTOs.Payments;
using Bizkit_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bizkit_backend.Services.Payments;

public sealed class PaymentService(
    ApplicationDbContext dbContext) : IPaymentService
{
    public async Task<PaymentResponseDto?> GetByIdAsync(
        int paymentId,
        string userId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Payments
            .AsNoTracking()
            .Include(payment => payment.Order)
            .Where(payment => payment.Id == paymentId);

        if (!isAdmin)
        {
            query = query.Where(
                payment => payment.Order.CustomerId == userId);
        }

        var payment = await query
            .FirstOrDefaultAsync(cancellationToken);

        return payment is null
            ? null
            : ToResponse(payment);
    }

    public async Task<PaymentServiceResult> MarkCodAsPaidAsync(
        int paymentId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var payment = await dbContext.Payments
            .Include(item => item.Order)
            .FirstOrDefaultAsync(
                item => item.Id == paymentId,
                cancellationToken);

        if (payment is null)
        {
            return PaymentServiceResult.Failure(
                "Payment not found.");
        }

        if (payment.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            return PaymentServiceResult.Failure(
                "This payment is not a Cash on Delivery payment.");
        }

        if (payment.Status == PaymentStatus.Success)
        {
            return PaymentServiceResult.Failure(
                "Payment has already been completed.");
        }

        if (payment.Order.Status == OrderStatus.Cancelled)
        {
            return PaymentServiceResult.Failure(
                "Payment cannot be completed for a cancelled order.");
        }

        if (payment.Order.Status != OrderStatus.Delivered)
        {
            return PaymentServiceResult.Failure(
                "Cash on Delivery payment can only be completed after the order is delivered.");
        }

        var now = DateTime.UtcNow;

        payment.Status = PaymentStatus.Success;
        payment.PaidAt = now;
        payment.UpdatedAt = now;

        payment.Order.PaymentStatus =
            OrderPaymentStatus.Paid;

        payment.Order.UpdatedAt = now;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return PaymentServiceResult.Success(
            ToResponse(payment));
    }

    private static PaymentResponseDto ToResponse(
        Models.Entities.Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentMethod = payment.PaymentMethod,
            Amount = payment.Amount,
            TransactionReference =
                payment.TransactionReference,
            Status = payment.Status,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}