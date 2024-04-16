namespace Mettings.Domain.Entities;

public class Attendee
{
    public Guid GatheringId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime CreateOnUtc { get; set; }
}
