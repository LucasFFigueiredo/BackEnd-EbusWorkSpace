using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Reservations;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net.Http.Json;
using System.Text.Json;

namespace JCA.WorkSpace.Service.API.Tests;

public class ReservationsEndToEndIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public ReservationsEndToEndIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateTestUserAsync()
    {
        var command = new CreateUserCommand
        {
            Name = "Testador de Integração",
            Email = $"integration_{Guid.NewGuid()}@jcatlm.com",
            Profile = UserProfile.Employee,
            Sector = "QA"
        };

        var response = await _client.PostAsJsonAsync("/api/Users", command);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        return content.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task CreateReservation_ShouldSucceed_WhenSpaceIsFree()
    {
        // Arrange
        var userId = await CreateTestUserAsync();
        var salaBeloHorizonteId = Guid.Parse("a090562b-4727-4503-b4ac-118867d3fbd5");

        var command = new CreateReservationCommand
        {
            UserId = userId,
            SpaceId = salaBeloHorizonteId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations", command);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();

        content.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateReservation_ShouldFailWith400_WhenDoubleBookingOccurs()
    {
        // Arrange
        var userId1 = await CreateTestUserAsync();
        var userId2 = await CreateTestUserAsync();

        var mesa01Id = Guid.Parse("074891d0-865d-4bad-910a-f48e958f0f63");
        var tomorrow = DateTime.UtcNow.AddDays(2);

        var firstCommand = new CreateReservationCommand
        {
            UserId = userId1,
            SpaceId = mesa01Id,
            StartTime = tomorrow.AddHours(14),
            EndTime = tomorrow.AddHours(18)
        };

        var overlappingCommand = new CreateReservationCommand
        {
            UserId = userId2,
            SpaceId = mesa01Id,
            StartTime = tomorrow.AddHours(15),
            EndTime = tomorrow.AddHours(16)
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync("/api/Reservations", firstCommand);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var secondResponse = await _client.PostAsJsonAsync("/api/Reservations", overlappingCommand);

        // Assert
        secondResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var errorContent = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        errorContent.GetProperty("result").GetString().Should().Contain("Conflito!");
    }

    [Fact]
    public async Task CreateReservation_ShouldFailWith400_WhenSpaceIsBlockedForMaintenance()
    {
        // Arrange - Criar usuário admin que vai bloquear a sala
        var adminUser = await CreateTestUserAsync();

        // Criar um espaço dedicado para este teste
        var createSpacePayload = new
        {
            Name = "Sala Teste Bloqueio",
            Type = 1, // Room = 1
            Floor = 2,
            Capacity = 4,
            IsBlocked = false,
            RequiresApproval = false
        };

        var createSpaceResponse = await _client.PostAsJsonAsync("/api/Spaces", createSpacePayload);
        createSpaceResponse.EnsureSuccessStatusCode();
        var spaceContent = await createSpaceResponse.Content.ReadFromJsonAsync<JsonElement>();
        var spaceId = spaceContent.GetProperty("id").GetGuid();

        // Bloquear a sala via PATCH /maintenance, passando UserId no body (sem JWT ativo)
        var blockPayload = new
        {
            UserId = adminUser,
            IsBlocked = true,
            MaintenanceReason = "Ar condicionado quebrado",
            MaintenanceUntil = (string?)null
        };

        var blockResponse = await _client.PatchAsJsonAsync($"/api/Spaces/{spaceId}/maintenance", blockPayload);
        blockResponse.EnsureSuccessStatusCode();

        // Tentar criar uma reserva no espaço bloqueado
        var command = new CreateReservationCommand
        {
            UserId = adminUser,
            SpaceId = spaceId,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Reservations", command);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadFromJsonAsync<JsonElement>();
        errorContent.GetProperty("result").GetString().Should().Contain("Espaço bloqueado para manutenção");
    }
}