using MediatR;
using Mettings.Application.Abstraction;
using Mettings.Domain.Entities;
using Mettings.Domain.Enums;
using Mettings.Domain.Repositories;

namespace Mettings.Application.Invitations.Commands.SendInvitation;

internal sealed class SendInvitationCommandHandler : IRequestHandler<SendInvitationCommand>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGatheringRepository _gatheringRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public SendInvitationCommandHandler(
        IMemberRepository memberRepository,
        IGatheringRepository gatheringRepository,
        IInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _memberRepository = memberRepository;
        _gatheringRepository = gatheringRepository;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByAsync(request.MemberId, cancellationToken);

        var gathering = await _gatheringRepository.GetByIdWithCreatorAsync(request.GatheringId, cancellationToken);

        if (member is null || gathering is null) return Unit.Value;

        if (gathering.Creator.Id == member.Id)
        {
            throw new Exception("Can't send invitation to the gathering creator.");
        }

        if (gathering.ScheduledAtUtc < DateTime.UtcNow)
        {
            throw new Exception("can't send invitation for gathering int the past.");
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            GatheringId = gathering.Id,
            Status = InvitationStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow
        };

        gathering.Invitations.Add(invitation);

        _invitationRepository.Add(invitation);

        await _unitOfWork.SaveChangeAsync(cancellationToken);

        await _emailService.SendInvitationSentEmailAsync(member, gathering, cancellationToken);

        return Unit.Value;
    }
}