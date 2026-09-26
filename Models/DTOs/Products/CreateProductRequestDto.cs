using Microsoft.AspNetCore.Http;

namespace Bizkit_backend.DTOs.Products;

public class CreateProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public IFormFile Image { get; set; } = null!;
    public int CategoryId { get; set; }
    public int? SupersedesProductId { get; set; }
}