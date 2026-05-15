using AuthService.Application.Dtos;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthAccountService _auth;

    public AuthController(IAuthAccountService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var (ok, error, tokens, user) = await _auth.RegisterAsync(request, ct).ConfigureAwait(false);
        if (!ok)
            return BadRequest(new { error });
        return Ok(new { user, tokens });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var (ok, error, tokens, user) = await _auth.LoginAsync(request, ct).ConfigureAwait(false);
        if (!ok)
            return Unauthorized(new { error });
        return Ok(new { user, tokens });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var (ok, error, tokens) = await _auth.RefreshAsync(request, ct).ConfigureAwait(false);
        if (!ok)
            return Unauthorized(new { error });
        return Ok(new { tokens });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request, ct).ConfigureAwait(false);
        return NoContent();
    }
}
