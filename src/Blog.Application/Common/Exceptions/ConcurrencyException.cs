namespace Blog.Application.Common.Exceptions;

public sealed class ConcurrencyException : AppException
{
    public ConcurrencyException(string message)
        : base(message)
    {
    }
}
