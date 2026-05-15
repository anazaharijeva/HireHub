using System.Security.Claims;
using MessagingService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public sealed class MessagesController : ControllerBase
{
    private readonly IMessagingService _messaging;

    public MessagesController(IMessagingService messaging) => _messaging = messaging;

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing sub claim"));

    [HttpGet("conversations")]
    public async Task<IActionResult> Conversations(CancellationToken ct) =>
        Ok(await _messaging.ListConversationsAsync(UserId, ct).ConfigureAwait(false));

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> Thread(Guid id, CancellationToken ct) =>
        Ok(await _messaging.GetMessagesAsync(id, UserId, ct).ConfigureAwait(false));

    public sealed record SendRequest(Guid ToUserId, string Body);

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendRequest request, CancellationToken ct) =>
        Ok(await _messaging.SendAsync(UserId, request.ToUserId, request.Body, ct).ConfigureAwait(false));

    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, CancellationToken ct)
    {
        await _messaging.MarkReadAsync(id, UserId, ct).ConfigureAwait(false);
        return NoContent();
    }
}
