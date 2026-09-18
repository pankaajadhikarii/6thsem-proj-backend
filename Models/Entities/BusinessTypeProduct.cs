namespace Bizkit_backend.Models.Entities;

public class BusinessTypeProduct
{
    public int BusinessTypeId { get; set; }
    public int ProductId { get; set; }

    public bool IsRequired { get; set; }
    public int RecommendedQuantity { get; set; } = 1;
    public int DisplayOrder { get; set; }

    public BusinessType BusinessType { get; set; } = null!;
    public Product Product { get; set; } = null!;
}