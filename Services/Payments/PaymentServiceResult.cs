using Bizkit_backend.DTOs.Payments;

namespace Bizkit_backend.Services.Payments;

public sealed class PaymentServiceResult
{
    private PaymentServiceResult(
        bool succeeded,
        PaymentResponseDto? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public PaymentResponseDto? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static PaymentServiceResult Success(
        PaymentResponseDto response) =>
        new(true, response, []);

    public static PaymentServiceResult Failure(
        params string[] errors) =>
        new(false, null, errors);
}