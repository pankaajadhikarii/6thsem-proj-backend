using Bizkit_backend.Models.Enums;

namespace Bizkit_backend.DTOs.Resale;

public class CreateResaleListingRequestDto
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public ResaleCondition Condition { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}