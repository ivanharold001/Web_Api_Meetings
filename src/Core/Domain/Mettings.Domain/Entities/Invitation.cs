using Mettings.Domain.Enums;

namespace Mettings.Domain.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public Guid GatheringId { get; set; }
    public Guid MemberId { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ModifiedOnUtC { get; set; }
}