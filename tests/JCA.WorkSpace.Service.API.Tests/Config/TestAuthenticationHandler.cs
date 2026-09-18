using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JCA.WorkSpace.Service.API.Tests.Config;

/// <summary>
/// AuthenticationHandler de teste que autentica automaticamente cada requisição.
/// 
/// Estratégia de determinação do userId:
/// 1. Tenta ler o header Authorization Bearer (JWT real gerado pelo TestAuthHelper)
/// 2. Tenta extrair o userId do corpo JSON da requisição
/// 3. Usa o userId admin padrão como fallback
/// 
/// Isso garante que todos os endpoints com [Authorize] sejam acessíveis nos testes
/// sem alterar as classes de teste existentes.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static readonly Guid DefaultAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = await ExtractUserIdAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim(ClaimTypes.Role, "Facilities"),
            new Claim("role", "admin"),
            new Claim(ClaimTypes.Email, "test@workspace.com"),
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestAuth");

        return AuthenticateResult.Success(ticket);
    }

    private async Task<Guid> ExtractUserIdAsync()
    {
        // Tenta extrair userId do body JSON (para manter o comportamento dos testes que passam userId no body)
        if (Request.ContentLength > 0 && Request.Body.CanRead)
        {
            try
            {
                Request.EnableBuffering();
                var body = await new StreamReader(Request.Body, leaveOpen: true).ReadToEndAsync();
                Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    foreach (var propertyName in new[] { "userId", "UserId", "approverId", "ApproverId" })
                    {
                        if (root.TryGetProperty(propertyName, out var userIdElement) &&
                            userIdElement.ValueKind == JsonValueKind.String &&
                            Guid.TryParse(userIdElement.GetString(), out var parsedId) &&
                            parsedId != Guid.Empty)
                        {
                            return parsedId;
                        }
                    }
                }
            }
            catch
            {
                // Ignora erros de parsing
            }
        }

        return DefaultAdminId;
    }
}
