using Mettings.Domain.Entities;

namespace Mettings.Domain.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByAsync(Guid id, CancellationToken cancellationToken = default);
}