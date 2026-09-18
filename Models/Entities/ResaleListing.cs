using Bizkit_backend.Models.Enums;

namespace Bizkit_backend.Models.Entities;

public class ResaleListing
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string SellerId { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ResaleCondition Condition { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public ResaleListingStatus Status { get; set; }
        = ResaleListingStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;

    public ApplicationUser Seller { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}