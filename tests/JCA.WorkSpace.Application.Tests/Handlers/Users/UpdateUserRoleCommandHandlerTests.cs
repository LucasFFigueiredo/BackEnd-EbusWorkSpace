using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Handlers.Users;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

[Collection(nameof(UserFixtureCollection))]
public class UpdateUserRoleCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new UpdateUserRoleCommandHandler(
            _fixture.UserRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateRoleAndGenerateAuditLog_WhenUserExists()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var user = _fixture.GenerateValidUser(UserProfile.Employee);
        user.Id = targetUserId;
        
        var oldProfile = user.Profile;
        var newProfile = UserProfile.Manager;

        var command = new UpdateUserRoleCommand
        {
            AdminId = adminId,
            TargetUserId = targetUserId,
            NewProfile = newProfile
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.Profile.Should().Be(newProfile);

        _fixture.UserRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == adminId &&
            log.EntityId == user.Id &&
            log.Action.Contains("Perfil de Acesso") &&
            log.Details != null && log.Details.Contains(oldProfile.ToString()) && log.Details.Contains(newProfile.ToString())
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserRoleCommand
        {
            AdminId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            NewProfile = UserProfile.Admin
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.TargetUserId))
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
