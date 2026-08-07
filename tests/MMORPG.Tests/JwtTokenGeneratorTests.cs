using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using MMORPG.Infrastructure.Services;
using Xunit;

namespace MMORPG.Tests;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _jwtGenerator;

    public JwtTokenGeneratorTests()
    {
        _jwtGenerator = new JwtTokenGenerator();
    }

    [Fact]
    public void GenerateToken_ValidPlayerCredentials_ReturnsSignedTokenWithClaims()
    {
        var playerId = Guid.NewGuid();
        string username = "WarriorLegend";

        string token = _jwtGenerator.GenerateToken(playerId, username, "Player");

        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Read claims back from generated JWT Token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value;
        Assert.Equal(username, nameClaim);
    }
}
