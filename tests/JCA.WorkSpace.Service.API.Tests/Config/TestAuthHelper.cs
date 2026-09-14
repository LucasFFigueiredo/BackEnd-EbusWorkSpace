using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace JCA.WorkSpace.Service.API.Tests;

public static class TestAuthHelper
{
    public static void AuthenticateClient(HttpClient client, IConfiguration config, string userId = "00000000-0000-0000-0000-000000000001")
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var secret = config["JwtSettings:Secret"] ?? "REPLACE_WITH_A_SECURE_KEY_AT_LEAST_32_CHARACTERS_LONG";
        var issuer = config["JwtSettings:Issuer"] ?? "WorkSpace-API";
        var audience = config["JwtSettings:Audience"] ?? "WorkSpace-WebApp";

        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim("role", "admin"),
                new Claim(JwtRegisteredClaimNames.Email, "teste@jcatlm.com.br")
            }),

            NotBefore = DateTime.UtcNow.AddMinutes(-1),
            Expires = DateTime.UtcNow.AddHours(1),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),

            Issuer = issuer,
            Audience = audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }
}