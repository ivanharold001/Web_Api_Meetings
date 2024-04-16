using MediatR;
using Mettings.Domain.Enums;

namespace Mettings.Application.Gatherings.Commands.CreateGathering;

public sealed record CreateGatheringCommand(
    Guid MemberId,
    GatheringType Type,
    DateTime ScheduledAtUtc,
    string Name,
    string? Location,
    int? MaximumNumberOfAttendees,
    int? InvitationsValidBeforeInHours) : IRequest;