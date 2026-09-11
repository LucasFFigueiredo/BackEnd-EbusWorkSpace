using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Handlers.Users;
using JCA.WorkSpace.Domain.Entities;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

[Collection(nameof(UserFixtureCollection))]
public class UpdateUserSectorCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly UpdateUserSectorCommandHandler _handler;

    public UpdateUserSectorCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new UpdateUserSectorCommandHandler(
            _fixture.UserRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateSectorAndGenerateAuditLog_WhenUserExists()
    {
        // Arrange
        var user = _fixture.GenerateValidUser();
        var oldSector = user.Sector;
        
        var command = new UpdateUserSectorCommand
        {
            UserId = user.Id,
            Sector = "Novo Setor de Inovacao"
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.Sector.Should().Be(command.Sector);

        _fixture.UserRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == user.Id &&
            log.EntityId == user.Id &&
            log.Action.Contains("Setor (Onboarding)") &&
            log.Details != null && log.Details.Contains(oldSector ?? "Nenhum") && log.Details.Contains(command.Sector)
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserSectorCommand
        {
            UserId = Guid.NewGuid(),
            Sector = "Setor Inválido"
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Usu*rio n*o encontrado.");

        _fixture.UserRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
    }
}
