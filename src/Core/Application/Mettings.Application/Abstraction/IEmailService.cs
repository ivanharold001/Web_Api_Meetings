using Mettings.Domain.Entities;

namespace Mettings.Application.Abstraction;

public interface IEmailService
{
    Task SendInvitationSentEmailAsync(Member member, Gathering gathering, CancellationToken cancellationToken = default);

    Task SendInvitationAcceptedEmailAsync(Gathering gathering, CancellationToken cancellationToken = default);
}