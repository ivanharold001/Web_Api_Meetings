using MediatR;
using Mettings.Application.Abstraction;
using Mettings.Domain.Repositories;
using Mettings.Domain.Enums;
using Mettings.Domain.Entities;

namespace Mettings.Application.Invitations.Commands.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IGatheringRepository _gatheringRepository;
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public AcceptInvitationCommandHandler(
        IInvitationRepository invitationRepository,
        IMemberRepository memberRepository,
        IGatheringRepository gatheringRepository,
        IAttendeeRepository attendeeRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService
    )
    {
        _invitationRepository = invitationRepository;
        _memberRepository = memberRepository;
        _gatheringRepository = gatheringRepository;
        _attendeeRepository = attendeeRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);

        if (invitation is null ||
            invitation.Status != InvitationStatus.Pending) return Unit.Value;

        var member = await _memberRepository.GetByAsync(invitation.MemberId, cancellationToken);

        var gathering = await _gatheringRepository.GetByIdWithCreatorAsync(invitation.GatheringId, cancellationToken);

        if (member is null || gathering is null) return Unit.Value;

        var expired = (gathering.Type == GatheringType.WithFixedNumberOfAttendees &&
                        gathering.NumberOfAttendees < gathering.MaximumNumberOfAttendees) ||
                        (gathering.Type == GatheringType.WithExpirationForInvitations &&
                        gathering.InvitationsExpireAtUtc < DateTime.UtcNow);

        if (expired)
        {
            invitation.Status = InvitationStatus.Expired;
            invitation.ModifiedOnUtC = DateTime.UtcNow;
        }
        else
        {
            invitation.Status = InvitationStatus.Accepted;
            invitation.ModifiedOnUtC = DateTime.UtcNow;
        }

        var attendee = new Attendee
        {
            MemberId = invitation.MemberId,
            GatheringId = invitation.GatheringId,
            CreateOnUtc = DateTime.UtcNow,
        };

        gathering.Attendees.Add(attendee);
        gathering.NumberOfAttendees++;

        _attendeeRepository.Add(attendee);

        await _unitOfWork.SaveChangeAsync(cancellationToken);

        if (invitation.Status == InvitationStatus.Accepted)
        {
            await _emailService.SendInvitationAcceptedEmailAsync(gathering, cancellationToken);
        }

        return Unit.Value;
    }
}