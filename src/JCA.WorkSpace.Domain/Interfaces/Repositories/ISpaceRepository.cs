using JCA.WorkSpace.Domain.Entities;
using JCA.WorkSpace.Domain.Enums;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface ISpaceRepository : IRepository<Space>
{
    Task<IEnumerable<Space>> GetByFloorAsync(int floor);
    Task<IEnumerable<Space>> GetAvailableAsync(DateTime start, DateTime end, SpaceType type);
    Task SetMaintenanceAsync(Guid id, bool isBlocked, string? reason, DateOnly? until);
    Task<IEnumerable<Space>> GetInMaintenanceAsync();
}