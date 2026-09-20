using Bizkit_backend.DTOs.Resale;

namespace Bizkit_backend.Services.Resale;

public sealed class ResaleServiceResult
{
    private ResaleServiceResult(
        bool succeeded,
        ResaleListingResponseDto? response,
        int? orderId,
        string? orderNumber,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        OrderId = orderId;
        OrderNumber = orderNumber;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public ResaleListingResponseDto? Response { get; }

    public int? OrderId { get; }

    public string? OrderNumber { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static ResaleServiceResult Success(
        ResaleListingResponseDto response) =>
        new(
            true,
            response,
            null,
            null,
            []);

    public static ResaleServiceResult PurchaseSuccess(
        int orderId,
        string orderNumber) =>
        new(
            true,
            null,
            orderId,
            orderNumber,
            []);

    public static ResaleServiceResult Failure(
        params string[] errors) =>
        new(
            false,
            null,
            null,
            null,
            errors);
}