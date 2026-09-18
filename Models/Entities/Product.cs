namespace Bizkit_backend.Models.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public int? SupersedesProductId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;

    public Product? SupersedesProduct { get; set; }

    public ICollection<Product> NewerProducts { get; set; } = [];

    public ICollection<BusinessTypeProduct> BusinessTypeProducts { get; set; } = [];

    public ICollection<CartItem> CartItems { get; set; } = [];

    public ICollection<OrderItem> OrderItems { get; set; } = [];

    public ICollection<ResaleListing> ResaleListings { get; set; } = [];

    public ICollection<Notification> NewProductNotifications { get; set; } = [];

    public ICollection<Notification> OldProductNotifications { get; set; } = [];
}