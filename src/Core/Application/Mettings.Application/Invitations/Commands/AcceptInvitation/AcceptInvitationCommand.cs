using MediatR;

namespace Mettings.Application.Invitations.Commands.AcceptInvitation;

public sealed record AcceptInvitationCommand(Guid InvitationId) : IRequest;