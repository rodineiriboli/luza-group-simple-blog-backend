using Blog.Domain.Entities;

namespace Blog.Application.Abstractions.Security;

public interface ITokenService
{
    DateTimeOffset GetExpirationDate();
    string GenerateToken(User user, DateTimeOffset expiresAt);
}
