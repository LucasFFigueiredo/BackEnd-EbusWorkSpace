using FluentAssertions;
using JCA.WorkSpace.Application.Queries.Spaces;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;
using JCA.WorkSpace.Application.Dtos.Spaces;
using JCA.WorkSpace.Application.Handlers.Spaces;

namespace JCA.WorkSpace.Application.Tests.Handlers.Spaces;

[Collection(nameof(SpaceFixtureCollection))]
public class GetAvailableSpacesQueryHandlerTests
{
    private readonly SpaceTestsFixture _fixture;
    private readonly GetAvailableSpacesQueryHandler _handler;

    public GetAvailableSpacesQueryHandlerTests(SpaceTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new GetAvailableSpacesQueryHandler(
            _fixture.SpaceRepositoryMock.Object,
            _fixture.MapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableSpaces_WhenCalled()
    {
        // Arrange
        var query = new GetAvailableSpacesQuery
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Type = SpaceType.Room
        };

        var availableSpaces = new List<Space>
        {
            _fixture.GenerateValidRoom(isBlocked: false),
            _fixture.GenerateValidRoom(isBlocked: false)
        };

        var expectedDtos = new List<SpaceDto>
        {
            new SpaceDto { Id = availableSpaces[0].Id, Name = availableSpaces[0].Name },
            new SpaceDto { Id = availableSpaces[1].Id, Name = availableSpaces[1].Name }
        };

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetAvailableAsync(query.StartTime, query.EndTime, query.Type))
            .ReturnsAsync(availableSpaces);

        _fixture.MapperMock
            .Setup(m => m.Map<IEnumerable<SpaceDto>>(availableSpaces))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedDtos);

        _fixture.SpaceRepositoryMock.Verify(repo => repo.GetAvailableAsync(
            query.StartTime,
            query.EndTime,
            query.Type
        ), Times.Once);
    }
}
