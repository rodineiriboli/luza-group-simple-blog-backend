namespace Blog.Application.Auth.Dtos;

public sealed class AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public AuthUserDto User { get; init; } = null!;
}
