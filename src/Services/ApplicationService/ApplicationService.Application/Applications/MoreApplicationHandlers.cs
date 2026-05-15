using ApplicationService.Application.Abstractions;
using ApplicationService.Domain;
using HireHub.Contracts.Events;
using HireHub.Contracts.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationService.Application.Applications;

public sealed class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, bool>
{
    private readonly IApplicationsRepository _repo;
    private readonly IIntegrationEventPublisher _events;
    private readonly ILogger<UpdateApplicationStatusCommandHandler> _logger;

    public UpdateApplicationStatusCommandHandler(IApplicationsRepository repo, IIntegrationEventPublisher events, ILogger<UpdateApplicationStatusCommandHandler> logger)
    {
        _repo = repo;
        _events = events;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var app = await _repo.GetTrackedAsync(request.ApplicationId, cancellationToken).ConfigureAwait(false);
        if (app is null || app.RecruiterUserId != request.RecruiterUserId)
            return false;

        app.Status = request.NewStatus;
        app.UpdatedUtc = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _events.PublishAsync(
                    new ApplicationUpdatedEvent(app.Id, app.CandidateUserId, app.Status.ToString(), DateTime.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish ApplicationUpdatedEvent");
        }

        return true;
    }
}

public sealed class WithdrawApplicationCommandHandler : IRequestHandler<WithdrawApplicationCommand, bool>
{
    private readonly IApplicationsRepository _repo;

    public WithdrawApplicationCommandHandler(IApplicationsRepository repo) => _repo = repo;

    public async Task<bool> Handle(WithdrawApplicationCommand request, CancellationToken cancellationToken)
    {
        var app = await _repo.GetTrackedAsync(request.ApplicationId, cancellationToken).ConfigureAwait(false);
        if (app is null || app.CandidateUserId != request.CandidateUserId)
            return false;

        _repo.Remove(app);
        await _repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
