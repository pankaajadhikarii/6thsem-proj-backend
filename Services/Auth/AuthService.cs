using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bizkit_backend.DTOs.Auth;
using Bizkit_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Bizkit_backend.Services.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<UserResponseDto?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return await ToUserResponse(user);
    }

    public async Task<AuthServiceResult> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return AuthServiceResult.Failure(
                createResult.Errors.Select(
                    error => error.Description));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(
            user,
            "Customer");

        if (!addToRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return AuthServiceResult.Failure(
                addToRoleResult.Errors.Select(
                    error => error.Description));
        }

        var response = await CreateAuthResponseAsync(user);

        return AuthServiceResult.Success(response);
    }

    public async Task<AuthServiceResult> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null ||
            !user.IsActive ||
            !await userManager.CheckPasswordAsync(
                user,
                request.Password))
        {
            return AuthServiceResult.Failure(
                ["Invalid email or password."]);
        }

        var response = await CreateAuthResponseAsync(user);

        return AuthServiceResult.Success(response);
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(
        ApplicationUser user)
    {
        var signingKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var expirationMinutes = configuration.GetValue<int?>(
            "Jwt:ExpirationMinutes") ?? 60;

        if (expirationMinutes <= 0)
        {
            expirationMinutes = 60;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(
            expirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),

            new(
                ClaimTypes.NameIdentifier,
                user.Id),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Email,
                user.Email ?? string.Empty)
        };

        var roles = await userManager.GetRolesAsync(user);

        claims.AddRange(
            roles.Select(role =>
                new Claim(
                    ClaimTypes.Role,
                    role)));

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),

            ExpiresAt = expiresAt,

            User = await ToUserResponse(user)
        };
    }

    private async Task<UserResponseDto> ToUserResponse(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles = roles.ToList()
        };
    }
}