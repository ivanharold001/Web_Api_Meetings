using Mettings.Domain.Entities;

namespace Mettings.Domain.Repositories;

public interface IGatheringRepository
{
    Task<Gathering?> GetByIdAsync(Guid id, CancellationToken cancellationToken= default);

    Task<Gathering?> GetByIdWithCreatorAsync(Guid id, CancellationToken cancellationToken = default);

    void Add (Gathering gathering);
}