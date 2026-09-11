using FluentAssertions;
using JCA.WorkSpace.Application.Commands.Login;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using Moq;
using JCA.WorkSpace.Application.Handlers.Login;

namespace JCA.WorkSpace.Application.Tests.Handlers.Users;

[Collection(nameof(UserFixtureCollection))]
public class LoginGoogleCommandHandlerTests
{
    private readonly UserTestsFixture _fixture;
    private readonly LoginGoogleCommandHandler _handler;

    public LoginGoogleCommandHandlerTests(UserTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();

        _handler = new LoginGoogleCommandHandler(
            _fixture.UserRepositoryMock.Object,
            _fixture.UnitOfWorkMock.Object,
            _fixture.ConfigurationMock.Object,
            _fixture.MapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedExceptionAndNotCommit_WhenTokenIsInvalid()
    {
        // Arrange
        var command = new LoginGoogleCommand
        {
            GoogleToken = "TOKEN_INVALIDO"
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Token do Google inv*lido ou expirado.");

        _fixture.UnitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Never);
        _fixture.UserRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _fixture.UserRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
