using Bizkit_backend.Models.Enums;

namespace Bizkit_backend.DTOs.Orders;

public class UpdateOrderStatusRequestDto
{
    public OrderStatus Status { get; set; }
}