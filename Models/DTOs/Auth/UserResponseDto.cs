namespace Bizkit_backend.DTOs.Auth;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? ProfileImageUrl { get; set; }

    public List<string> Roles { get; set; } = [];
}