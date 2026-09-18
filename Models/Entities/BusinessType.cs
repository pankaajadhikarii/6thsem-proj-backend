namespace Bizkit_backend.Models.Entities;

public class BusinessType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<BusinessTypeProduct> BusinessTypeProducts { get; set; } = [];
}