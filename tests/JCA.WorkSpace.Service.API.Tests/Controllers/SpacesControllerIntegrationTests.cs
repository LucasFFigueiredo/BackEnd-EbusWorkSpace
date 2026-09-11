using FluentAssertions;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Service.API.Tests.Config;
using System.Net.Http.Json;

namespace JCA.WorkSpace.Service.API.Tests;

public class SpacesControllerIntegrationTests : IClassFixture<WorkSpaceApiFactory>
{
    private readonly HttpClient _client;

    public SpacesControllerIntegrationTests(WorkSpaceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllSpaces_ShouldReturnSeededSpaces_FromDockerDatabase()
    {
        var response = await _client.GetAsync("/api/Spaces");
        response.IsSuccessStatusCode.Should().BeTrue();

        var spaces = await response.Content.ReadFromJsonAsync<List<SpaceDto>>();
        spaces.Should().NotBeNull();
        spaces!.Count.Should().BeGreaterThanOrEqualTo(34);
    }

    [Fact]
    public async Task GetSpacesByFloor_ShouldReturnOnlySpacesFromSpecificFloor()
    {
        // Act:
        var response = await _client.GetAsync("/api/Spaces/floor/13");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var spaces = await response.Content.ReadFromJsonAsync<List<SpaceDto>>();

        spaces.Should().NotBeNull();
        spaces.Should().NotBeEmpty();

        spaces!.All(s => s.Floor == 13).Should().BeTrue();
        spaces.Should().Contain(s => s.Name == "Clube Giro");
        spaces.Should().Contain(s => s.Name == "13-01");
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoSpacesAreBlocked()
    {
        // Act
        var response = await _client.GetAsync("/api/Spaces/maintenance");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var spaces = await response.Content.ReadFromJsonAsync<List<SpaceDto>>();

        spaces.Should().NotBeNull();

        spaces.Should().BeEmpty();
    }
}