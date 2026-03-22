using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Helper;

/// <summary>
/// Creates and signs JWT access and refresh tokens using settings from configuration (JWT:Key, issuer, audience).
/// </summary>
public class Jwthelper
{
    private readonly IConfiguration _configuration;

    public Jwthelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Builds an access token with user id, email, and role claims.</summary>
    public string GenerateJwtToken(string userId, string email, UserRole role)
    {
        var issuer = _configuration["JWT:issuer"]!;
        var audience = _configuration["JWT:audience"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Opaque refresh token (no user claims).</summary>
    public string GenerateJwtRefreshToken()
    {
        var issuer = _configuration["JWT:issuer"]!;
        var audience = _configuration["JWT:audience"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
