using Bizkit_backend.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Bizkit_backend.DTOs.Resale;

public class CreateResaleListingRequestDto
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public ResaleCondition Condition { get; set; }
    public string? Description { get; set; }
    public IFormFile Image { get; set; } = null!;
}