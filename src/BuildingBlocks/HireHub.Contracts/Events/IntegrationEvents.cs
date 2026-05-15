namespace HireHub.Contracts.Events;

public record UserRegisteredEvent(Guid UserId, string Email, string Role, DateTime OccurredOnUtc);

public record JobCreatedEvent(Guid JobId, Guid CreatorUserId, string Title, string CompanyName, DateTime OccurredOnUtc);

public record ApplicationCreatedEvent(
    Guid ApplicationId,
    Guid JobId,
    Guid CandidateUserId,
    Guid? RecruiterUserId,
    DateTime OccurredOnUtc);

public record ApplicationUpdatedEvent(
    Guid ApplicationId,
    Guid CandidateUserId,
    string NewStatus,
    DateTime OccurredOnUtc);

public record MessageSentEvent(
    Guid ConversationId,
    Guid FromUserId,
    Guid ToUserId,
    Guid MessageId,
    string Preview,
    DateTime OccurredOnUtc);
