using ApplicationService.Domain;
using MediatR;

namespace ApplicationService.Application.Applications;

public sealed record ApplyToJobCommand(Guid CandidateUserId, Guid JobId, Guid? RecruiterUserId, string? CoverLetter) : IRequest<ApplyToJobResult>;

public sealed record ApplyToJobResult(bool Ok, string? Error, Guid? ApplicationId);

public sealed record UpdateApplicationStatusCommand(Guid ApplicationId, Guid RecruiterUserId, ApplicationStatus NewStatus) : IRequest<bool>;

public sealed record WithdrawApplicationCommand(Guid ApplicationId, Guid CandidateUserId) : IRequest<bool>;
