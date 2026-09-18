using Microsoft.AspNetCore.Identity;

namespace Bizkit_backend.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Cart? Cart { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    // public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<ResaleListing> ResaleListings { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}