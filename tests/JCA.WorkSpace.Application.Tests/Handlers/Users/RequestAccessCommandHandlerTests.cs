using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Users;
using JCA.WorkSpace.Application.Handlers.Users;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

[Collection(nameof(UserFixtureCollection))]
public class RequestAccessCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly RequestAccessCommandHandler _handler;
    private readonly Mock<ILogger<RequestAccessCommandHandler>> _loggerMock;

    public RequestAccessCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
        _loggerMock = new Mock<ILogger<RequestAccessCommandHandler>>();

        _handler = new RequestAccessCommandHandler(
            _fixture.UserRepositoryMock.Object,
            _fixture.AuditLogRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRegisterJustificationAndRequestedProfile_WhenValid()
    {
        // Arrange
        var user = _fixture.GenerateValidUser(UserProfile.Employee);
        var command = new RequestAccessCommand
        {
            UserId = user.Id,
            RequestedProfile = UserProfile.Manager,
            Justification = "Preciso acessar os relatorios de ocupacao."
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _fixture.AuditLogRepositoryMock.Verify(repo => repo.AddAsync(It.Is<AuditLog>(log => 
            log.UserId == user.Id &&
            log.EntityId == user.Id &&
            log.Action.Contains("Acesso Enviada") &&
            log.Details != null && log.Details.Contains(command.RequestedProfile.ToString()) && log.Details.Contains(command.Justification)
        )), Times.Once);

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRequestedProfileIsSameAsCurrentProfile()
    {
        // Arrange
        var user = _fixture.GenerateValidUser(UserProfile.Manager);
        var command = new RequestAccessCommand
        {
            UserId = user.Id,
            RequestedProfile = UserProfile.Manager,
            Justification = "Tentativa inválida de pedir mesmo nível."
        };

        _fixture.UserRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert (Este teste falhará na implementação atual se o handler não validar, o que garante a prática de TDD solicitada)
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*este n*vel*");
    }
}
