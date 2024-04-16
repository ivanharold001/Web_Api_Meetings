using MediatR;
using Mettings.Domain.Entities;
using Mettings.Domain.Enums;
using Mettings.Domain.Repositories;

namespace Mettings.Application.Gatherings.Commands.CreateGathering;

internal sealed class CreateGatheringCommandHandler : IRequestHandler<CreateGatheringCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGatheringRepository _gatheringRepository;

    private readonly IUnitOfWork _unitOfWork;

    public CreateGatheringCommandHandler(
        IMemberRepository memberRepository,
        IGatheringRepository gatheringRepository,
        IUnitOfWork unitOfWork
    )
    {
        _memberRepository = memberRepository;
        _gatheringRepository = gatheringRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<Unit> Handle(CreateGatheringCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByAsync(request.MemberId, cancellationToken);

        if (member is null) return Unit.Value;

        var gathering = new Gathering
        {
            Id = Guid.NewGuid(),
            Creator = member,
            Type = request.Type,
            Name = request.Name,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Location = request.Location,
        };

        switch (gathering.Type)
        {
            case GatheringType.WithFixedNumberOfAttendees:
                if (request.MaximumNumberOfAttendees is null)
                {
                    throw new Exception($"{nameof(request.MaximumNumberOfAttendees)} can't be null.");
                }

                gathering.MaximumNumberOfAttendees = request.MaximumNumberOfAttendees;
                break;
            case GatheringType.WithExpirationForInvitations:
                if (request.InvitationsValidBeforeInHours is null)
                {
                    throw new Exception($"{nameof(request.InvitationsValidBeforeInHours)} can't be null.");
                }

                gathering.InvitationsExpireAtUtc = gathering.ScheduledAtUtc.AddHours(-request.InvitationsValidBeforeInHours.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(GatheringType));
        }

        _gatheringRepository.Add(gathering);

        await _unitOfWork.SaveChangeAsync(cancellationToken);

        return Unit.Value;
    }
}