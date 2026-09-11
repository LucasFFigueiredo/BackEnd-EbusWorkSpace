using AutoMapper;
using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers.Spaces;

public class SpaceTestsFixture
{
    public Mock<ISpaceRepository> SpaceRepositoryMock { get; set; }
    public Mock<IAuditLogRepository> AuditLogRepositoryMock { get; set; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; set; }
    public Mock<IMapper> MapperMock { get; set; }

    public SpaceTestsFixture()
    {
        SpaceRepositoryMock = new Mock<ISpaceRepository>();
        AuditLogRepositoryMock = new Mock<IAuditLogRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        MapperMock = new Mock<IMapper>();
    }

    public void ResetMocks()
    {
        SpaceRepositoryMock.Reset();
        AuditLogRepositoryMock.Reset();
        UnitOfWorkMock.Reset();
        MapperMock.Reset();

        UnitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);
    }

    public Space GenerateValidRoom(bool requiresApproval = false, bool isBlocked = false)
    {
        return new Space
        {
            Id = Guid.NewGuid(),
            Name = "Sala de Reunião Teste",
            Type = SpaceType.Room,
            Floor = 2,
            Capacity = 6,
            IsBlocked = isBlocked,
            RequiresApproval = requiresApproval,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Space GenerateValidDesk(bool isBlocked = false)
    {
        return new Space
        {
            Id = Guid.NewGuid(),
            Name = "Mesa 2-01 Teste",
            Type = SpaceType.Desk,
            Floor = 2,
            Capacity = 1,
            IsBlocked = isBlocked,
            RequiresApproval = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}

[CollectionDefinition(nameof(SpaceFixtureCollection))]
public class SpaceFixtureCollection : ICollectionFixture<SpaceTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
