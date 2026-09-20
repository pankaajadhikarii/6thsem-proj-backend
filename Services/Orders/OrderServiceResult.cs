using Bizkit_backend.DTOs.Orders;

namespace Bizkit_backend.Services.Orders;

public sealed class OrderServiceResult
{
    private OrderServiceResult(
        bool succeeded,
        OrderResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public OrderResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static OrderServiceResult Success(
        OrderResponseDto response) =>
        new(true, response, []);

    public static OrderServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}