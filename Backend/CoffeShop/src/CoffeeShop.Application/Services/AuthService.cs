using CoffeeShop.Application.Configuration;
using CoffeeShop.Application.DTOs.Auth;
using CoffeeShop.Application.Interfaces;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Exceptions;
using CoffeeShop.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace CoffeeShop.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminSetupSettings _adminSetup;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IOptions<AdminSetupSettings> adminSetup)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _adminSetup = adminSetup.Value;
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => CreateUserAsync(request.FullName, request.Email, request.Password, UserRole.Customer, cancellationToken);

    public async Task<AuthResponse> RegisterAdminAsync(RegisterAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.SetupSecretKey, _adminSetup.SetupSecretKey, StringComparison.Ordinal))
        {
            throw new UnauthorizedException("Invalid admin setup secret key.");
        }

        return await CreateUserAsync(
            request.FullName,
            request.Email,
            request.Password,
            UserRole.Admin,
            cancellationToken);
    }

    public async Task<AuthResponse> BootstrapFirstAdminAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.AnyAdminExistsAsync(cancellationToken))
        {
            throw new BusinessException(
                "An admin user already exists. Login as admin or use POST /api/auth/register-admin with SetupSecretKey.");
        }

        return await CreateUserAsync(
            request.FullName,
            request.Email,
            request.Password,
            UserRole.Admin,
            cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var tokens = _tokenService.CreateTokens(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiry = tokens.RefreshTokenExpiry;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToAuthResponse(user, tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var tokens = _tokenService.CreateTokens(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiry = tokens.RefreshTokenExpiry;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToAuthResponse(user, tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateUserAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Users.ExistsAsync(u => u.Email == normalizedEmail, cancellationToken))
        {
            throw new BusinessException("Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var tokens = _tokenService.CreateTokens(user);
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiry = tokens.RefreshTokenExpiry;

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToAuthResponse(user, tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiry);
    }

    private static AuthResponse MapToAuthResponse(User user, string accessToken, string refreshToken, DateTime refreshExpiry)
    {
        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshExpiry
        };
    }
}
