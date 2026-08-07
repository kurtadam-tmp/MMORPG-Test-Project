using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const string SecretKey = "MMORPG_SECRET_JWT_KEY_SUPER_SECURE_PRODUCTION_2026_VERY_LONG!";
    private const string Issuer = "MMORPG.GatewayApi";
    private const string Audience = "MMORPG.Clients";

    public string GenerateToken(Guid playerId, string username, string role = "Player")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("TokenType", "SessionToken")
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
