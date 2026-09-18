using Bizkit_backend.Models.Enums;

namespace Bizkit_backend.DTOs.Orders;

public class CreateOrderRequestDto
{
    public string ShippingAddress { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
}