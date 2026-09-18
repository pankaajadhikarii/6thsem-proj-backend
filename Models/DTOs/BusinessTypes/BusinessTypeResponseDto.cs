namespace Bizkit_backend.DTOs.BusinessTypes;

public class BusinessTypeResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public ICollection<BusinessTypeProductResponseDto> Products { get; set; } = [];
}