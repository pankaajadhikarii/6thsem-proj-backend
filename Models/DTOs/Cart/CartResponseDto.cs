namespace Bizkit_backend.DTOs.Cart;

public class CartResponseDto
{
    public int Id { get; set; }
    public ICollection<CartItemResponseDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public DateTime UpdatedAt { get; set; }
}