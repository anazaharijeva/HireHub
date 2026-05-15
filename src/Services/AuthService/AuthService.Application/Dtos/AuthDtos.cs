namespace AuthService.Application.Dtos;

public sealed record RegisterRequest(string Email, string Password, string Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresUtc, DateTime RefreshTokenExpiresUtc);

public sealed record UserSummaryDto(Guid Id, string Email, IReadOnlyList<string> Roles);
