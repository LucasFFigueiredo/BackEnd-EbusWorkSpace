using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;
using JCA.WorkSpace.Domain.Interfaces;
using JCA.WorkSpace.Domain.Interfaces.Repositories;
using Moq;

namespace JCA.WorkSpace.Application.Tests.Handlers;

public class ReservationTestsFixture
{
    public Mock<IReservationRepository> ReservationRepositoryMock { get; set; }
    public Mock<ISpaceRepository> SpaceRepositoryMock { get; set; }
    public Mock<IAuditLogRepository> AuditLogRepositoryMock { get; set; }
    public Mock<IUnitOfWork> UnitOfWorkMock { get; set; }

    public ReservationTestsFixture()
    {
        ReservationRepositoryMock = new Mock<IReservationRepository>();
        SpaceRepositoryMock = new Mock<ISpaceRepository>();
        AuditLogRepositoryMock = new Mock<IAuditLogRepository>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public void ResetMocks()
    {
        ReservationRepositoryMock.Reset();
        SpaceRepositoryMock.Reset();
        AuditLogRepositoryMock.Reset();
        UnitOfWorkMock.Reset();

        UnitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);
    }

    public Space GenerateValidSpace(bool requiresApproval = false, bool isBlocked = false)
    {
        return new Space
        {
            Id = Guid.NewGuid(),
            Name = "Sala Teste",
            Type = SpaceType.Room,
            Floor = 2,
            Capacity = 6,
            IsBlocked = isBlocked,
            RequiresApproval = requiresApproval,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Reservation GenerateValidReservation(Guid userId, Guid spaceId, ReservationStatus status = ReservationStatus.Pending)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SpaceId = spaceId,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}

[CollectionDefinition(nameof(ReservationFixtureCollection))]
public class ReservationFixtureCollection : ICollectionFixture<ReservationTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
