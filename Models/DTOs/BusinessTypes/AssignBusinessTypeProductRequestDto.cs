namespace Bizkit_backend.DTOs.BusinessTypes;

public class AssignBusinessTypeProductRequestDto
{
    public int ProductId { get; set; }

    public bool IsRequired { get; set; }

    public int RecommendedQuantity { get; set; } = 1;

    public int DisplayOrder { get; set; }
}