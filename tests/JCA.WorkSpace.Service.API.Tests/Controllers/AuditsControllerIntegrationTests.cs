using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Dtos.AuditLogs;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net.Http.Json;
using System.Text.Json;

namespace JCA.WorkSpace.Service.API.Tests;

public class AuditsControllerIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public AuditsControllerIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRecentLogs_ShouldContainAuditTrail_AfterCancelAction()
    {
        // Arrange
        var userCommand = new CreateUserCommand
        {
            Name = "Auditor Cancelamento",
            Email = $"audit_{Guid.NewGuid()}@empresa.com",
            Profile = UserProfile.Employee,
            Sector = "Facilities"
        };

        var userRes = await _client.PostAsJsonAsync("/api/Users", userCommand);
        var userContentStr = await userRes.Content.ReadAsStringAsync();

        userRes.IsSuccessStatusCode.Should().BeTrue($"Falha ao criar usuário. Retorno da API: {userContentStr}");

        var userContent = JsonSerializer.Deserialize<JsonElement>(userContentStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var userId = userContent.GetProperty("id").GetGuid();

        var spaceId = Guid.Parse("074891d0-865d-4bad-910a-f48e958f0f63");

        var resCommand = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = spaceId,
            StartTime = DateTime.UtcNow.AddDays(10),
            EndTime = DateTime.UtcNow.AddDays(10).AddHours(2)
        };

        var resResponse = await _client.PostAsJsonAsync("/api/Reservations", resCommand);
        var resContentStr = await resResponse.Content.ReadAsStringAsync();

        resResponse.IsSuccessStatusCode.Should().BeTrue($"Falha ao criar reserva. Retorno da API: {resContentStr}");

        var resContent = JsonSerializer.Deserialize<JsonElement>(resContentStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var reservationId = resContent.GetProperty("id").GetGuid();

        var cancelCommand = new CancelReservationCommand
        {
            UserId = userId,
            ReservationId = reservationId
        };

        var cancelResponse = await _client.PatchAsJsonAsync($"/api/Reservations/{reservationId}/cancel", cancelCommand);
        var cancelContentStr = await cancelResponse.Content.ReadAsStringAsync();

        cancelResponse.IsSuccessStatusCode.Should().BeTrue($"Falha ao cancelar reserva. Retorno da API: {cancelContentStr}");

        // Act
        var response = await _client.GetAsync("/api/Audits?limit=50");
        var auditsContentStr = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue($"Falha ao buscar auditoria. Retorno da API: {auditsContentStr}");

        var logs = JsonSerializer.Deserialize<List<AuditLogDto>>(auditsContentStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        logs.Should().NotBeNull();
        logs.Should().NotBeEmpty();

        logs.Should().Contain(l => l.EntityId == reservationId && l.Action.Contains("Cancelamento"));
    }
}