using AuthService.Application.Abstractions;
using AuthService.Application.Dtos;
using AuthService.Application.Options;
using AuthService.Application.Security;
using AuthService.Domain;
using AuthService.Domain.Entities;
using HireHub.Contracts.Events;
using HireHub.Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Services;

public interface IAuthAccountService
{
    Task<(bool Ok, string? Error, AuthTokensResponse? Tokens, UserSummaryDto? User)> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<(bool Ok, string? Error, AuthTokensResponse? Tokens, UserSummaryDto? User)> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<(bool Ok, string? Error, AuthTokensResponse? Tokens)> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(RefreshRequest request, CancellationToken ct);
}

public sealed class AuthAccountService : IAuthAccountService
{
    private readonly IAuthDb _db;
    private readonly IJwtTokenFactory _jwt;
    private readonly IIntegrationEventPublisher _events;
    private readonly ILogger<AuthAccountService> _logger;
    private readonly IOptions<JwtOptions> _jwtOptions;

    public AuthAccountService(
        IAuthDb db,
        IJwtTokenFactory jwt,
        IIntegrationEventPublisher events,
        ILogger<AuthAccountService> logger,
        IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwt = jwt;
        _events = events;
        _logger = logger;
        _jwtOptions = jwtOptions;
    }

    public async Task<(bool Ok, string? Error, AuthTokensResponse? Tokens, UserSummaryDto? User)> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _db.FindUserByEmailAsync(normalizedEmail, ct).ConfigureAwait(false) is not null)
            return (false, "Email already registered.", null, null);

        var roleName = request.Role.Trim();
        var role = await _db.FindRoleByNameAsync(roleName, ct).ConfigureAwait(false);
        if (role is null)
            return (false, "Invalid role.", null, null);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            EmailVerified = false,
            CreatedUtc = DateTime.UtcNow,
            UserRoles = new List<AppUserRole> { new() { RoleId = role.Id, Role = role } }
        };

        _db.AddUser(user);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var tokens = await IssueTokensAsync(user.Id, roles, ct).ConfigureAwait(false);

        try
        {
            await _events.PublishAsync(new UserRegisteredEvent(user.Id, user.Email, roles[0], DateTime.UtcNow), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish UserRegisteredEvent");
        }

        return (true, null, tokens, new UserSummaryDto(user.Id, user.Email, roles));
    }

    public async Task<(bool Ok, string? Error, AuthTokensResponse? Tokens, UserSummaryDto? User)> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.FindUserByEmailAsync(normalizedEmail, ct).ConfigureAwait(false);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, "Invalid credentials.", null, null);

        user = await _db.FindUserWithRolesAsync(user.Id, ct).ConfigureAwait(false) ?? user;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var tokens = await IssueTokensAsync(user.Id, roles, ct).ConfigureAwait(false);
        return (true, null, tokens, new UserSummaryDto(user.Id, user.Email, roles));
    }

    public async Task<(bool Ok, string? Error, AuthTokensResponse? Tokens)> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var rt = await _db.FindRefreshTokenAsync(request.RefreshToken, ct).ConfigureAwait(false);
        if (rt is null || rt.RevokedUtc is not null || rt.ExpiresUtc < DateTime.UtcNow)
            return (false, "Invalid refresh token.", null);

        var user = await _db.FindUserWithRolesAsync(rt.UserId, ct).ConfigureAwait(false);
        if (user is null)
            return (false, "User not found.", null);

        rt.RevokedUtc = DateTime.UtcNow;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var tokens = await IssueTokensAsync(user.Id, roles, ct).ConfigureAwait(false);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return (true, null, tokens);
    }

    public async Task LogoutAsync(RefreshRequest request, CancellationToken ct)
    {
        var rt = await _db.FindRefreshTokenAsync(request.RefreshToken, ct).ConfigureAwait(false);
        if (rt is null)
            return;
        rt.RevokedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<AuthTokensResponse> IssueTokensAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var user = await _db.FindUserWithRolesAsync(userId, ct).ConfigureAwait(false)
                   ?? throw new InvalidOperationException("User not found for token issuance.");
        var (access, accessExp) = _jwt.CreateAccessToken(user.Id, user.Email, roles);
        var refreshValue = JwtTokenFactory.CreateRefreshTokenValue();
        var refreshDays = _jwtOptions.Value.RefreshTokenDays;
        var refresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshValue,
            ExpiresUtc = DateTime.UtcNow.AddDays(refreshDays)
        };
        _db.AddRefreshToken(refresh);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return new AuthTokensResponse(access, refreshValue, accessExp, refresh.ExpiresUtc);
    }
}
