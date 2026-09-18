using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace JCA.WorkSpace.Service.API.Tests.Config;

/// <summary>
/// DelegatingHandler que gera automaticamente um JWT de teste para cada requisição.
/// Extrai o userId do corpo da requisição (se disponível) para simular o comportamento
/// real onde o JWT é gerado com o ID do usuário autenticado.
/// </summary>
public class TestJwtDelegatingHandler : DelegatingHandler
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private static readonly Guid DefaultAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public TestJwtDelegatingHandler(string secret, string issuer, string audience)
    {
        _secret = secret;
        _issuer = issuer;
        _audience = audience;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userId = await TryExtractUserIdFromBodyAsync(request);

        var token = GenerateJwt(userId ?? DefaultAdminId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }

    private static async Task<Guid?> TryExtractUserIdFromBodyAsync(HttpRequestMessage request)
    {
        if (request.Content == null) return null;

        try
        {
            var bodyBytes = await request.Content.ReadAsByteArrayAsync();
            if (bodyBytes.Length == 0) return null;

            using var doc = JsonDocument.Parse(bodyBytes);
            var root = doc.RootElement;

            foreach (var propertyName in new[] { "userId", "UserId" })
            {
                if (root.TryGetProperty(propertyName, out var userIdElement) &&
                    userIdElement.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(userIdElement.GetString(), out var parsedId) &&
                    parsedId != Guid.Empty)
                {
                    request.Content = new ByteArrayContent(bodyBytes);
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    return parsedId;
                }
            }

            request.Content = new ByteArrayContent(bodyBytes);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }
        catch
        {
        }

        return null;
    }

    private string GenerateJwt(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "Manager"),
                new Claim(ClaimTypes.Role, "Facilities"),
                new Claim("role", "admin"),
                new Claim(JwtRegisteredClaimNames.Email, "test@workspace.com")
            }),
            NotBefore = DateTime.UtcNow.AddMinutes(-1),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _issuer,
            Audience = _audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
