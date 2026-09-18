namespace Bizkit_backend.DTOs.Cart;

public class AddCartItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}