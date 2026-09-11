using JCA.WorkSpace.Domain.Entities;

namespace JCA.WorkSpace.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task UpdateLastLoginAsync(Guid id);
}