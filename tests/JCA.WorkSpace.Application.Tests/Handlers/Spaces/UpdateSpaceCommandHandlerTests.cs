using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Application.Handlers.Spaces;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Spaces;

[Collection(nameof(SpaceFixtureCollection))]
public class UpdateSpaceCommandHandlerTests
{
    private readonly SpaceTestsFixture _fixture;
    private readonly UpdateSpaceCommandHandler _handler;

    public UpdateSpaceCommandHandlerTests(SpaceTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new UpdateSpaceCommandHandler(
            _fixture.SpaceRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var space = _fixture.GenerateValidRoom();
        var command = new UpdateSpaceCommand
        {
            Id = space.Id,
            Name = "Sala de Reunião Editada",
            Type = SpaceType.Room,
            Floor = 4,
            Capacity = 12,
            Sector = "TI",
            Resources = "TV",
            RequiresApproval = true
        };

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.Id))
            .ReturnsAsync(space);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        space.Name.Should().Be(command.Name);
        space.Type.Should().Be(command.Type);
        space.Floor.Should().Be(command.Floor);
        space.Capacity.Should().Be(command.Capacity);
        space.Sector.Should().Be(command.Sector);
        space.Resources.Should().Be(command.Resources);
        space.RequiresApproval.Should().Be(command.RequiresApproval);

        _fixture.SpaceRepositoryMock.Verify(repo => repo.UpdateAsync(space), Times.Once);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenSpaceIsNotFound()
    {
        // Arrange
        var command = new UpdateSpaceCommand
        {
            Id = Guid.NewGuid(),
            Name = "Sala Fake"
        };

        _fixture.SpaceRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.Id))
            .ReturnsAsync((Space?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _fixture.SpaceRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Space>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
    }
}
