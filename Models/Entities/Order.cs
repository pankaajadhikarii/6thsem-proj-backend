using Bizkit_backend.Models.Enums;

namespace Bizkit_backend.Models.Entities;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public OrderPaymentStatus PaymentStatus { get; set; }
        = OrderPaymentStatus.Unpaid;

    public PaymentMethod PaymentMethod { get; set; }

    public OrderStatus Status { get; set; }
        = OrderStatus.Pending;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Customer { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = [];

    public ICollection<Payment> Payments { get; set; } = [];
}