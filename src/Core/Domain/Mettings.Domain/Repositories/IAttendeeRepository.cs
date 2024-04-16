using Mettings.Domain.Entities;

namespace Mettings.Domain.Repositories;

public interface IAttendeeRepository
{
    void Add(Attendee attendee);
}