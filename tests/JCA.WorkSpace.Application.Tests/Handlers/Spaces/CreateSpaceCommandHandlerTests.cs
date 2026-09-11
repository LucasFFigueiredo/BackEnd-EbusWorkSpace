using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Spaces;
using JCA.WorkSpace.Application.Handlers.Spaces;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Spaces;

[Collection(nameof(SpaceFixtureCollection))]
public class CreateSpaceCommandHandlerTests
{
    private readonly SpaceTestsFixture _fixture;
    private readonly CreateSpaceCommandHandler _handler;

    public CreateSpaceCommandHandlerTests(SpaceTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new CreateSpaceCommandHandler(
            _fixture.SpaceRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateSpaceCorrectlyAndReturnGuid_WhenCalled()
    {
        // Arrange
        var command = new CreateSpaceCommand
        {
            Name = "Nova Sala de Reunião",
            Type = SpaceType.Room,
            Floor = 3,
            Capacity = 10,
            Sector = "Marketing",
            Resources = "Projetor, TV",
            RequiresApproval = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _fixture.SpaceRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Space>(s => 
            s.Name == command.Name &&
            s.Type == command.Type &&
            s.Floor == command.Floor &&
            s.Capacity == command.Capacity &&
            s.Sector == command.Sector &&
            s.Resources == command.Resources &&
            s.RequiresApproval == command.RequiresApproval &&
            s.IsBlocked == false
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }
}
