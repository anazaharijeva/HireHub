using ApplicationService.Application.Abstractions;
using ApplicationService.Domain;
using ApplicationService.Domain.Entities;
using HireHub.Contracts.Events;
using HireHub.Contracts.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationService.Application.Applications;

public sealed class ApplyToJobCommandHandler : IRequestHandler<ApplyToJobCommand, ApplyToJobResult>
{
    private readonly IApplicationsRepository _repo;
    private readonly IIntegrationEventPublisher _events;
    private readonly ILogger<ApplyToJobCommandHandler> _logger;

    public ApplyToJobCommandHandler(IApplicationsRepository repo, IIntegrationEventPublisher events, ILogger<ApplyToJobCommandHandler> logger)
    {
        _repo = repo;
        _events = events;
        _logger = logger;
    }

    public async Task<ApplyToJobResult> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        if (await _repo.ExistsForJobAndCandidateAsync(request.JobId, request.CandidateUserId, cancellationToken).ConfigureAwait(false))
            return new ApplyToJobResult(false, "Already applied to this job.", null);

        var app = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = request.JobId,
            CandidateUserId = request.CandidateUserId,
            RecruiterUserId = request.RecruiterUserId,
            Status = ApplicationStatus.Applied,
            CoverLetter = request.CoverLetter,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        _repo.Add(app);
        await _repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _events.PublishAsync(
                    new ApplicationCreatedEvent(app.Id, app.JobId, app.CandidateUserId, app.RecruiterUserId, DateTime.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish ApplicationCreatedEvent");
        }

        return new ApplyToJobResult(true, null, app.Id);
    }
}
