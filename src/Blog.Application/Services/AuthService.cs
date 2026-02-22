using Blog.Application.Abstractions.Persistence;
using Blog.Application.Abstractions.Security;
using Blog.Application.Auth.Commands;
using Blog.Application.Auth.Dtos;
using Blog.Application.Common.Exceptions;
using Blog.Application.Services.Interfaces;
using Blog.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Blog.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<User>();
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new BadRequestException("Email already in use.");
        }

        var user = User.Create(normalizedEmail, request.DisplayName);
        var hash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(hash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var expiresAt = _tokenService.GetExpirationDate();
        var token = _tokenService.GenerateToken(user, expiresAt);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return BuildResponse(user, token, expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult is PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var expiresAt = _tokenService.GetExpirationDate();
        var token = _tokenService.GenerateToken(user, expiresAt);

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return BuildResponse(user, token, expiresAt);
    }

    private static AuthResponse BuildResponse(User user, string token, DateTimeOffset expiresAt)
    {
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            }
        };
    }
}
