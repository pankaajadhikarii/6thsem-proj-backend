namespace Bizkit_backend.Configuration;

public class AdminSettings
{
    public AdminAccount Account1 { get; set; } = new();
    public AdminAccount Account2 { get; set; } = new();
}

public class AdminAccount
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}