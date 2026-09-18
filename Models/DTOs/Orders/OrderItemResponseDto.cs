namespace Bizkit_backend.DTOs.Orders;

public class OrderItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ResaleListingId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}