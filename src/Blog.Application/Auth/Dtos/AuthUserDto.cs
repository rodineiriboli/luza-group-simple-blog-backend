namespace Blog.Application.Auth.Dtos;

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}
