namespace Bizkit_backend.DTOs.BusinessTypes;

public class BusinessTypeProductResponseDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsRequired { get; set; }

    public int RecommendedQuantity { get; set; }

    public int DisplayOrder { get; set; }
}