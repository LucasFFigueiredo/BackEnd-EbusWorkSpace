using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Dtos.Users;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JCA.WorkSpace.Service.API.Tests;

public class UsersControllerIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public UsersControllerIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ShouldReturn200AndId_WhenEmailIsUnique()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Mariana Costa",
            Email = $"mariana_{Guid.NewGuid()}@empresa.com",
            Sector = "Tecnologia",
            Profile = UserProfile.Employee
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_ShouldFailWith400_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@empresa.com";
        var firstCommand = new CreateUserCommand
        {
            Name = "Primeiro Cadastro",
            Email = email,
            Sector = "Operações",
            Profile = UserProfile.Employee
        };

        var secondCommand = new CreateUserCommand
        {
            Name = "Segundo Cadastro",
            Email = email,
            Sector = "Operações",
            Profile = UserProfile.Manager
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync("/api/Users", firstCommand);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var secondResponse = await _client.PostAsJsonAsync("/api/Users", secondCommand);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        errorContent.GetProperty("result").GetString().Should().Contain("Já existe um usuário cadastrado com este e-mail.");
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnListOfRegisteredUsers()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "Usuario Listagem",
            Email = $"listagem_{Guid.NewGuid()}@empresa.com",
            Sector = "Financeiro",
            Profile = UserProfile.Employee
        };
        await _client.PostAsJsonAsync("/api/Users", command);

        // Act
        var response = await _client.GetAsync("/api/Users");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().Contain(u => u.Email == command.Email);
    }
}